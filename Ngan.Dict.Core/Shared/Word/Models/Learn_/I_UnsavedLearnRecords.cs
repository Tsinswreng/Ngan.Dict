namespace Ngaq.Core.Shared.Word.Models.Learn_;

public partial interface I_UnsavedLearnRecords{
	public IList<ILearnRecord> UnsavedLearnRecords{get;set;}
	#if Impl
	= new List<ILearnRecord>();
	#endif
}

public static class ExtnI_UnsavedLearnRecords{
	
	/// 適用于 每輪學習中只有一個新ʹ學習記錄
	
	/// <param name="z"></param>
	/// <param name="LearnRecord"></param>
	/// <returns></returns>
	public static i32 AddLearnRecordIfEmpty(
		this I_UnsavedLearnRecords z
		,LearnRecord LearnRecord
	){
		if(z.UnsavedLearnRecords.Count == 0){
			z.UnsavedLearnRecords.Add(LearnRecord);
			return 0;
		}
		return 1;
	}
	public static ILearnRecord? RmLastUnsavedLearnRecord(
		this I_UnsavedLearnRecords z
	){
		if(z.UnsavedLearnRecords.Count == 0){
			return null;
		}
		var Last = z.UnsavedLearnRecords[^1];
		z.UnsavedLearnRecords.RemoveAt(z.UnsavedLearnRecords.Count-1);
		return Last;
	}

	public static ILearnRecord? GetLastUnsavedLearnRecord(
		this I_UnsavedLearnRecords z
	){
		if(z.UnsavedLearnRecords.Count == 0){
			return null;
		}
		return z.UnsavedLearnRecords[^1];
	}
}
