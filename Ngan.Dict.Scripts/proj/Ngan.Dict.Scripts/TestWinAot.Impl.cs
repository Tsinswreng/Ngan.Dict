using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.ShGlobal;

namespace Ngan.Dict.Scripts;

/// Implements the Windows AOT test entry point.
internal static partial class TestWinAot{
	internal static async partial Task Main(string Root, CancellationToken Ct) {
		var ProjectDir = Root/"Ngan.Dict/Ngan.Dict.Test/proj/Ngan.Dict.Windows.Test";
		var PublishExe = ProjectDir/"bin/Release/net10.0/win-x64/publish/Ngan.Dict.Windows.Test.exe";

		// 先以 NativeAOT 發布（csproj 已設 PublishAot=true），再運行發布物，
		// 驗證 AOT 剪裁後測試仍全部通過。
		Cd(ProjectDir);
		await Exe("dotnet", ["publish", "-c", "Release", "-r", "win-x64"], Ct);
		// 測試輸出必須轉送終端，否則看不到結果。
		await Cmd(PublishExe, [], Ct).Out(Ct);
	}
}
