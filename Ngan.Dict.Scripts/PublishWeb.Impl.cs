using static Tsinswreng.CsSh.Sh;

namespace Ngan.Dict.Scripts;

/// Implements the browser publishing entry point.
internal static partial class PublishWeb{
	internal static async partial Task<object?> Main(CancellationToken Ct) {
		var ProjectDir = ScriptContext.FrontendProjects / "Ngan.Dict.Browser";
		await ScriptContext.XTerm("dotnet publish -c Release", new(Cwd: ProjectDir), Ct);
		return null;
	}
}
