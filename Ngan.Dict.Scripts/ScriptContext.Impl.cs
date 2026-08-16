using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.Sh;

namespace Ngan.Dict.Scripts;

/// Implements shared command forwarding and tree-copy helpers for Ngan.Dict scripts.
internal static partial class ScriptContext{
	internal static async partial Task<CommandExit> XTerm(string Text, CommandOptions? Options, CancellationToken Ct) {
		await using var Command = Options is null ? X(Text, Ct) : X(Text, Options, Ct);
		await Task.WhenAll(
			Write(Stdout, Command.Result.Stdout, Ct),
			Write(Stderr, Command.Result.Stderr, Ct),
			Command.Done);
		return await Command.Done;
	}

	internal static async partial Task<object?> CopyContents(string Source, string Destination, CancellationToken Ct) {
		await Mkdir(Destination, Ct);
		await foreach (var Entry in Find(Source / "**/*", Ct)) {
			var Relative = Path.GetRelativePath(Source, Entry.FullName).Replace('\\', '/');
			var Target = Destination / Relative;
			if (Entry is DirectoryInfo) {
				await Mkdir(Target, Ct);
				continue;
			}
			await using var Content = await Read(Entry.FullName, Ct);
			await Write(Target, Content, Ct);
		}
		return null;
	}
}
