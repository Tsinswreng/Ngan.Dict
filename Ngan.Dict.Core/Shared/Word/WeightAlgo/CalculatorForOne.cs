namespace Ngaq.Core.Shared.Word.WeightAlgo;

using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Shared.Word.WeightAlgo.Models;
using Ngaq.Core.Word.WeightAlgo.Models;
using Tsinswreng.CsTempus;
using Tsinswreng.CsTools;


[Doc(@$"針對單個單詞計算權重的算法執行器。

整體思路:
1. 先按時間順序遍歷該單詞的 LearnRecords。
2. 遇到 add/rmb/fgt 三類事件時，分別按不同規則調整權重。
3. 在遍歷過程中維護一份 WeightWordState，承載「目前遍歷到哪」以及各類前綴計數。
4. 最後再根據「最後一個事件是什麼」做一次尾部修正，讓當前狀態更符合業務直覺。

設計目標不是做精確的記憶曲線擬合，而是做「事件流驅動的優先級排序」:
- 重要詞(被多次加入)應更容易浮到前面；
- 剛忘掉的詞應更快回來；
- 剛記住的詞應暫時往後沉；
- 剛加入的新詞應在近期內有更強曝光。
")]
public partial class CalculatorForOne{
	public DfltWeightCfg Cfg{get;set;} = new();

	[Doc(@$"算法執行器的內部狀態容器。

	把輸入單詞與運行時狀態綁在一起，是爲了讓 CalculatorForOne 的各個私有方法
	都只關注「當前上下文」，不用把 Word 和 WordState 當參數層層傳遞。
	")]
	public partial class State_{
		/// 正在計算的目標單詞。
		public IWeightWord Word{get;set;} = null!;
		/// 本次計算過程中的可變運行時狀態。
		public WeightWordState WordState{get;set;} = new();
	}
	public State_ State{get;set;} = new();
	public IWeightWord Word{
		get{return State.Word;}
	}
	public WeightWordState WordState{
		get{return State.WordState;}
	}

	[Doc(@$"初始化算法器，綁定待計算單詞，並預先計算某些會被反覆使用的輔助信息。

	注意:
	- 此方法會重置 State，因此同一個 CalculatorForOne 實例在切換單詞時是「覆蓋式復用」。
	- Init 後應再調用 Run/RunAsy，才會真正把權重算出來。
	")]
	public nil Init(IWeightWord Word){
		State = new();
		State.Word = Word;
		_PreCount();
		return NIL;
	}

	/// 目前僅作調試佔位。日後若接入日誌系統，可在此統一格式化算法過程輸出。
	public str Log(str s){
		return s;
	}

	[Doc(@$"預先算好必要數據")]
	nil _PreCount(){
		_FindFinalAddPos();
		return NIL;
	}

	[Doc(@$"初始化{nameof(WordState.PosFinalAdd)}")]
	nil _FindFinalAddPos(){
		for(var i = Word.LearnRecords.Count-1; i>=0; i--){
			var LearnRecord = Word.LearnRecords[i];
			if(LearnRecord.Learn == ELearn.Add){
				WordState.PosFinalAdd = (u64)i;
				break;
			}
		}
		return NIL;
	}

	[Doc(@$"異步入口。

	目前實現上仍是同步計算，直接返回 Run() 的結果。
	保留 async 形態主要是爲了和外層可能的批量並發接口保持一致，
	將來若計算過程需要真正異步化，可在不改調用方接口的前提下演進。
	")]
	public async Task<nil> RunAsy(CT Ct){
		//return Task.Run(Run, Ct);
		return Run();
	}

	[Doc("計算當前單詞的權重 並賦值")]
	public nil Run(){
		var Word = State.Word;
		//Word.SavedLearnRecords //TODO 確保此潙以時升序
		_ = @$"按時間順序遍歷所有事件。
		這裡的核心假設是: LearnRecords 已按時間升序排列。
		若順序錯亂，則「前一事件」「最後一個 add」「時間差」等語義都會被破壞，
		整套權重算法將不再可靠。";
		for(var i = 0; i < Word.LearnRecords.Count; i++, State.WordState.Pos++){
			var LearnRecord = Word.LearnRecords[i];
			//根據當前事件 做相應處理。三類事件對權重的影響方向不同:
			//add: 提高基礎重要性
			//rmb: 降低近期再次出現概率
			//fgt: 提高近期再次出現概率
			if(LearnRecord.Learn == ELearn.Add){
				_Add();
			}else if(LearnRecord.Learn == ELearn.Rmb){
				_Rmb();
			}else if(LearnRecord.Learn == ELearn.Fgt){
				_Fgt();
			}
			//如果是處理最後一個事件 再做額外處理
			if(i == Word.LearnRecords.Count-1){
				_HandleFinal();
			}
		}
		return NIL;
	}

	[Doc(@$"處理 添加事件。
	「添加」指一個單詞被加入詞庫。
	一個單詞能被多次加入詞庫。「添加」事件越多代表這個單詞越重要。故「添加」事件會使權重變大

	設計重點:
	1. add 表達的是「此詞值得被納入學習系統」的業務重要性，而不是記憶成敗。
	2. 因此它的增益通常要顯著大於 fgt，避免重要詞因一時記住就被埋沒。
	3. 若當前 add 恰好是最後一次 add，還會再疊加一次「新近加入」增益。
	")]
	public nil _Add(){
		WordState.CurCntAdd++;
		WordState.CurCntValidRmb = 0;
		var coefficient0 = Cfg.AddCnt_Bonus.AtOrDefault(WordState.CurCntAdd.AsI32()-1, Cfg.DfltAddBonus);
		var coefficient = coefficient0;
		
		f64? FinalAddBonus;
		if(WordState.Pos == WordState.PosFinalAdd){
			//若當前處理的事件 是 所有「添加」事件中的最後一個
			//則給權重 增益
			FinalAddBonus = _CalcFinalAddBonus();
			coefficient *= FinalAddBonus.Value;
		}
		WordState.Weight *= coefficient;
		//TODO log change of weight

		return NIL;
	}

	[Doc(@$"處理「記得」事件。會使權重變小")]
	nil _Rmb(){
		WordState.CurCntRmb++;
		
		var coefficient0 = _CalcTimeWeightForCur();
		
		f64 debuff;//負加成

		//TOFIX 蠹:finalAddEventPos之前者亦有debuff?
		_ = @$"當 當前事件 在 {nameof(WordState.PosFinalAdd)} 之後
		且 所有事件中的最後一個事件是 「記得」 事件 時、
		算一個負加成 讓 權重變得更小。目標是使 剛背過的單詞 在短時間內 更不容易出現。
		";
		if(//若有debuff
			WordState.Pos >= WordState.PosFinalAdd
			&& _GetFinalLearnRecord().Learn == ELearn.Rmb
		){
			debuff = _CalcDebuff();
			var coefficient = coefficient0 * debuff;
			No0(ref coefficient);
			WordState.Weight /= coefficient; //權重除以係數 使權重更小
		}else{
			No0(ref coefficient0);
			WordState.Weight /= coefficient0;
		}
		//TODO log change
		return NIL;
	}

	[Doc(@$"處理「忘記」事件。會使權重變大")]
	nil _Fgt(){
		WordState.CurCntFgt++;
		var Prev = _GetPrevLearnRecord();//上個學習事件
		f64 coefficient0 = _CalcTimeWeightForCur();
		_ = @$"添加次數越多表示單詞越重要。
		對于更重要的單詞、忘記事件的權重增幅應該更大 相比于 不重要的單詞。
		故用係數乘以 單詞添加次數
		";
		coefficient0 *= WordState.CurCntAdd; // curPos之後之cnt_add不算
		//不讓係數太大 一個單詞可能會被忘記很多次。
		// 「記得」事件與「忘記」事件使用類似的算法、使「忘記」事件的權重增長不要太大
		coefficient0 *= Cfg.FgtCoefficientBuff; 
		_ = @$"如果上個事件剛好是 「添加」 事件 就讓權重更大、讓被加過的單詞更容易先被學一輪";
		if(Prev.Learn == ELearn.Add){
			coefficient0 *= Cfg.FgtCoefficientBuffIfPrevIsLearn;
		}
		WordState.Weight *= coefficient0;
		//TODO log change
		return NIL;
	}

	[Doc(@$"額外處理最後一個學習記錄")]
	nil _HandleFinal(){
		var Cur = _GetCurLearnRecord();
		if(Cur.Learn == ELearn.Add){
			_ = @$"若最後事件是 add、說明這個詞 在最近一次被添加之後還沒有被學習過。
			此時要補一筆額外加成，確保它在接下來一段時間內更容易被實際刷到。";
			var Bonus = _CalcBonusForFinal();
			if(Bonus < 1.1){
				Bonus = 1.1;
			}
			WordState.Weight *= Bonus;
			//TODO Log
		}//~
		//加ʹ次ˋ大於三之詞 視潙重要單詞。 若逾幾日未學習 則增權重
		else if(Cur.Learn == ELearn.Rmb){
			if((i64)WordState.CurCntAdd >= Cfg.AddCntOfImportantWord){
				i64 Diff = _Now() - Cur.UnixMs;
				if(Diff > Cfg.ImportantWordUnLearnedMs){ //重要單詞超過30天未學習過了，增權重
					var Days = Diff/(i64)ETimeInMs.Day; // 未學習天數
					WordState.Weight *= Days * Cfg.ImportantWordUnLearnedBuffPerDay;
					//TODO Log
				}
			}
		}
		else if(Cur.Learn == ELearn.Fgt){
			_ = @$"若最後事件是 fgt，代表此詞最新狀態是「目前不會」。
			這種情況 和 最後事件是 add 一樣重要，故借用 add 的時間近因加成後，再疊加 add 次數相關倍率。";
			var Bonus = _CalcBonusForFinal(); //借用
			if(Bonus < 1.1){Bonus = 1.1;}
			Bonus *= (WordState.CurCntAdd+1)*Cfg.FinalFgtCoefficientOfAddCnt; // 添加次數越大、增益越大
			WordState.Weight *= Bonus;
			//TODO log
		}
		return NIL;
	}



	ILearnRecord _GetCurLearnRecord(){
		return Word.LearnRecords[(i32)WordState.Pos];
	}

	[Doc(@$"取當前事件的前一個事件。

	若不存在則視爲非法狀態。
	這裡的隱含前提是: 首個學習事件應爲 add，因此需要前序事件的分支不應落在第一個事件上。")]
	ILearnRecord _GetPrevLearnRecord(){
		var R= Word.LearnRecords!.AtOrDefault((i32)WordState.Pos-1, null);
		if(R == null){
			throw new Exception("prev Learn Record not found"); //首個事件必潙add
		}
		return R;
	}

	[Doc(@$"取最後一個學習事件。

	它代表「此詞目前的最新狀態」，很多尾部修正邏輯都依賴這個判斷。")]
	ILearnRecord _GetFinalLearnRecord(){
		if(Word.LearnRecords.Count == 0){
			throw new Exception("no Learn Record found"); // 首個事件必潙add
		}
		return Word.LearnRecords[^1];
	}

	[Doc(@$"取最後一次 add 事件。

	和 _GetFinalLearnRecord() 不同，這裡關心的是「最近一次進入學習流程的起點」。")]
	ILearnRecord _GetFinalAdd(){
		var R = Word.LearnRecords!.AtOrDefault((i32)WordState.PosFinalAdd, null);
		if(R == null){
			throw new Exception("final Add Learn Record not found");// 首個事件必潙add
		}
		return R;
	}

	[Doc(@$"計算當前事件相對前一事件的時間差。

	這個時間差是多個公式的基礎輸入，代表兩次學習行爲之間隔了多久。")]
	i64 _TimeMsCurDiffPrev(){
		var Cur = _GetCurLearnRecord();
		var Prev = _GetPrevLearnRecord();
		var R = Cur.UnixMs - Prev.UnixMs;
		if(R < 0){
			throw new Exception("time is earlier than prev");
		}
		return R;
	}

	[Doc(@$"最基礎的 時間權重係數。
	目標: 将时间差映射为权重，且权重随着时间差增大而增大，但增幅逐渐放缓
	#Params([毫秒級時間差])
	主要流程 : 以秒爲單位 取 時間差 的 四次方根。
	f(t) = t^(1/4) 這個函數、隨 t 單調遞增。 符合: 時間差越大, 時間權重越大。
	當 t 越大時, 隨着 t 增大, f(t) 增幅越不明顯 (一階導數遞減, 增长率递减)
	")]
	f64 _CalcTimeWeight(i64 TimeDiffMs){
		f64 R = TimeDiffMs;
		R /= 1000; //轉化成秒。 毫秒級不方便理解
		//R = Math.Pow(R, 1/4); //整數除法 1/4 得0
		R = Math.Pow(R, 1.0/4);
		if(R <= 1){
			R = 1.01;
		}
		return R;
	}

	[Doc(@$"
	當前事件的時間 減去 上個事件的時間 再用時間差 去 {nameof(_CalcTimeWeight)} 算權重。
	這是一個常用包裝函數，目的是把「取時間差」和「套時間函數」綁在一起，
	減少各個事件處理分支重複寫樣板。
	")]
	f64 _CalcTimeWeightForCur(){
		var TimeDiffMs = _TimeMsCurDiffPrev();
		return _CalcTimeWeight(TimeDiffMs);
	}


	[Doc(@$"當 當前事件 在 {nameof(WordState.PosFinalAdd)} 之後
	且 所有事件中的最後一個事件是 「記得」 事件 時、
	算一個負加成 讓 權重變得更小。目標是使 剛背過的單詞 在短時間內 更不容易出現。
	")]
	public f64 _CalcDebuff(){
		f64 R = 1;
		_ = @$"當前時間 到 當前學習記錄 的 時間差";
		var Diff = _Now() - _GetCurLearnRecord().UnixMs;
		var DebuffDenominator = Cfg.DebuffDenominator;
		//12 小時內學過的詞，降權效果會被極大放大，讓它更不容易再次出現。
		if((u64)Diff < Cfg.TimeDiffInWhatMsNeedSuperDebuff){
			DebuffDenominator *= Cfg.SuperDebuffCoefficient;
		}
		_ = @$" f(t) = a / (t - b) 然後 取絕對值 
		這是 反比例函數芝、但橫軸往右偏移了 b。
		- 在 t 屬于 (b, 正無窮) 的區間內 t越大、f(t)越小。越難以掩蓋目標單詞的出現
		- 在 t 屬于 (0, b) 的 區間內, t越大、f(t)越大。越容易掩蓋目標單詞的出現 
		在 t 屬于 (0, b) 的 區間內 設計比較奇怪、預期的想法是 仍然是 t越大、f(t)越小、但 下降的幅度更慢、f(t)值 比 (b, 正無窮)的 f(t) 值 遠大
		";
		R = DebuffDenominator/(Diff - Cfg.DebuffRightShiftMs); // 冀 岡憶得之詞 于100分鐘內不復出 ??
		R = Math.Abs(R);
		if(R < 1){
			R = 1;
		}
		return R;
	}

	[Doc(@$"當前Unix毫秒時間戳")]
	i64 _Now(){
		return UnixMs.Now();
	}


	[Doc(@$"當 當前處理的事件 是 所有「添加」事件中的最後一個 時
	給單詞權重乘上一個額外增益係數。
	具體規則/目標:
	末次 加事件越近、加成越大。
	#Rtn[額外增益係數、 用于乘到單詞權重上]

	這個加成不是爲了表達「重要性」，而是爲了表達「新近添加的詞應被儘快看到」。
	換言之，它更偏向調度優先級，而不是詞本身價值判斷。
	")]
	public f64 _CalcFinalAddBonus(){
		f64 R = _NowDiffFinalAddTime();
		No0(ref R);//R要作除數、確保非0
		
		_ = @$"
		t:= _NowDiffFinalAddTime 即 當前時間 減 最末一次添加 所發生的時間。
		函數 f(t) = FinalAddBonusDenominator / t
		這是反比例函數
		隨着當前時間增大、 t 也增大、 f(t)則會變小。符合目標: 末次 加事件越近、加成越大。
		";
		R = Cfg.FinalAddBonusDenominator / R;
		
		_ = @$"再讓返回的加成係數除以一次記得次數。
		目標: 記得ʹ次數越多 加成越少。記得次數多說明已經對這個單詞比較熟悉了、不需要這麼多加成
		+1是蔿使其不潙0";
		var Rmb = State.WordState.CurCntRmb + 1;
		R = R / Rmb;
		if(R < 1){//確保不小於1。小於1則 權重與係數的乘除操作 會出意外
			R = 1;
		}
		return R;
	}

	[Doc(@$"當最後一個事件是 add 或需要借用 add 的尾部加成邏輯時使用。

	其核心語義是:
	- 最後事件離現在越近，返回值越大；
	- 也就是說，剛剛被加進來或剛剛重新暴露出問題的詞，會被更靠前地重新安排。

	這和 _CalcFinalAddBonus() 類似，但用途更偏向「最後事件是 add/fgt 時的收尾調整」，
	而不是在遍歷到最後一個 add 當下立刻加成。
	")]
	f64 _CalcBonusForFinal(){
		var DiffMs = _Now() - _GetCurLearnRecord().UnixMs;
		var DiffS = DiffMs/1000;//換算成秒
		No0(ref DiffS);
		// 以時間爲 自變量 的 反比例函數。
		// 目標: 越近的事件 其權重越大。
		f64 R = Cfg.BonusDenominatorWhenFinalIsAdd / DiffS;
		if(R < 1){
			R = 1;
		}
		return R;
	}

	[Doc(@$"UnixMs下 當前時間 減 最末一次添加 所發生的時間")]
	i64 _NowDiffFinalAddTime(){
		var FinalAdd = _GetFinalAdd();
		var R = WordState.Now - FinalAdd.UnixMs;
		if(R < 0){
			throw new Exception("final Add time is earlier than now");
		}
		return R;
	}

	[Doc(@$"確保數值不爲0、若爲0則改爲1。
	保證除法運算時不出錯

	這不是數學上嚴格的處理，而是工程上的保底處理。
	目標是避免「極端小概率輸入」把整個算法算崩，而不是追求在 0 點附近的精確連續性。
	")]
	static void No0(ref f64 z){
		if(z == 0){
			z = 1;
		}
	}

	
	static void No0(ref i64 z){
		if(z == 0){
			z = 1;
		}
	}


}
