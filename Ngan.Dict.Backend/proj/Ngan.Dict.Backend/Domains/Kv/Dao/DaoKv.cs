namespace Ngan.Dict.Backend.Domains.Kv.Dao;

using Ngan.Dict.Core.Shared.Kv.Models;
using Ngan.Dict.Core.Shared.User.Models;
using Ngan.Dict.Core.Shared.User.Models.Po.User;
using Ngan.Dict.Core.Shared.User.UserCtx;
using Ngan.Dict.Core.Shared.Word.Models.Po.Kv;
using Ngan.Dict.Backend.Db.TswG;
using Tsinswreng.CsCfg;
using Tsinswreng.CsCore;
using Tsinswreng.CsPage;
using Tsinswreng.CsSql;
using Tsinswreng.CsTools;


using Z = DaoKv;
public partial class DaoKv(
	ISqlCmdMkr SqlCmdMkr
	,ITblMgr TblMgr
	, IRepo<PoKv, IdKv> RepoKv
){

	ISqlCmdMkr SqlCmdMkr = SqlCmdMkr;

	//ITable<PoKv> T{get=>TblMgr.GetTbl<PoKv>();}

	protected ITable<PoKv> T{
		get{return TblMgr.GetTbl<PoKv>();}
	}

	// -------- IList 批核心(新範式)--------
	// 每方法吃整批 IList、內部零業務分批;批大小/切批是調用方(Svc 流式入口的 SqlFlow.Batches)的職責。

	/// IList 版：批量按 (Owner, Key) 複合鍵查 PoKv(不帶軟刪行)。
	/// 位置一一對應、查無補 null。
	/// 實現要點:Owner/Key 各 Many 同步綁定——Build 產多語句一命令、泛型 Get1d 逐結果集回讀。
	public async Task<IList<PoKv?>> BatGetByOwnerEtKStr(
		IDbFnCtx Ctx,
		IList<(IdUser Owner, str Key)> Owner_Key,
		CT Ct
	){
		if(Owner_Key.Count == 0){
			return [];
		}
		var Owners = new List<obj?>(Owner_Key.Count);
		var Keys = new List<obj?>(Owner_Key.Count);
		foreach(var (Owner, Key) in Owner_Key){
			Owners.Add(Owner);
			Keys.Add(Key);
		}
		var Sql = T.SqlSplicer().Select("*").FromT().WhereNonDel()
			.And().Bool(nameof(PoKv.Owner), "=", x=>x.Many(Owners))
			.And().Bool(nameof(PoKv.KStr), "=", x=>x.Many(Keys));
		var Ans = await SqlCmdMkr.Get1d(Ctx, T, Sql.Build(), Ct)
			.ToListAsync(Ct);
		return Ans;
	}

	/// IList 版：批量按 (Owner, Key) 複合鍵查 PoKv(不帶軟刪行)。
	/// 位置一一對應、查無補 null。同 BatGetByOwnerEtKStr 的 IList 版、只是 Key 列為 KI64。
	public async Task<IList<PoKv?>> BatGetByOwnerEtKI64(
		IDbFnCtx Ctx,
		IList<(IdUser Owner, i64 Key)> Owner_Key,
		CT Ct
	){
		if(Owner_Key.Count == 0){
			return [];
		}
		var Owners = new List<obj?>(Owner_Key.Count);
		var Keys = new List<obj?>(Owner_Key.Count);
		foreach(var (Owner, Key) in Owner_Key){
			Owners.Add(Owner);
			Keys.Add(Key);
		}
		var Sql = T.SqlSplicer().Select("*").FromT().WhereNonDel()
			.And().Bool(nameof(PoKv.Owner), "=", x=>x.Many(Owners))
			.And().Bool(nameof(PoKv.KI64), "=", x=>x.Many(Keys));
		var Ans = await SqlCmdMkr.Get1d(Ctx, T, Sql.Build(), Ct)
			.ToListAsync(Ct);
		return Ans;
	}
	
}