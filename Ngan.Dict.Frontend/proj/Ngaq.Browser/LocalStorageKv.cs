using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Core.Shared.Kv.Svc;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Tools.Json;
using Tsinswreng.CsCore;
using Tsinswreng.CsSql;

namespace Ngaq.Browser;

[Doc(@$"用瀏覽器 localStorage 實現。
用 {nameof(IJsonSerializer)} 把 {nameof(PoKv)} 序列化成json之後存到值。
")]
public class LocalStorageKv : ISvcKv {

	public IAsyncEnumerable<PoKv?> OrdGetByOwnerEtKI64(IDbFnCtx? Ctx, IAsyncEnumerable<(IdUser, long)> Owner_Key, CT Ct) {
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<PoKv?> OrdGetByOwnerEtKStr(IDbFnCtx? Ctx, IAsyncEnumerable<(IdUser, string)> Owner_Key, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<nil> OrdSet(IDbFnCtx? Ctx, IAsyncEnumerable<PoKv> Kvs, CT Ct) {
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
