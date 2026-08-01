namespace Ngaq.Core.Shared.Sync;

[Doc(@$"比較單個實體。
用于同步。
設把Remote合入Local。
")]
public interface IDtoEntityDiffEtSync<T>{
	[Doc(@$"
	0:一樣新;
	正數:Local更加新;
	負數:Remote更加新
	")]
	public i32 LocalCompareToRemote{get;set;}
	[Doc(@$"同步後的實體。爲null時表示不需要同步")]
	public T? SyncedEntity{get;set;}
}

public class DtoEntityDiffEtSync<T>:IDtoEntityDiffEtSync<T>
{
	public i32 LocalCompareToRemote{get;set;}
	public T? SyncedEntity{get;set;}
}
