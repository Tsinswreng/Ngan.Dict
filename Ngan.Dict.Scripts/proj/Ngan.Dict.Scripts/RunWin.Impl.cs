using static Tsinswreng.CsSh.ShGlobal;

namespace Ngan.Dict.Scripts;

/// 本機開發時把 Windows 前端以 Debug 配置建置並啟動。
/// 與 PublishWin 的差別：這裡只求「改完代碼能在本機看到界面」，
/// 所以走 Debug 配置、輸出留在 bin 下，不做打包與去 pdb。
internal static partial class RunWin{
	/// Windows 前端專案（相對倉庫根）。
	const str CsprojRelPath = "Ngan.Dict/Ngan.Dict.Frontend/proj/Ngan.Dict.Windows/Ngan.Dict.Windows.csproj";

	/// 建置產物中的可執行檔名。
	const str ExeName = "Ngan.Dict.Windows.exe";

	internal static async partial Task Main(string Root, CancellationToken Ct) {
		// step 1: 先獨立建置，讓「建置失敗」與「啟動後崩潰」在輸出上分得開。
		await Exe("dotnet", ["build", Root/CsprojRelPath, "-c", "Debug"], Ct);

		// step 2: 外部資源（配置、語言包、sqlite）要落在輸出目錄，程式才讀得到。
		await CpAssets.CpWindowsDebug(Root, Ct);

		// step 3: 以輸出目錄為工作目錄啟動；入口以當前目錄作 BaseDir，日誌也落在那裏。
		// 子程序繼承本終端的 stdout/stderr，啟動期崩潰時看得到原因。
		var OutDir = Root/CpAssets.WindowsDebugRelPath;
		await Exe(
			OutDir/ExeName,
			[],
			new CommandOptions(Cwd: OutDir),
			Ct
		);
	}
}
