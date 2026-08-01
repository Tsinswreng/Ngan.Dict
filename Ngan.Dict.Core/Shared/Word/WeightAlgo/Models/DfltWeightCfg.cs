using System.Collections;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Tools;
using Ngaq.Core.Tools.Json;
using Ngaq.Core.Word.WeightAlgo.Models;
using Tsinswreng.CsTools;

namespace Ngaq.Core.Shared.Word.WeightAlgo.Models;

[Doc(@$"默認權重算法的配置模型。
此類只負責描述「算法可調參數」，不直接執行計算。

設計意圖:
1. 把權重算法中最容易反覆調參的常量抽離到配置層。
2. 讓算法本體保留穩定結構，而把「力度多大」這類策略問題交給配置。
3. 便於把配置序列化、持久化、或在不同算法版本之間切換。

閱讀方式:
- 若想知道算法整體流程，先看 CalculatorForOne。
- 若想知道某個倍率/分母/閾值爲何存在，先回到此類看字段註釋。
")]
public partial class DfltWeightCfg:IAppSerializable{
	/// 此配置所屬的計算器名稱。用於在外層系統中把配置和算法實現對上。
	public static str ForName => DfltWeightCalculator.Name;
	/// 配置版本名。當算法策略演進時，可藉此區分不同版本的調參方案。
	public static str Name => "Tswg-EventFlowV1";
	// public IDictionary<u64, f64> AddCnt_Bonus = new Dictionary<u64, f64>(){
	// 	[0] = 0x1
	// 	,[1] = 0xff
	// 	,[2] = 0xfff
	// 	,[3] = 0xffff
	// };
	[Doc(@$"
	#See{nameof(CalculatorForOne._Add)}
	處理「添加」事件時給權重乘的系數。
	- 處理索引爲0的添加事件(第1次添加)時 就給權重乘上 {nameof(AddCnt_Bonus)}[0]
	- 處理索引爲1的添加事件(第2次添加)時 就給權重乘上 {nameof(AddCnt_Bonus)}[1]
	依此類推。
	
	單詞的添加次數 在整個權重算法中是最優先考慮的。
	因 添加次數越多 則說明單詞出現次數越多、該單詞越重要、故乘上的系數越大。
	若「忘記」事件數量多 但「添加」事件不多、只能說明此單詞不熟、但無法說明 此單詞出現頻率高/重要。
	故優先考慮添加次數、按添加次數劃級。
	
	「添加」與「忘記」事件會使單詞的權重增加、「記得」事件會使單詞的權重減少。
	「添加」事件 對 單詞權重的增幅 遠大於 「忘記」事件。

	設計上的關鍵點:
	1. 這是一個「按添加次數分級」的表，而不是平滑函數。
	因爲添加次數通常不會大到需要連續函數擬合，直接分級更直觀，也更容易調參。
	2. 這裡表達的是「單詞重要性」而非「記憶熟練度」。
	重要性高的詞，就算後面記住了很多次，也應該先有一個較高的基礎權重。
	")]
	public IList<f64> AddCnt_Bonus{get;set;} = new List<f64>(){
		0xff,0xfff,0xffff,0xfffff //對應 舊版ʹaddWeight
	};

	[Doc(@$"
	#See{nameof(CalculatorForOne._Add)}
	當添加次數超出 {nameof(AddCnt_Bonus)} 表長時使用的默認值。
	設計意圖:
	- 前幾次添加通常最有分析價值，故單獨列在 {nameof(AddCnt_Bonus)} 中。
	- 再往後的添加次數雖然少見，但若真的出現，往往代表這個詞極重要，
	  因此直接給一個很大的兜底增益，而不是讓算法退化成與前幾級相同。
	")]
	public f64 DfltAddBonus{get;set;} = 0xffffffff;
	
	[Doc(@$"
	#See[{nameof(CalculatorForOne._CalcDebuff)}]
	
	計算 debuff 時使用的分子。
	它控制「最近剛記住的詞要被壓低到什麼程度」。
	值越大，整體 debuff 越強；值越小，剛記住的詞回到候選集合的速度越快。

	之所以設成配置，而不是寫死在公式裡，是因爲這個量級非常依賴實際使用節奏。
	不同場景下，使用者一天學很多詞或只學少量詞，對 debuff 強度的容忍度會很不一樣。
	")]
	public f64 DebuffDenominator{get;set;} = 36*ETimeInMs.Day;

	[Doc(@$"保留字段。當前版本算法未直接使用。")]
	public f64 Base{get;set;} = 20;

	[Doc(@$"
	#See{nameof(CalculatorForOne._CalcFinalAddBonus)}
	計算「最後一次添加事件」額外加成時使用的分子。

	對應公式位於 CalculatorForOne._CalcFinalAddBonus()。
	當末次添加離現在越近時，分母越小，最終加成越大。

	此值控制的是「新近加入詞庫的詞要被優先到什麼程度」。
	若該值過大，剛加入的詞會長時間霸佔前列；
	若過小，則新詞剛加入後不夠容易被再次刷到。
	")]
	public f64 FinalAddBonusDenominator{get;set;} = ETimeInMs.Day*3000;
	
	public f64 FgtCoefficientBuff{get;set;} = 1.0/10.0;
	
	public f64 FgtCoefficientBuffIfPrevIsLearn{get;set;} = 4.0;
	
	public i64 AddCntOfImportantWord{get;set;} = 3;
	public f64 ImportantWordUnLearnedMs{get;set;} = (i64)ETimeInMs.Day*30;
	public f64 ImportantWordUnLearnedBuffPerDay{get;set;} = 0xfff;
	
	public f64 FinalFgtCoefficientOfAddCnt{get;set;} = 2;
	
	public f64 TimeDiffInWhatMsNeedSuperDebuff{get;set;} = ETimeInMs.Hour * 12;
	public f64 SuperDebuffCoefficient{get;set;} = 0xffffffff;
	public f64 DebuffRightShiftMs{get;set;} = ETimeInMs.Min*100;
	
	public f64 BonusDenominatorWhenFinalIsAdd{get;set;} = ETimeInMs.Day*360000;

	[Doc(@$"從字典反序列化出配置實例。

	之所以不手寫字段映射，而是先轉 JSON 再反序列化，是爲了:
	1. 復用既有序列化邏輯；
	2. 降低字段增減時的維護成本；
	3. 讓外部配置來源只要能提供字典即可接入。
	")]
	public static DfltWeightCfg FromDict(IDictionary<str, obj?> Dict){
		var json = ToolJson.DictToJson(Dict);
		var r = JSON.Parse<DfltWeightCfg>(json);
		return r!;
	}
}
