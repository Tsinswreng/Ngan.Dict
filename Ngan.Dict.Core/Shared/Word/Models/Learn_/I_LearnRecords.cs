namespace Ngaq.Core.Shared.Word.Models.Learn_;

public  partial interface I_LearnRecords{
	
	/// 已保存之學習記錄
	/// 當有序
	
	public IList<ILearnRecord> LearnRecords{get;set;}
	#if Impl
	= new List<ILearnRecord>();
	#endif
}
