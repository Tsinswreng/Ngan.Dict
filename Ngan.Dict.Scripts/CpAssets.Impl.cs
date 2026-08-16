using static Tsinswreng.CsSh.ShGlobal;

namespace Ngan.Dict.Scripts;

/// Implements external resource synchronization without invoking CpAssets.sh.
internal static partial class CpAssets{
	internal static async partial Task Main(string Root, CancellationToken Ct) {
		var Dict = Root / "Ngan.Dict";
		var FrontendProjects = Dict / "Ngan.Dict.Frontend/proj";
		var PublicResources = Root / "ExternalRsrc";
		var PrivateResources = Root / "ExternalRsrc.__Private";
		var WindowsDebug = FrontendProjects / "Ngan.Dict.Windows/bin/Debug/net10.0";
		var LinuxDebug = FrontendProjects / "Ngan.Dict.Linux/bin/Debug/net10.0";
		var LinuxPublish = FrontendProjects / "Ngan.Dict.Linux/bin/Release/net10.0/linux-x64/publish";
		var WindowsPublish = FrontendProjects / "Ngan.Dict.Windows/bin/Release/net10.0/win-x64/publish";
		var AndroidAssets = FrontendProjects / "Ngan.Dict.Android/Assets";
		var WebDebug = Dict / "Ngan.Dict.Server/proj/Ngan.Dict.Server.Http/bin/Debug/net10.0";
		var WebResources = Dict / "Ngan.Dict.Server/ExternalRsrc";

		// These three targets receive both resource trees, matching the original shell script's order.
		await CopyPair(PublicResources, PrivateResources, WindowsDebug, Ct);
		await CopyPair(PublicResources, PrivateResources, LinuxDebug, Ct);
		await CopyPair(PublicResources, PrivateResources, LinuxPublish, Ct);

		// Windows publish intentionally receives only public resources; private files are not distributable there.
		await Cp(PublicResources / "*", WindowsPublish, Ct);

		// Android receives only public resources, matching the original build asset policy.
		await Cp(PublicResources / "*", AndroidAssets, Ct);
		await Cp(WebResources / "*", WebDebug, Ct);
	}

	/// Copies the two resource trees in the source order used by CpAssets.sh.
	private static async Task CopyPair(string PublicResources, string PrivateResources, string Destination, CancellationToken Ct) {
		await Cp(PublicResources / "*", Destination, Ct);
		await Cp(PrivateResources / "*", Destination, Ct);
	}
}
