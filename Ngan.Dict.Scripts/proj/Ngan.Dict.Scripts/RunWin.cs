namespace Ngan.Dict.Scripts;

/// 本機開發時把 Windows 前端以 Debug 配置建置並啟動。
internal static partial class RunWin{
	/// 執行：build -c Debug → 同步外部資源到輸出目錄 → 從輸出目錄啟動 exe。
	internal static partial Task Main(string Root, CancellationToken Ct);
}
