namespace Ngan.Dict.Backend.Db.TswG;
using Tsinswreng.CsSql.Sqlite;

public partial class LocalTblMgr:SqliteTblMgr{
	public static LocalTblMgr Inst => field??= new LocalTblMgr();
}
