// namespace Ngan.Dict.Core.Shared.Kv.Svc;

// using Ngan.Dict.Core.Shared.Kv.Models;
// using Ngan.Dict.Core.Shared.User.Models.Po.User;
// using Tsinswreng.CsSql;
// using Tsinswreng.CsTools;

// public partial interface ISvcKv{
	
// 	[Obsolete]
// 	public Task<Func<
// 		IdUser
// 		,obj
// 		,CT, Task<PoKv?>
// 	>> FnGetByOwnerEtKey(IDbFnCtx Ctx, CT Ct);
// 	[Obsolete]
// 	public Task<Func<
// 		PoKv
// 		,CT, Task<nil>
// 	>> FnSet(IDbFnCtx Ctx, CT Ct);

// 	[Obsolete]
// 	public Task<nil> Set(PoKv Po, CT Ct);

// 	[Obsolete]
// 	public Task<Func<
// 		IEnumerable<PoKv>
// 		,CT, Task<nil>
// 	>> FnSetMany(IDbFnCtx Ctx, CT Ct);
// 	public Task<nil> SetMany(IEnumerable<PoKv> Pos, CT Ct);
// }
