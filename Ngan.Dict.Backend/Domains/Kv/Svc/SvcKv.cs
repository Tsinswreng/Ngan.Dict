namespace Ngan.Dict.Backend.Domains.Kv.Svc;

using Ngan.Dict.Core.Shared.Kv.Models;
using Ngan.Dict.Core.Shared.Kv.Svc;
using Ngan.Dict.Core.Shared.User.Models.Po.User;
using Ngan.Dict.Core.Shared.Word.Models.Po.Kv;
using Ngan.Dict.Backend.Db.TswG;
using Ngan.Dict.Backend.Domains.Kv.Dao;
using Tsinswreng.CsCore;
using Tsinswreng.CsSql;

using Z = SvcKv;
public partial class SvcKv(
	DaoKv DaoKv
	,IRepo<PoKv, IdKv> RepoKv
	,ISqlCmdMkr SqlCmdMkr
)
	:ISvcKv
{
	DaoKv DaoKv = DaoKv;
	IRepo<PoKv, IdKv> RepoKv = RepoKv;

	public IAsyncEnumerable<PoKv?> OrdGetByOwnerEtKStr(
		IDbFnCtx? Ctx, IAsyncEnumerable<(IdUser, str)> Owner_Key, CT Ct
	) {
		Ctx ??= new DbFnCtx();
		return DaoKv.BatGetByOwnerEtKStr(Ctx, Owner_Key, Ct);
	}

	public IAsyncEnumerable<PoKv?> OrdGetByOwnerEtKI64(
		IDbFnCtx? Ctx, IAsyncEnumerable<(IdUser, i64)> Owner_Key
		,CT Ct
	){
		Ctx ??= new DbFnCtx();
		return DaoKv.BatGetByOwnerEtKI64(Ctx, Owner_Key, Ct);
	}
	
	public async Task<nil> OrdSet(
		IDbFnCtx? Ctx
		,IAsyncEnumerable<PoKv> Kvs, CT Ct
	){
		return await SqlCmdMkr.EnsureTxn(Ctx, Ct, (Ctx)=>{
			return RepoKv.OrdUpsert(Ctx, Kvs, Ct);
		});
	}
}
