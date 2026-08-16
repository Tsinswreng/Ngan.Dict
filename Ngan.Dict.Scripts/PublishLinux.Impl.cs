using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.Sh;

namespace Ngan.Dict.Scripts;

/// Implements the Linux publishing entry point.
internal static partial class PublishLinux{
	internal static async partial Task Main(string Root, CancellationToken Ct) {
		var ProjectDir = Root / "Ngan.Dict/Ngan.Dict.Frontend/proj/Ngan.Dict.Linux";
		var PublishDir = ProjectDir / "bin/Release/net10.0/linux-x64/publish";
		var PublishNoPdbDir = ProjectDir / "bin/Release/net10.0/linux-x64/publishNoPdb";
		var ArchivePath = ProjectDir / "bin/Release/net10.0/linux-x64/Ngan.Dict.Linux.tar.gz";
		var OldPublishDir = ProjectDir / "publishOld";

		// Match the original Bash flow: enter the project once, then use relative command paths.
		Cd(ProjectDir);
		await Rm(OldPublishDir, Ct);
		if (await Exists(PublishDir, Ct))
			await Mv(PublishDir, OldPublishDir, Ct);
		await X("dotnet publish -c Release -r linux-x64", Ct).Out(Ct);
		await Rm(OldPublishDir, Ct);
		await CpAssets.Main(Root, Ct);

		await Rm(PublishNoPdbDir, Ct);
		await Cp(PublishDir / "*", PublishNoPdbDir, Ct);
		await foreach (var Entry in Find(PublishNoPdbDir / "**/*.pdb", Ct))
			await Rm(Entry.FullName, Ct);
		await Rm(ArchivePath, Ct);
		Cd(PublishNoPdbDir);
		await X($"tar -czf \"{ArchivePath}\" .", Ct).Out(Ct);
		await Mv(ArchivePath, PublishNoPdbDir / "Ngan.Dict.Linux.tar.gz", Ct);
	}
}
