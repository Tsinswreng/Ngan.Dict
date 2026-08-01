using Ngaq.Core.Shared.Word.Models;

namespace Ngaq.Core.Shared.Sync;

[Doc(@$"一對聚合實體(也可能是單個Po)同步結果。
Remote合入Local。
#See[{nameof(AggDiffCaseForSync)}]
")]
public interface IAggSyncResult<T>{
	public EDiffByBizIdResultForSync DiffResult{get;set;}
	
	[Doc(@$"適用于當{nameof(EDiffByBizIdResultForSync.RemoteIsNewer)}時。")]
	public IAggDiffCaseForSync? DiffCase{get;set;}
	
	[Doc(@$"同步前的Local 原對象")]
	public T? Local{get;set;}
	[Doc(@$"Remote對象")]
	public T? Remote{get;set;}
	
	[Doc(@$"僅含 Remote 比 Local多出的新資產。
	不考慮此字段之聚合根
	#Example([
	假設{nameof(T)}是{nameof(JnWord)}、
	則此字段爲多出的{nameof(JnWord.Props)}和{nameof(JnWord.Learns)}、
	不要管這個字段的{nameof(JnWord.Word)}(聚合根)
	])
	")]
	public T? NewAssets{get;set;}
	
	[Doc(@$"僅含 Remote 比 Local 更改的資產。
	不考慮此字段之聚合根
	#See[{nameof(NewAssets)}]
	")]
	public T? ChangedAssets{get;set;}
	
	[Doc(@$"同步後的聚合根。
	用來對應更新Local的聚合根。
	")]
	public T? SyncedRoot{get;set;}
}


public class AggSyncResult<T>: IAggSyncResult<T>{
	public EDiffByBizIdResultForSync DiffResult{get;set;} = EDiffByBizIdResultForSync.Unknown;
	public IAggDiffCaseForSync? DiffCase{get;set;}
	
	public T? Local{get;set;}
	public T? Remote{get;set;}
	public T? NewAssets{get;set;}
	public T? ChangedAssets{get;set;}
	public T? SyncedRoot{get;set;}
}
