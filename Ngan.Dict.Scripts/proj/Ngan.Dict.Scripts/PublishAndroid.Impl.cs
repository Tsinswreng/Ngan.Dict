using static Tsinswreng.CsSh.ShGlobal;

namespace Ngan.Dict.Scripts;

/// Implements the regular Android publishing entry point.
internal static partial class PublishAndroid{
	internal static async partial Task Main(string Root, CancellationToken Ct) {
		Cd(Root/"Ngan.Dict/Ngan.Dict.Frontend/proj/Ngan.Dict.Android");
		await Exe("dotnet", ["publish", "-c", "Release", "-p:AllowMissingPrunePackageData=true"], Ct);
	}
}
