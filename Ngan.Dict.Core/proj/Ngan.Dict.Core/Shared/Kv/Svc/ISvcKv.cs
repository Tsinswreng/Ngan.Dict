namespace Ngan.Dict.Core.Shared.Kv.Svc;

using Ngan.Dict.Core.Shared.Kv.Models;
using Ngan.Dict.Core.Shared.User.Models.Po.User;
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

	// -------- IList 批核心形狀(新範式)--------
	// 每方法兩個形狀:流式入口(IAsyncEnumerable、對外)+ IList 批核心(同名重載、函數只管一批)。
	// 寫操作批核心內部 EnsureTxn 可嵌套(有外層只加入、無外層自開自提交);純讀不開事務。

	[Doc(@$"IList 版 `{nameof(OrdGetByOwnerEtKI64)}`:純讀、不開事務,一批一次查。
	出參 IList 與入參位置一一對應、查無補 null。
	#See[{nameof(ISvcKv.OrdGetByOwnerEtKI64)}]")]
	public Task<IList<PoKv?>> OrdGetByOwnerEtKI64(
		IDbFnCtx? Ctx, IList<(IdUser, i64)> Owner_Key, CT Ct
	);

	[Doc(@$"IList 版 `{nameof(OrdGetByOwnerEtKStr)}`:純讀、不開事務,一批一次查。
	出參 IList 與入參位置一一對應、查無補 null。
	#See[{nameof(ISvcKv.OrdGetByOwnerEtKStr)}]")]
	public Task<IList<PoKv?>> OrdGetByOwnerEtKStr(
		IDbFnCtx? Ctx, IList<(IdUser, str)> Owner_Key, CT Ct
	);

	[Doc(@$"IList 版 `{nameof(OrdSet)}`(Upsert):一批一次寫(整批一個事務)。
	#See[{nameof(ISvcKv.OrdSet)}]")]
	public Task<nil> OrdSet(
		IDbFnCtx? Ctx, IList<PoKv> Kvs, CT Ct
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
