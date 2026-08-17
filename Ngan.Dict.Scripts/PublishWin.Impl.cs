using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.ShGlobal;

namespace Ngan.Dict.Scripts;

/// Implements the Windows publishing entry point.
internal static partial class PublishWin{
	internal static async partial Task Main(string Root, CancellationToken Ct) {
		var ProjectDir = Root/"Ngan.Dict/Ngan.Dict.Frontend/proj/Ngan.Dict.Windows";
		var PublishDir = ProjectDir/"bin/Release/net10.0/win-x64/publish";
		var PublishNoPdbDir = ProjectDir/"bin/Release/net10.0/win-x64/publishNoPdb";
		var ArchivePath = ProjectDir/"bin/Release/net10.0/win-x64/Ngan.Dict.Windows.tar.gz";
		var OldPublishDir = ProjectDir/"publishOld";

		// Match the original Bash flow: enter the project once, then use relative command paths.
		Cd(ProjectDir);
		await Rm(OldPublishDir, Ct);
		if (await Exists(PublishDir, Ct)){
			await Mv(PublishDir, OldPublishDir, Ct);
		}
			
		await Exe("dotnet", [
			"publish", "-c", "Release", "-r", "win-x64",
			"-p:AllowMissingPrunePackageData=true"
		], Ct);
		await Rm(OldPublishDir, Ct);
		await CpAssets.Main(Root, Ct);

		await Rm(PublishNoPdbDir, Ct);
		await Cp(PublishDir/"*", PublishNoPdbDir, Ct);
		await foreach (var Entry in Glob(PublishNoPdbDir/"**/*.pdb", Ct)){
			await Rm(Entry.FullName, Ct);
		}
			
		await Rm(ArchivePath, Ct);
		Cd(PublishNoPdbDir);
		await Exe("tar", ["-czf", ArchivePath, "."], Ct);
		await Mv(ArchivePath, PublishNoPdbDir/"Ngan.Dict.Windows.tar.gz", Ct);
	}
}
