using System.Diagnostics.Contracts;
using Ngaq.Core.Infra;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.StudyPlan.Models.Po.StudyPlan;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Tsinswreng.CsErr;
using Tsinswreng.CsSql;
using Tsinswreng.CsTextWithBlob;

namespace Ngaq.Core.Shared.Sync;

public class EntitySyncerInMem<T>: IEntitySyncerInMem<T>
	where T: IPoBase, IBizCreateUpdateTime
{
	
}

[Doc(@$"實體(Po)內存中 同步邏輯。
每個成員都是純函數。
")]
public interface IEntitySyncerInMem<T>
	where T: IPoBase, IBizCreateUpdateTime
{
	
	[Doc(@$"適用于單個獨立的實體(包括聚合類的資產)、
	不適用于聚合類(如{nameof(JnWord)})")]
	[Pure]
	public int DiffPoByTime(
		T X, T Y
	){
		if(X.BizUpdatedAt == Y.BizUpdatedAt
			&& X.BizCreatedAt == Y.BizCreatedAt
			&& X.DelAt == Y.DelAt
		){
			return 0;
		}
		var xUpd = X.GetNewestBizUpdOrDelTime();
		var yUpd = Y.GetNewestBizUpdOrDelTime();
		return xUpd.CompareTo(yUpd);
		//throw new Exception("Id相同時 BizCreatedAt 不相等");
	}
	
	
	[Doc(@$"適用于單個獨立的實體(包括聚合類的資產)、
	不適用于聚合類(如{nameof(JnWord)})")]
	public IDtoEntityDiffEtSync<T> Sync(
		T Local, T Remote
	){
		if(!Local.Id_().EqObj(Remote.Id_())){
			throw KeysErr.Sync.SyncShouldUseSameId.ToErr();
		}
		var diff = DiffPoByTime(Local, Remote);
		var r = new DtoEntityDiffEtSync<T>();
		r.LocalCompareToRemote = diff;
		if(diff == 0){
			r.SyncedEntity = default;
		}
		else if(diff > 0){
			r.SyncedEntity = Remote;
		}
		else{
			r.SyncedEntity = default;
		}
		return r;
	}
}

[Doc(@$"實際操作數據庫")]
public interface IEntitySyncerDb<TPo, TId>
	where TPo: class, IPoBase, IBizCreateUpdateTime, I_Id<TId>, new()
{
	public IAsyncEnumerable<DtoEntityDiffEtSync<TPo>> OrdSyncPo(
		IDbFnCtx Ctx, IAsyncEnumerable<TPo> Pos, CT Ct
	);
}

