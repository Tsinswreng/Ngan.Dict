namespace Ngaq.Core.Shared.Word.Models;

using Ngaq.Core.Infra.IF;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Infra.Errors;
using Tsinswreng.CsTools;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Infra;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Tools.Algo;
using Ngaq.Core.Model.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Learn;
using Ngaq.Core.Model.Po.Learn_;
using Ngaq.Core.Shared.Word.Models.Dto;
using Tsinswreng.CsErr;
using Tsinswreng.Srefl;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.User.Models.Po;
using Tsinswreng.CsTempus;

public static class ExtnJnWord{
	extension<TSelf>(TSelf z)
		where TSelf:IJnWord
	{
		public IJnWord DeepClone(){
			var word = (PoWord)z.Word.ShallowCloneSelf();
			var props = z.Props.Select(x=>(PoWordProp)x.ShallowCloneSelf()).ToList();
			var learns = z.Learns.Select(x=>(PoWordLearn)x.ShallowCloneSelf()).ToList();
			var R = new JnWord(
				word, props, learns
			);
			return R;
		}
		public IJnWord ResetAllIds(){
			z.Word.Id = new();
			foreach(var prop in z.Props){
				prop.Id = new ();
			}
			foreach(var learn in z.Learns){
				learn.Id = new();
			}
			return z;
		}

		public IJnWord DeepCloneEtResetIds(){
			var R = z.DeepClone();
			R.ResetAllIds();
			return R;
		}


		public IdWord Id{
			get{return z.Word.Id;}
			set{
				z.Word.Id = value;
				z.EnsureForeignId();
			}
		}
		
		public IdUser Owner{
			get{return z.Word.Owner;}
			set{z.Word.Owner = value;}
		}
		public str Lang{
			get{return z.Word.Lang;}
			set{z.Word.Lang = value;}
		}
		public str Head{
			get{return z.Word.Head;}
			set{z.Word.Head = value;}
		}

		[Impl(typeof(IPoWord))]
		public UnixMs StoredAt{
			get{return z.Word.StoredAt;}
			set{z.Word.StoredAt = value;}
		}

		public UnixMs DbCreatedAt{
			get{return z.Word.DbCreatedAt;}
			set{z.Word.DbCreatedAt = value;}
		}

		public UnixMs BizCreatedAt{
			get{return z.Word.BizCreatedAt;}
			set{z.Word.BizCreatedAt = value;}
		}

		/// 當關聯ʹ他表 更新旹、亦當更新此字段
		public UnixMs BizUpdatedAt{
			get{return z.Word.BizUpdatedAt;}
			set{z.Word.BizUpdatedAt = value;}
		}
		public UnixMs DbUpdatedAt{
			get{return z.Word.DbUpdatedAt;}
			set{z.Word.DbUpdatedAt = value;}
		}
		
		public IdDel DelAt{
			get{return z.Word.DelAt;}
			set{z.Word.DelAt = value;}
		}
		
		

		public IDictionary<str, obj?> ToDict(
			IPropAccessorReg PropAccessorReg
		){
			
			IDictionary<str, obj?> ToShallow(Type Type, obj Obj){
				if(!PropAccessorReg.Type_PropAccessor.TryGetValue(Type, out var Accessor)){
					throw new Exception($"No {nameof(IPropAccessor)} registered for type: {Type}");
				}
				var R2 = new Dictionary<str, obj?>();
				foreach(var Key in Accessor.GetGetterNames(Obj)){
					if(!Accessor.TryGet(Obj, Key, out var V)){
						continue;
					}
					R2[Key] = V;
				}
				return R2;
			}

			var R = new Dictionary<str, obj?>();
			R[nameof(IJnWord.Word)] = ToShallow(z.Word.GetType(), z.Word);
			var props = new List<IDictionary<str, obj?>>();
			R[nameof(IJnWord.Props)] = props;
			foreach(var prop in z.Props){
				props.Add(ToShallow(prop.GetType(), prop));
			}
			var learns = new List<IDictionary<str, obj?>>();
			R[nameof(IJnWord.Learns)] = learns;
			foreach(var learn in z.Learns){
				learns.Add(ToShallow(learn.GetType(), learn));
			}
			return R;
		}

		/// 把諸資產之外鍵設潙主Word之id
		public TSelf EnsureForeignId(){
			foreach(var prop in z.Props){
				prop.WordId = z.Word.Id;
			}
			foreach(var learn in z.Learns){
				learn.WordId = z.Word.Id.Value;
			}
			return z;
		}


		public TSelf SetIdEtEnsureFKey(IdWord Id){
			z.Word.Id = Id;
			z.EnsureForeignId();
			return z;
		}


	}

	public static JnWord AsOrToJnWord(this IJnWord z){
		if(z is JnWord j){
			return j;
		}
		return new JnWord(z.Word, z.Props, z.Learns);
	}
	public static IJnWord AsSimpleJnWord(this JnWord z){
		return (IJnWord)z;
	}

	/// 按詞頭對諸詞分組
	/// 若入ʹ諸詞 非皆屬同一語 則拋錯
	/// 返ʹ值: 詞頭->諸詞芝厥語語詞頭皆同者
	public static IDictionary<str, IList<IJnWord>> GroupByHeadOfSameLang(
		this IEnumerable<IJnWord> JnWords
	){
		// var Dict = BoWords.GroupBy(w=>w.PoWord.WordFormId)
		// 	.ToDictionary(g=>g.Key, g=>(IList<BoWord>)[.. g])//  g=>g.ToList() -> [..g]
		// ;
		// return Dict;
		var Dict = new Dictionary<str, IList<IJnWord>>();
		str Lang = "";
		var i = 0;
		foreach(var JWord in JnWords){
			if(i == 0){
				Lang = JWord.Word.Lang;
			}
			if(JWord.Word.Lang != Lang){
				throw KeysErr.Word.__NotBelongToLang__
				.ToErr(JWord.Id, Lang)
				.AddDebugArgs(JWord);
			}
			if(Dict.TryGetValue(JWord.Word.Head, out var List)){
				List.Add(JWord);
			}else{
				Dict[JWord.Word.Head] = [JWord];
			}
			i++;
		}
		return Dict;
	}


	/// 按(詞頭,語言)對諸詞分組
	/// 返ʹ值: (詞頭,語言)->諸詞芝厥語語詞頭皆同者
	public static IDictionary<Head_Lang, IList<IJnWord>> GroupByLangHead(
		this IEnumerable<IJnWord> JnWords
	){
		var Dict = new Dictionary<Head_Lang, IList<IJnWord>>();
		foreach(var JWord in JnWords){
			var lang_head = new Head_Lang{
				Lang = JWord.Word.Lang
				,Head = JWord.Word.Head
			};
			if(Dict.TryGetValue(lang_head, out var List)){
				List.Add(JWord);
			}else{
				Dict[lang_head] = [JWord];
			}
		}
		return Dict;
	}

	/// 只比詞頭 語言 與擁者
	public static bool IsSameUserWord(
		this IPoWord W1
		,IPoWord W2
	){
		if(W1.Head != W2.Head){
			return false;
		}
		if(W1.Lang != W2.Lang){
			return false;
		}
		if(W1.Owner != W2.Owner){
			return false;
		}
		return true;
	}

	public static nil CheckSameUserWord(
		this IJnWord W1
		,IJnWord W2
	){
		if(!W1.Word.IsSameUserWord(W2.Word)){
			return MkErrNotSameUserWord(W1,W2);
		}
		return NIL;
	}

	//有蠹 2個Prop芝CreatedAtˋ同者 diff 一個Prop旹 diff不出 只適用于新增單詞
	[Obsolete]
	public static IJnWord? DiffByTime(
		this IJnWord z
		,IJnWord Other
	){
		IJnWord? R=null;
		z.DiffByTime(Other, ref R);
		return R;
	}

	//有蠹 2個Prop芝CreatedAtˋ同者 diff 一個Prop旹 diff不出 只適用于新增單詞
	/// Other 合入 z 返R
	/// 無需合併旹返null
	/// 非同ʹ詞旹拋錯
	[Obsolete]
	public static IJnWord? DiffByTime(
		this IJnWord z
		,IJnWord Other
		,ref IJnWord? R
	){
		var z_ = z.AsOrToJnWord();
		var Other_ = Other.AsOrToJnWord();
		z_.CheckSameUserWord(Other_);
		if(z.IsSynced(Other)){
			return null;
		}
		var DiffedProps = JnWord.DiffPropsByTime(z.Props, Other.Props);
		var DiffedLearns = JnWord.DiffLearnsByTime(z.Learns, Other.Learns);
		R??=new JnWord();
		R.Word = z.Word;
		R.Props = DiffedProps;
		R.Learns = DiffedLearns;
		R.UpdTimeOfSelfContrastToOther(Other);
		return R;
	}

	static AppErr MkErrNotSameUserWord(
		IJnWord z
		,IJnWord Other
	){
		return KeysErr.Word.__And__IsNotSameUserWord
		.ToErr(z.Id, Other.Id)
		.AddDebugArgs(z,Other);
	}

/// CreatedAt取更早者
/// UpdatedAt取最晚者
	public static IJnWord UpdTimeOfSelfContrastToOther(
		this IJnWord z
		,IJnWord Other
	){
		var jnWord = z.AsOrToJnWord();
		var other_ = Other.AsOrToJnWord();
		CheckSameUserWord(jnWord, other_);

		if(jnWord.DbCreatedAt > other_.DbCreatedAt){
			jnWord.DbCreatedAt = other_.DbCreatedAt;
		}
		if(jnWord.BizCreatedAt > other_.BizCreatedAt){
			jnWord.BizCreatedAt = other_.BizCreatedAt;
		}
		if(jnWord.BizUpdatedAt < other_.BizUpdatedAt){
			jnWord.BizUpdatedAt = other_.BizUpdatedAt;
		}
		// if(R.LastUpdatedBy == null && Other.LastUpdatedBy != null){
		// 	R.LastUpdatedBy = Other.LastUpdatedBy;
		// }
		return jnWord;
	}


	/// 無去褈ˌᵈ併ᵣ諸詞ˇ
	/// 用于併ᵣ文本詞表ᙆʹ待加ʹ諸詞
	/// JnWords 須潙同一詞 否則拋錯

	public static IJnWord? NoDiffMergeSameWords(
		this IEnumerable<IJnWord> JnWords
	){
		JnWord R = null!;
		foreach(var (i, SimpleJWord) in JnWords.Index()){
			var jWord = SimpleJWord.AsOrToJnWord();
			if(i == 0){
				R = jWord.AsOrToJnWord();
			}else{
				CheckSameUserWord(R,jWord);
				R.UpdTimeOfSelfContrastToOther(jWord);
				R.Props.AddRange(jWord.Props);
				R.Learns.AddRange(jWord.Learns);
			}
		}//~foreach
		return R;
	}

	public static IJnWord SortByCreatedAtAsc(
		this IJnWord z
	){
		z.Props.Sort((a,b)=>a.BizCreatedAt.Value.CompareTo(b.BizCreatedAt));
		z.Learns.Sort((a,b)=>a.BizCreatedAt.Value.CompareTo(b.BizCreatedAt));
		return z;
	}


	/// 簡ᵈ判斷是否同步。
	/// 緣要求有改動旹須改聚合根(即JnWord.Word)ʹBizUpdatedAt、
	/// 故正常ʹ態下 BizUpdatedAt 與 BizCreatedAt 一致 即已同步
	public static bool IsSynced(
		this IJnWord Other
		,IJnWord Existing
	){
		var Other_ = Other.AsOrToJnWord();
		var Existing_ = Existing.AsOrToJnWord();
		CheckSameUserWord(Other_, Existing_);
		if(//視潙同一詞 返null
			Other_.BizCreatedAt == Existing_.BizCreatedAt
			&& Other_.BizUpdatedAt == Existing_.BizUpdatedAt
			&& Other_.Props.Count == Existing_.Props.Count
			&& Other_.Learns.Count == Existing_.Learns.Count
		){
			return true;
		}
		return false;
	}



/// 同步ˢ。若Other更新則使Other合入。
/// 合入後ʹ果ˇ寫入ref R。如ref R潙null則R=z
/// </typeparam><param name="z"> 舊實體。只讀 若需寫入z則使R=z
/// </param><param name="Other">新實體。只讀
/// </param><param name="R"> 果ˇ寫入焉
	public static ESyncResult SyncPo<T>(
		this T z
		,T Other
		,ref T? R
	)where T : class, IBizCreateUpdateTime
	{

		if(z.BizUpdatedAt >= Other.BizUpdatedAt){
			return ESyncResult.NoNeedToSync;
		}
		R??=z;
		var mgr = (IPropAccessorReg)CoreDictMapper.Inst;
		if(!mgr.Type_PropAccessor.TryGetValue(typeof(T), out var accessor)){
			throw new Exception($"No {nameof(IPropAccessor)} registered for type: {typeof(T)}");
		}
		var selfDict = new Dictionary<str, obj?>();
		var otherDict = new Dictionary<str, obj?>();
		foreach(var key in accessor.GetGetterNames(z)){
			if(accessor.TryGet(z, key, out var v1)){
				selfDict[key] = v1;
			}
		}
		foreach(var key in accessor.GetGetterNames(Other)){
			if(accessor.TryGet(Other, key, out var v2)){
				otherDict[key] = v2;
			}
		}
		foreach(var (otherK, otherV) in otherDict){
			selfDict[otherK] = otherV;
		}
		foreach(var (k, v) in selfDict){
			accessor.TrySet(R, k, v);
		}
		return ESyncResult.Ok;
	}

	/// 同步。添ʃ缺、改ʃ異。
	/// 注意 于外ʸ 慎ᵈ 直ᵈ㕥 ref R 當 一般ₐJnWord用。
	/// <param name="z">舊詞(函數中不會通過z指針㕥改己)若需寫入z則使R=z
	/// </param><param name="Neoer">新詞。只讀
	/// </param><param name="R">同步ʹ果ˇ寫入焉
	public static DtoSyncTwoWords Sync(
		this IJnWord z//函數中只讀  作同步依據
		,IJnWord Neoer
		//,ref IJnWord? R//函數中作果ˇ寫入ʹ處
		,ref IJnWord? RNeoPart //只有 列表 Props,Learns
		,ref IJnWord? RChangedPart //Word,Props,Learns都可能有
	){
		var R = new DtoSyncTwoWords();
		CheckSameUserWord(z,Neoer);
		var z_ = z.AsOrToJnWord();
		var Neoer_ = Neoer.AsOrToJnWord();
		if(z.IsSynced(Neoer)){
			// RNeoPart = null;
			// RChangedPart = null;
			return R;
		}

		RNeoPart??=z;
		RChangedPart??=z;
		PoWord? NeoPoWord = RChangedPart.Word;
		if(z.Word.SyncPo(Neoer.Word, ref NeoPoWord) == ESyncResult.Ok){
			RChangedPart.Word = NeoPoWord!;
			R.ChangedPoWord = NeoPoWord;
		}


		var UnAddedProps = Neoer.Props.DiffById<PoWordProp, IdWordProp>(z.Props);
		var UnAddedLearns = Neoer.Learns.DiffById<PoWordLearn, IdWordLearn>(z.Learns);

		var OldId_Prop = z.Props.Select(x=>x).ToDictionary(x=>x.Id, x=>x);
		var NeoId_Prop = Neoer.Props.Select(x=>x).ToDictionary(x=>x.Id, x=>x);

		var OldId_Learn = z.Learns.Select(x=>x).ToDictionary(x=>x.Id, x=>x);
		var NeoId_Learn = Neoer.Learns.Select(x=>x).ToDictionary(x=>x.Id, x=>x);

		IList<PoWordProp> changedWordProps = [];
		foreach(var (oldId, _oldProp) in OldId_Prop){
			//var neoProp = NeoId_Prop[oldId];
			if(!NeoId_Prop.TryGetValue(oldId, out var neoProp)){
				continue;
			}
			var oldProp = _oldProp;
			if(oldProp.SyncPo(neoProp, ref oldProp) != ESyncResult.Ok){
				continue;
			}
			changedWordProps.Add(oldProp!);
		}

		IList<PoWordLearn> changedWordLearns = [];
		foreach(var (oldId, _oldLearn) in OldId_Learn){
			//var neoLearn = NeoId_Learn[oldId];
			if(!NeoId_Learn.TryGetValue(oldId, out var neoLearn)){
				continue;
			}
			var oldLearn = _oldLearn;
			if(oldLearn.SyncPo(neoLearn, ref oldLearn) != ESyncResult.Ok){
				continue;
			}
			changedWordLearns.Add(oldLearn!);
		}

		RChangedPart.Props = changedWordProps;
		RChangedPart.Learns = changedWordLearns;

		RNeoPart.Props.AddRange(UnAddedProps);
		RNeoPart.Learns.AddRange(UnAddedLearns);

		R.NeoOrChangedProps.NeoPart = UnAddedProps;
		R.NeoOrChangedProps.ChangedPart = changedWordProps;

		R.NeoOrChangedLearns.NeoPart = UnAddedLearns;
		R.NeoOrChangedLearns.ChangedPart = changedWordLearns;

		return R;
	}

	public static IList<TItem> DiffById<TItem, TId>(
		this IList<TItem> ListA, IList<TItem> ListB
	)where TItem: AppI_Id<TId>
	where TId: notnull
	{
		var diff = Algo.DiffListIntoDict(
			ListA, ListB
			, (e)=> e.Id
		);
		List<TItem> ans = [];
		foreach(var (Time,Props) in diff){
			ans.AddRange(Props);
		}
		return ans;
	}

}


