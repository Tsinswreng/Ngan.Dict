using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.ShGlobal;

namespace Ngan.Dict.Scripts;

/// Implements external resource synchronization without invoking CpAssets.sh.
internal static partial class CpAssets{
	internal static async partial Task Main(string Root, CancellationToken Ct) {
		var Dict = Root / "Ngan.Dict";
		var FrontendProjects = Dict / "Ngan.Dict.Frontend/proj";
		var PublicResources = Root / "ExternalRsrc";
		var PrivateResources = Root / "ExternalRsrc.__Private";
		var LinuxDebug = FrontendProjects / "Ngan.Dict.Linux/bin/Debug/net10.0";
		var LinuxPublish = FrontendProjects / "Ngan.Dict.Linux/bin/Release/net10.0/linux-x64/publish";
		var WindowsPublish = FrontendProjects / "Ngan.Dict.Windows/bin/Release/net10.0/win-x64/publish";
		var AndroidAssets = FrontendProjects / "Ngan.Dict.Android/Assets";
		var WebDebug = Dict / "Ngan.Dict.Server/proj/Ngan.Dict.Server.Http/bin/Debug/net10.0";
		var WebResources = Dict / "Ngan.Dict.Server/ExternalRsrc";

		// Windows 的 Debug 目錄另拆成獨立入口，供 RunWin 在建置後單獨呼叫。
		await CpWindowsDebug(Root, Ct);
		await CopyPair(PublicResources, PrivateResources, LinuxDebug, Ct);
		await CopyPair(PublicResources, PrivateResources, LinuxPublish, Ct);

		// Windows publish intentionally receives only public resources; private files are not distributable there.
		await Cp(PublicResources / "*", WindowsPublish, Ct);

		// Android receives only public resources, matching the original build asset policy.
		await Cp(PublicResources / "*", AndroidAssets, Ct);
		await Cp(WebResources / "*", WebDebug, Ct);
	}

	internal static async partial Task CpWindowsDebug(string Root, CancellationToken Ct) {
		await CopyPair(
			Root / "ExternalRsrc",
			Root / "ExternalRsrc.__Private",
			Root / WindowsDebugRelPath,
			Ct
		);
	}

	/// Copies the two resource trees in the source order used by CpAssets.sh.
	/// 私有樹與公開樹有同名檔（如 `Ngan.Dict.jsonc`），私有版才是有本機值的那份，
	/// 故一律覆蓋——等同舊 shell 流程的 `cp -rf` 語義。
	private static async Task CopyPair(string PublicResources, string PrivateResources, string Destination, CancellationToken Ct) {
		var Options = new CpOptions(Overwrite: true);
		await Cp(PublicResources / "*", Destination, Options, Ct);
		await Cp(PrivateResources / "*", Destination, Options, Ct);
	}
}
