namespace Ngan.Dict.Scripts;

/// Synchronizes external runtime resources into each Ngan.Dict output or Android asset location.
internal static partial class CpAssets{
	/// Windows 前端 Debug 輸出目錄（相對倉庫根）。
	/// CpAssets 與 RunWin 共用同一份路徑，避免兩處各寫一份而走鐘。
	internal const str WindowsDebugRelPath = "Ngan.Dict/Ngan.Dict.Frontend/proj/Ngan.Dict.Windows/bin/Debug/net10.0";

	/// Copies public and private resources to the platform-specific destinations used by the original shell script.
	internal static partial Task Main(string Root, CancellationToken Ct);

	/// 只把公開與私有資源同步到 Windows 前端的 Debug 輸出目錄。
	/// RunWin 在建置後呼叫，讓本機啟動前一定拿到最新的配置與資源。
	internal static partial Task CpWindowsDebug(string Root, CancellationToken Ct);
}
