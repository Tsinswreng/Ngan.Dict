// Impl：聲明見 AppCfg.cs。
namespace Ngan.Dict.Core.Infra.Cfg;

using Ngan.Dict.Core.Infra.Url;
using Tsinswreng.CsCfg;
using Tsinswreng.CsCfg.Json;

public partial class AppCfg {

	public partial void Setup(I_GetBaseDir BaseDir) {
		// 門面的名字：事件負載與日誌裡用它標識「這一層」
		Name = "app";
		var Dir = BaseDir.GetBaseDir();

		// 1: 出廠那份。檔案不存在就當空源；開關檔與初始讀取都在源的建構子裡。
		var Ro = new JsonFileCfgReader(Path.Combine(Dir, RoCfgFileName), RoSrcName);
		RoCfg = Ro;
		RoCfgPath = Ro.Path;

		// 2: 日常那份。位置由出廠配置的引導鍵決定，故必須先讀過出廠那份——
		//    「配置裡有一個鍵指向配置檔」這種事庫不知道，是本應用的策略。
		var RwCfgRelPath = Ro.Get(KeysClientCfg.RwCfgPath);
		if(str.IsNullOrWhiteSpace(RwCfgRelPath)){
			// 不是「沒有配置檔」，而是「配置檔把引導鍵寫成空串」：啟動期的不變式被破壞。
			// 這裡不拋業務異常——那是給終端用戶看的；這種錯要帶著檔名與鍵名直接崩，便於排查。
			throw new InvalidOperationException(
				$"Read-only config '{RoCfgPath}' resolved '{KeysClientCfg.RwCfgPath}' to an empty path."
			);
		}
		var Rw = new JsonFileCfgAccessor(Path.Combine(Dir, RwCfgRelPath), RwSrcName);
		RwCfg = Rw;
		RwCfgPath = Rw.Path;

		// 3: 疊起來：只讀在前、日常在後，寫入只落日常那份。
		this.SetRoRw([Ro], Rw);
	}
}
