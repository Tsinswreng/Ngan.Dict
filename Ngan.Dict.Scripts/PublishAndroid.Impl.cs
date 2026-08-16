using static Tsinswreng.CsSh.ShGlobal;

namespace Ngan.Dict.Scripts;

/// Implements the regular Android publishing entry point.
internal static partial class PublishAndroid{
	internal static async partial Task Main(string Root, CancellationToken Ct) {
		var ProjectDir = Root / "Ngan.Dict/Ngan.Dict.Frontend/proj/Ngan.Dict.Android";
		Cd(ProjectDir);
		await X("dotnet publish -c Release -p:AllowMissingPrunePackageData=true", Ct).Out(Ct);
	}
}
