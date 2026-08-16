namespace Ngan.Dict.Scripts;

/// Implements the regular Android publishing entry point.
internal static partial class PublishAndroid{
	internal static async partial Task<object?> Main(CancellationToken Ct) {
		var ProjectDir = ScriptContext.FrontendProjects / "Ngan.Dict.Android";
		await ScriptContext.XTerm("dotnet publish -c Release -p:AllowMissingPrunePackageData=true", new(Cwd: ProjectDir), Ct);
		return null;
	}
}
