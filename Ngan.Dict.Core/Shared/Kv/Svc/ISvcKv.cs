namespace Ngaq.Core.Shared.Kv.Svc;

using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Core.Shared.User.Models.Po.User;
using Tsinswreng.CsSql;
using Tsinswreng.CsTools;


/// TODO  增接口㕥刪(軟/硬);
[Doc(@$"#See[{nameof(PoKv)}]")]
public partial interface ISvcKv{
	public IAsyncEnumerable<PoKv?> OrdGetByOwnerEtKI64(
		IDbFnCtx? Ctx, IAsyncEnumerable<(IdUser, i64)> Owner_Key, CT Ct
	);
	
	public IAsyncEnumerable<PoKv?> OrdGetByOwnerEtKStr(
		IDbFnCtx? Ctx, IAsyncEnumerable<(IdUser, str)> Owner_Key, CT Ct
	);
	
	[Doc("Upsert")]
	public Task<nil> OrdSet(
		IDbFnCtx? Ctx
		,IAsyncEnumerable<PoKv> Kvs, CT Ct
	);
}

public static class ExtnISvcKv{
	extension(ISvcKv z){
		public async Task<PoKv?> GetByOwnerEtKStr(IdUser Owner, str Key, CT Ct){
			var kvs = z.OrdGetByOwnerEtKStr(null, ToolAsyE.ToAsyE([(Owner, Key)]), Ct);
			var first = await kvs.FirstOrDefaultAsync(Ct);
			return first;
		}
	}
}
