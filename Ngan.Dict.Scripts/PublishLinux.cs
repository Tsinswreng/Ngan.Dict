namespace Ngan.Dict.Scripts;

/// Publishes and packages the Ngan.Dict Linux application.
internal static partial class PublishLinux{
	/// Runs the Linux publish, resource synchronization and tarball packaging flow.
	internal static partial Task<object?> Main(CancellationToken Ct);
}
