namespace Ngaq.Core.Shared.Word.Models.Learn_;


/// 上輪學習記錄
/// 供權重算法用 如 無上輪學習記錄者視潙未學 則不緟算權重

public  partial interface I_PrevTurnLearnRecords{
	
	/// 上輪學習記錄
	/// 供權重算法用 如 無上輪學習記錄者視潙未學 則不緟算權重
	
	public IList<ILearnRecord> PrevTurnLearnRecords{get;set;}
	#if Impl
	= new List<ILearnRecord>();
	#endif
}
