using Ngan.Dict.Core.Infra.IF;
using Ngan.Dict.Core.Shared.Dictionary.Models;
using Ngan.Dict.Core.Shared.Word.Models.Po.NormLangToUserLang;
using Tsinswreng.CsSql;

namespace Ngan.Dict.Backend.Db.TswG;

public partial class LocalTblMgrIniter{
	public static ITblMgr InitNormLangToUserLang(ITblMgr Mgr){
		var Tbl_NormLangToUserLang = Mk<PoNormLangToUserLang>("NormLangToUserLang");
		Mgr.AddTbl(Tbl_NormLangToUserLang);
		{
			var o = Tbl_NormLangToUserLang;
			CfgPoBase(o);
			CfgBizCreateUpdateTime(o);
			o.Col(x=>x.Id).MapType(IdNormLangToUserLang.MkTypeMapFn());
			o.Col(x=>x.Owner).MapType(MapIdUser());
			o.Col(x=>x.NormLangType).MapEnumToStr<ELangIdentType>();
			o.IdxExpr(
				new OptMkIdx{
					Unique = true,
					Where = o.Tbl.SqlIsNonDel()
				},
				x=>new{x.Owner, x.NormLang, x.NormLangType}
			);
			// 依 UserLang 查詢標準語言映射。
			o.IdxExpr(null, x=>new{x.Owner, x.UserLang});
		}
		return Mgr;
	}
}
