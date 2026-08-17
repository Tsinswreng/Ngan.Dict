namespace Ngan.Dict.Scripts;

/// Publishes and packages the Ngan.Dict Windows application.
internal static partial class PublishWin{
	/// Runs the Windows publish, resource synchronization and tarball packaging flow.
	internal static partial Task Main(string Root, CancellationToken Ct);
}
