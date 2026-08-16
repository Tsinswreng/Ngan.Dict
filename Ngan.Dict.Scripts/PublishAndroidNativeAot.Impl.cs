using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.Sh;

namespace Ngan.Dict.Scripts;

/// Implements the WSL NativeAOT Android publishing entry point.
internal static partial class PublishAndroidNativeAot{
	internal static async partial Task<object?> Main(CancellationToken Ct) {
		var ProjectDir = ScriptContext.FrontendProjects / "Ngan.Dict.Android";
		var Project = ProjectDir / "Ngan.Dict.Android.csproj";
		var Sdk = GetEnv("ANDROID_SDK_DIRECTORY") ?? "/mnt/d/ENV/Android/Sdk";
		var Ndk = GetEnv("ANDROID_NDK_DIRECTORY") ?? "/mnt/d/ENV/_wsl/android-ndk-r27c";
		var Runtime = GetEnv("ANDROID_RUNTIME_IDENTIFIER") ?? "android-arm64";
		var Artifacts = GetEnv("NATIVEAOT_ARTIFACTS_PATH") ?? (GetEnv("HOME") ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)) / "ngan.dict-nativeaot-artifacts";

		// Do not pass PublishAot globally: this dedicated switch is consumed only by Ngan.Dict.Android.csproj.
		var Command =
			$"dotnet publish \"{Project}\" -c Release -r {Runtime} --artifacts-path \"{Artifacts}\" " +
			$"-p:Ngan.DictAndroidNativeAot=true -p:AndroidSdkDirectory=\"{Sdk}\" -p:AndroidNdkDirectory=\"{Ndk}\" " +
			"-p:AllowMissingPrunePackageData=true -p:UseSharedCompilation=false -nodeReuse:false";
		await ScriptContext.XTerm(Command, new(Cwd: ProjectDir), Ct);
		await Echo("NativeAOT publish completed: " + Artifacts, Ct);
		return null;
	}
}
