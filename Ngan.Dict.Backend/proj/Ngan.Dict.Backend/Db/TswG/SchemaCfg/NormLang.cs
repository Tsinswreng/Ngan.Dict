using Ngan.Dict.Core.Infra.IF;
using Ngan.Dict.Core.Shared.Dictionary.Models;
using Ngan.Dict.Core.Shared.Dictionary.Models.Po.NormLang;
using Tsinswreng.CsSql;

namespace Ngan.Dict.Backend.Db.TswG;

public partial class LocalTblMgrIniter{
	public static ITblMgr InitNormLang(ITblMgr Mgr){
		var Tbl_NormLang = Mk<PoNormLang>("NormLang");
		Mgr.AddTbl(Tbl_NormLang);
		{
			var o = Tbl_NormLang;
			CfgPoBase(o);
			CfgBizCreateUpdateTime(o);
			o.Col(x=>x.Id).MapType(IdNormLang.MkTypeMapFn());
			o.Col(x=>x.Owner).MapType(MapIdUser());
			o.Col(x=>x.Type).MapEnumToStr<ELangIdentType>();
			o.IdxExpr(
				new OptMkIdx{
					Unique = true,
					Where = o.Tbl.SqlIsNonDel()
				},
				x=>new{x.Owner, x.Code, x.Type}
			);
		}
		return Mgr;
	}
}
