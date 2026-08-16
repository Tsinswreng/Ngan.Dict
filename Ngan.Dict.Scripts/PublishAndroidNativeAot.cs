using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.Sh;

namespace Ngan.Dict.Scripts;

/// Publishes a NativeAOT Android APK from WSL or another Linux environment with the Android toolchain installed.
internal static partial class PublishAndroidNativeAot{
	/// Publishes the Android project with its dedicated NativeAOT project switch.
	internal static partial Task Main(string Root, CancellationToken Ct);
}
