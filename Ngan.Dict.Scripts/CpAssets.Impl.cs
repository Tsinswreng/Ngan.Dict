using static Tsinswreng.CsSh.Sh;

namespace Ngan.Dict.Scripts;

/// Implements external resource synchronization without invoking CpAssets.sh.
internal static partial class CpAssets{
	internal static async partial Task<object?> Main(CancellationToken Ct) {
		var PublicResources = ScriptContext.Root / "ExternalRsrc";
		var PrivateResources = ScriptContext.Root / "ExternalRsrc.__Private";
		var WindowsDebug = ScriptContext.FrontendProjects / "Ngan.Dict.Windows/bin/Debug/net10.0";
		var LinuxDebug = ScriptContext.FrontendProjects / "Ngan.Dict.Linux/bin/Debug/net10.0";
		var LinuxPublish = ScriptContext.FrontendProjects / "Ngan.Dict.Linux/bin/Release/net10.0/linux-x64/publish";
		var WindowsPublish = ScriptContext.FrontendProjects / "Ngan.Dict.Windows/bin/Release/net10.0/win-x64/publish";
		var AndroidAssets = ScriptContext.FrontendProjects / "Ngan.Dict.Android/Assets";
		var WebDebug = ScriptContext.Dict / "Ngan.Dict.Server/proj/Ngan.Dict.Server.Http/bin/Debug/net10.0";
		var WebResources = ScriptContext.Dict / "Ngan.Dict.Server/ExternalRsrc";

		// Desktop debug and publish outputs require both public and private resources.
		await CopyPair(PublicResources, PrivateResources, WindowsDebug, Ct);
		await CopyPair(PublicResources, PrivateResources, LinuxDebug, Ct);
		await CopyPair(PublicResources, PrivateResources, LinuxPublish, Ct);
		await CopyPair(PublicResources, PrivateResources, WindowsPublish, Ct);

		// Android receives only public resources, matching the original build asset policy.
		await ScriptContext.CopyContents(PublicResources, AndroidAssets, Ct);
		await ScriptContext.CopyContents(WebResources, WebDebug, Ct);
		return null;
	}

	/// Copies the two resource trees in the source order used by CpAssets.sh.
	private static async Task CopyPair(string PublicResources, string PrivateResources, string Destination, CancellationToken Ct) {
		await ScriptContext.CopyContents(PublicResources, Destination, Ct);
		await ScriptContext.CopyContents(PrivateResources, Destination, Ct);
	}
}
