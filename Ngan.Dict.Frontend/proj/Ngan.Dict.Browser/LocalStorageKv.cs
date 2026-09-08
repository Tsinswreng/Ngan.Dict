using Ngan.Dict.Core.Shared.Kv.Models;
using Ngan.Dict.Core.Shared.Kv.Svc;
using Ngan.Dict.Core.Shared.User.Models.Po.User;
using Ngan.Dict.Core.Tools.Json;
using Tsinswreng.CsCore;
using Tsinswreng.CsSql;

namespace Ngan.Dict.Browser;

[Doc(@$"用瀏覽器 localStorage 實現。
用 {nameof(IJsonSerializer)} 把 {nameof(PoKv)} 序列化成json之後存到值。
")]
public class LocalStorageKv : ISvcKv {

	public IAsyncEnumerable<PoKv?> OrdGetByOwnerEtKI64(IDbFnCtx? Ctx, IAsyncEnumerable<(IdUser, long)> Owner_Key, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<IList<PoKv?>> OrdGetByOwnerEtKI64(IDbFnCtx? Ctx, IList<(IdUser, long)> Owner_Key, CT Ct) {
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<PoKv?> OrdGetByOwnerEtKStr(IDbFnCtx? Ctx, IAsyncEnumerable<(IdUser, string)> Owner_Key, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<IList<PoKv?>> OrdGetByOwnerEtKStr(IDbFnCtx? Ctx, IList<(IdUser, string)> Owner_Key, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<nil> OrdSet(IDbFnCtx? Ctx, IAsyncEnumerable<PoKv> Kvs, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<nil> OrdSet(IDbFnCtx? Ctx, IList<PoKv> Kvs, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<Func<IdUser, object, CT, Task<PoKv?>>> FnGetByOwnerEtKey(IDbFnCtx Ctx, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<Func<PoKv, CT, Task<nil>>> FnSet(IDbFnCtx Ctx, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<Func<IEnumerable<PoKv>, CT, Task<nil>>> FnSetMany(IDbFnCtx Ctx, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<nil> Set(PoKv Po, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<nil> SetMany(IEnumerable<PoKv> Pos, CT Ct) {
		throw new NotImplementedException();
	}
}
