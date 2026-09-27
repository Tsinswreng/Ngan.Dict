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
	- `GenAppJsonCtx`：用 Roslyn 分析 Ngan.Dict.Core，
	  收集 `IAppSerializable`/`IRawValueSerializable` 之實現者，
	  生成 AppJsonCtx（JsonSerializable/JsonSourceGenerationOptions/JsonConverters）之註冊代碼到
	  `Ngan.Dict.Core/Tsinswreng.SrcGen/Ngan.Dict.Gen.g.cs`（單文件）。
	  替代舊的 `GenAppJsonCtx.sh` + Tsinswreng.CsIfaceGen.Cli 流程；
	  型別元資料不再走 Tsinswreng.Srefl 之 `[SreflType]`，改用 Tsinswreng.CsRefl
	  （見 `Ngan.Dict.Core/Infra/AppTypeInfoSrc.cs`）。
	- `TestWinAot`：發布 `Ngan.Dict.Windows.Test` 為 win-x64 NativeAOT 並運行測試 exe，
	  在 AOT 剪裁環境下驗證測試全部通過。
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
