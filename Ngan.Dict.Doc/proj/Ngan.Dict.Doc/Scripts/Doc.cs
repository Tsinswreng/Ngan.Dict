namespace Ngan.Dict.Doc.Scripts;

using Tsinswreng.CsCore;

[Doc($$"""
#H[腳本程序集][
	`Ngan.Dict.Scripts` 是 Ngan.Dict 專用的維護腳本程序集。
	它以單一 dispatcher 作為 CLR 入口，
	由第一個命令列引數分派至具名腳本，避免多個真正的 C# `Main` 入口造成編譯衝突。

	從 Ngan 工作區執行格式為：
	`dotnet run --project Ngan.Dict/Ngan.Dict.Scripts -- <入口>`。
	腳本根目錄由 `Program.cs` 的 CallerFilePath 向上推得，
	不依賴呼叫時的目前工作目錄。
	外部命令由 `X(...)` 建立，預設不轉送輸出；需要顯示終端輸出時使用 `await X(..., Ct).Out(Ct)`。
]

#H[可用入口][
	- `PublishWeb`：發布 Browser 專案。
	- `PublishWin`：發布 Windows、同步資源、移除 PDB 並產生 tar.gz。
	- `PublishLinux`：發布 Linux、同步資源、移除 PDB 並產生 tar.gz。
	- `PublishAndroid`：以 Mono runtime 發布 Android APK。
	- `PublishAndroidNativeAot`：以 WSL/Linux Android SDK、NDK 發布 NativeAOT APK。
	- `CpAssets`：同步外部資源到各目標輸出目錄。
]

#H[NativeAOT 約束][
	`PublishAndroidNativeAot` 只傳入
	`Ngan.DictAndroidNativeAot=true`，
	由 Android 主項目將此專用開關轉為 NativeAOT 屬性。
	不得在命令列傳入全局 `PublishAot=true`，
	否則屬性會傳播到不支援 NativeAOT 的 ProjectReference。
]

""")]
file class _{

}
