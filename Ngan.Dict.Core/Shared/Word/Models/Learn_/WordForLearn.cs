#define Impl
namespace Ngaq.Core.Shared.Word.Models.Learn_;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Ngaq.Core.Shared.User.Models.Po;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Tsinswreng.CsTools;
using Ngaq.Core.Shared.Base.Models.Po;
using Tsinswreng.CsTempus;

public partial class WordForLearn
	:IWordForLearn
{
	protected PoWord PoWord{get;set;}
	protected IJnWord _SimpleJnWord{get;set;}
	public IJnWord JnWord{get;set;}
	public WordForLearn(
		IJnWord JWord
	){
		_SimpleJnWord = JWord;
		JnWord = JWord.AsOrToJnWord();
		PoWord = JWord.Word;
		StrKey_Props.FromPoKvs(_SimpleJnWord.Props);
		Learn_Records.AddFromPoLearns(_SimpleJnWord.Learns);
		LearnRecords = _SimpleJnWord.Learns
			.Select(x=>x.ToLearnRecord())
			.OrderBy(x=>x.UnixMs)
			.ToList()
		;
	}

	public event PropertyChangedEventHandler? PropertyChanged;//受保護之委託 不能在類外調
	public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null){
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	[Impl(typeof(I_Weight))]
	public f64? Weight{get;set;}
	[Impl(typeof(I_Index))]
	public u64? Index{get;set;}

	[Impl(typeof(I_StrKey_Props))]
	public IDictionary<str, IList<IProp>> StrKey_Props{get;set;}
	#if Impl
	= new Dictionary<str, IList<IProp>>();
	#endif

	[Impl(typeof(I_Learn_Records))]
	public IDictionary<ELearn, IList<ILearnRecord>> Learn_Records{get;set;}
	#if Impl
	= new Dictionary<ELearn, IList<ILearnRecord>>();
	#endif

	[Impl(typeof(I_LearnRecords))]
	public IList<ILearnRecord> LearnRecords{get;set;}
	#if Impl
	= new List<ILearnRecord>();
	#endif

	// public IDictionary<i64, LearnRecord> Time_UnsavedLearnRecords{get;set;}
	// #if Impl
	// = new Dictionary<i64, LearnRecord>();
	// #endif

	[Impl(typeof(I_UnsavedLearnRecords))]
	public IList<ILearnRecord> UnsavedLearnRecords{get;set;}
	#if Impl
	= new List<ILearnRecord>();
	#endif

	[Impl(typeof(I_PrevTurnLearnRecords))]
	public IList<ILearnRecord> PrevTurnLearnRecords{get;set;}
	#if Impl
	= new List<ILearnRecord>();
	#endif


	//public i64 LastLearnedTime{get;set;}

	[Impl(typeof(I_IdWord))]
	public IdWord Id{
		get=>PoWord.Id;
		set{
			JnWord.SetIdEtEnsureFKey(value);
		}
	}

	[Impl(typeof(IPoWord))]
	public IdUser Owner{
		get=>PoWord.Owner;
		set{PoWord.Owner = value;}
	}

	[Impl(typeof(IPoWord))]
	public str Lang{
		get=>PoWord.Lang;
		set{PoWord.Lang = value;}
	}

	[Impl(typeof(IPoWord))]
	public str Head{
		get=>PoWord.Head;
		set{PoWord.Head = value;}
	}


	#region IPoBase
	[Impl(typeof(IPoBase))]
	public IdDel DelAt{
		get=>PoWord.DelAt;
		set{
			PoWord.DelAt = value;
		}
	}

	[Impl(typeof(IPoBase))]
	public UnixMs DbCreatedAt{
		get{return PoWord.DbCreatedAt;}
		set{PoWord.DbCreatedAt = value;}
	}

	[Impl(typeof(IPoBase))]
	public UnixMs BizCreatedAt{
		get{return PoWord.BizCreatedAt;}
		set{PoWord.BizCreatedAt = value;}
	}

	[Impl(typeof(IPoBase))]
	public UnixMs DbUpdatedAt{
		get{return PoWord.DbUpdatedAt;}
		set{PoWord.DbUpdatedAt = value;}
	}

	/// 當關聯ʹ他表 更新旹、亦當更新此字段
	[Impl]
	public UnixMs BizUpdatedAt{
		get{return PoWord.BizUpdatedAt;}
		set{PoWord.BizUpdatedAt = value;}
	}

	// [Impl(typeof(IPoBase))]
	// public PoStatus Status{
	// 	get{return PoWord.Status;}
	// 	set{PoWord.Status = value;}
	// }


	#endregion IPoBase
[Impl(typeof(I_ShallowCloneSelf))]
	public object ShallowCloneSelf(){
		return MemberwiseClone();
	}

}


public static class ExtnIWordForLearn{
	public static i64 LastLearnedTime_(
		this IWordForLearn z
	){
		if(z.LearnRecords.Count > 0){
			return z.LearnRecords[^1].UnixMs;
		}
		return 0;//should not happen
	}

	/// 保存後 把新ʹ學習記錄 添入LearnRecords列表與Learn_Records字典
	/// 把新學習記錄予上輪ʹ學習記錄、緟設新學習記錄
	/// <param name="z"></param>
	/// <returns></returns>
	public static nil HandleLearnRecordsOnSave(
		this IWordForLearn z
	){
		z.LearnRecords.AddRange(z.UnsavedLearnRecords);
		z.Learn_Records.AddFromLearnRecords(z.UnsavedLearnRecords);//TODO 把學習記錄專封到一個類中。否則改于外易漏
		z.PrevTurnLearnRecords = z.UnsavedLearnRecords;
		z.UnsavedLearnRecords = new List<ILearnRecord>();
		z.OnPropertyChanged("");//事件“INotifyPropertyChanged.PropertyChanged”只能出现在 += 或 -= 的左边CS0079
		return NIL;
	}
}
