using Microsoft.Extensions.DependencyInjection;
using Tsinswreng.CsCore;
using Tsinswreng.CsSql;

namespace Ngan.Dict.Backend.Db.TswG.Migrations;

[Doc($$"""
本地庫的**唯一**一支遷移：按當前表配置一次建出完整結構。

*為何只剩一支*：本應用舊版沒有實際使用者，故歷史上的增量遷移（改列型別、補表）
一律刪除——留著它們只會讓每個新庫多跑幾十條無意義的 DDL。
新庫直接建當前結構；已存在的舊庫本來就要重建，不需要升級路徑。

*以後要加結構*：新增一支 `M<日期>_<時間>` 遷移類，追加到
`LocalMigrations.UseLocalMigrations` 的清單尾部；別改這一支（已發佈的遷移不可變）。
""")]
public class MInit:SqlMigrationInfo{
	readonly IMkrTblMgr MkrTblMgr;

	public MInit(
		IServiceProvider Sp
	){
		this.MkrTblMgr = Sp.GetRequiredService<IMkrTblMgr>();
		Init();
	}

	/// 本支遷移的版本號：取本次重整結構的時刻，須大於任何已存在的歷史版本。
	public override i64 CreatedMs{get;set;} = 1762000000000;

	/// 註冊當前全部表（含遷移歷史表本身），再由表管理器生成建表 DDL。
	void Init(){
		var Mgr = MkrTblMgr.MkTblMgr();
		// 遷移歷史表也屬於建庫內容的一部分，否則下次啟動就查不到「已跑過哪些遷移」。
		Mgr.AddTbl(new SchemaHistoryTblMkr().MkTbl());
		new LocalTblMgrIniter(Mgr).Init();
		SqlsUp = Mgr.SqlsMkSchema();
	}
}
