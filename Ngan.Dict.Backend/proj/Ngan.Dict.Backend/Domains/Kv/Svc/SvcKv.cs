namespace Ngan.Dict.Backend.Domains.Kv.Svc;

using Ngan.Dict.Core.Shared.Kv.Models;
using Ngan.Dict.Core.Shared.Kv.Svc;
using Ngan.Dict.Core.Shared.User.Models.Po.User;
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

	/// KV 查詢是純讀:不開事務。流式入口 = SqlFlow.Batches 切流 → 逐批調 IList 批核心。
	public IAsyncEnumerable<PoKv?> OrdGetByOwnerEtKStr(
		IDbFnCtx? Ctx, IAsyncEnumerable<(IdUser, str)> Owner_Key, CT Ct
	){
		return Impl(Ct);

		async IAsyncEnumerable<PoKv?> Impl([System.Runtime.CompilerServices.EnumeratorCancellation] CT Ct2){
			await foreach(var one in SqlFlow.Batches<(IdUser, str), PoKv?>(
				Owner_Key,
				async(batch, ct2)=> (IEnumerable<PoKv?>)(await OrdGetByOwnerEtKStr(Ctx, batch, ct2)),
				Ct2
			)){
				yield return one;
			}
		}
	}

	// ■ IList 批核心:函數只管一批——整批一次查(純讀、不開事務、位置對齊)。
	public async Task<IList<PoKv?>> OrdGetByOwnerEtKStr(
		IDbFnCtx? Ctx, IList<(IdUser, str)> Owner_Key, CT Ct
	){
		if(Owner_Key.Count == 0){
			return [];
		}
		Ctx ??= new DbFnCtx();
		return await DaoKv.BatGetByOwnerEtKStr(Ctx, Owner_Key, Ct);
	}

	public IAsyncEnumerable<PoKv?> OrdGetByOwnerEtKI64(
		IDbFnCtx? Ctx, IAsyncEnumerable<(IdUser, i64)> Owner_Key
		,CT Ct
	){
		return Impl(Ct);

		async IAsyncEnumerable<PoKv?> Impl([System.Runtime.CompilerServices.EnumeratorCancellation] CT Ct2){
			await foreach(var one in SqlFlow.Batches<(IdUser, i64), PoKv?>(
				Owner_Key,
				async(batch, ct2)=> (IEnumerable<PoKv?>)(await OrdGetByOwnerEtKI64(Ctx, batch, ct2)),
				Ct2
			)){
				yield return one;
			}
		}
	}

	// ■ IList 批核心:函數只管一批——整批一次查(純讀、不開事務、位置對齊)。
	public async Task<IList<PoKv?>> OrdGetByOwnerEtKI64(
		IDbFnCtx? Ctx, IList<(IdUser, i64)> Owner_Key, CT Ct
	){
		if(Owner_Key.Count == 0){
			return [];
		}
		Ctx ??= new DbFnCtx();
		return await DaoKv.BatGetByOwnerEtKI64(Ctx, Owner_Key, Ct);
	}

	/// OrdSet(Upsert)是寫操作:整操作一個事務。
	/// 流式入口 = EnsureTxn 整流 + BatchesInOnly 逐批調 IList 批核心。
	public Task<nil> OrdSet(
		IDbFnCtx? Ctx
		,IAsyncEnumerable<PoKv> Kvs, CT Ct
	){
		return SqlCmdMkr.EnsureTxn(Ctx, Ct, (DbCtx)=>{
			return SqlFlow.BatchesInOnly(Kvs, (batch, ct2)=> OrdSet(DbCtx, batch, ct2), Ct);
		});
	}

	// ■ IList 批核心:函數只管一批——整批一次 Upsert(EnsureTxn 可嵌套:有外層只加入)。
	public Task<nil> OrdSet(
		IDbFnCtx? Ctx
		,IList<PoKv> Kvs, CT Ct
	){
		return SqlCmdMkr.EnsureTxn(Ctx, Ct, async(DbCtx)=>{
			await RepoKv.OrdSet(DbCtx, Kvs, Ct);
			return NIL;
		});
	}
}