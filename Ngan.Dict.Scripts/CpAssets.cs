namespace Ngan.Dict.Scripts;

/// Synchronizes external runtime resources into each Ngan.Dict output or Android asset location.
internal static partial class CpAssets{
	/// Copies public and private resources to the platform-specific destinations used by the original shell script.
	internal static partial Task Main(string Root, CancellationToken Ct);
}
