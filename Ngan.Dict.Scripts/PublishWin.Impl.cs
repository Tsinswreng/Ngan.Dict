using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.Sh;

namespace Ngan.Dict.Scripts;

/// Implements the Windows publishing entry point.
internal static partial class PublishWin{
	internal static async partial Task<object?> Main(CancellationToken Ct) {
		var ProjectDir = ScriptContext.FrontendProjects / "Ngan.Dict.Windows";
		var PublishDir = ProjectDir / "bin/Release/net10.0/win-x64/publish";
		var PublishNoPdbDir = ProjectDir / "bin/Release/net10.0/win-x64/publishNoPdb";
		var ArchivePath = ProjectDir / "bin/Release/net10.0/win-x64/Ngan.Dict.Windows.tar.gz";

		Cd(ProjectDir);
		await Rm("publishOld", Ct);
		if (await Exists("publish", Ct))
			await Mv("publish", "publishOld", Ct);
		await ScriptContext.XTerm("dotnet publish -c Release -r win-x64 -p:AllowMissingPrunePackageData=true", null, Ct);
		await Rm("publishOld", Ct);
		await CpAssets.Main(Ct);

		await Rm(PublishNoPdbDir, Ct);
		await ScriptContext.CopyContents(PublishDir, PublishNoPdbDir, Ct);
		await foreach (var Entry in Find(PublishNoPdbDir / "**/*.pdb", Ct))
			await Rm(Entry.Path, Ct);
		await Rm(ArchivePath, Ct);
		await ScriptContext.XTerm($"tar -czf \"{ArchivePath}\" .", new(Cwd: PublishNoPdbDir), Ct);
		await Mv(ArchivePath, PublishNoPdbDir / "Ngan.Dict.Windows.tar.gz", Ct);
		return null;
	}
}
