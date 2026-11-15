using Ngan.Dict.Backend.Db.TswG.Migrations;
using Tsinswreng.CsSql;

namespace Ngan.Dict.Backend.Db.TswG;

/// Local 端遷移清單註冊（僅保留業務選擇，不包含通用去重邏輯）。
public static class LocalMigrations{
	extension(IMigrationMgr z){
		/// 註冊 Local 端全部遷移。
		/// 通用去重策略由 CsSql.ExtnMigrationMgr 提供。
		///
		/// 約定：
		/// - 這裏只列出「Local 端有哪些遷移」
		/// - 不在這裏做執行，不在這裏做歷史表寫入
		/// - 新增 Local 遷移時，只需要把新類追加到這個列表裏
		public IMigrationMgr UseLocalMigrations(IServiceProvider Sp){
			z.AddMigrationsIfAbsent([
				// 唯一一支：按當前表配置建出完整結構。
				// 舊版的增量遷移已全部刪除（本應用舊版無實際使用者，不需要升級路徑）。
				new MInit(Sp)
			]);
			return z;
		}
	}
}
