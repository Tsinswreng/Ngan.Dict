using static Tsinswreng.CsSh.Sh;

namespace Ngan.Dict.Scripts;

/// Implements the browser publishing entry point.
internal static partial class PublishWeb{
	internal static async partial Task Main(string Root, CancellationToken Ct) {
		var ProjectDir = Root / "Ngan.Dict/Ngan.Dict.Frontend/proj/Ngan.Dict.Browser";
		Cd(ProjectDir);
		await X("dotnet publish -c Release", Ct).Out(Ct);
	}
}
