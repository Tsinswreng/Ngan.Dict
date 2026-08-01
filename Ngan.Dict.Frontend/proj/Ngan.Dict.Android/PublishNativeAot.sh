#!/usr/bin/env bash
set -euo pipefail

# 日常構建保持 Mono；此腳本在 WSL 中使用新版 Linux NDK 顯式發佈 NativeAOT APK。
ProjectDir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
AndroidSdkDirectory="${ANDROID_SDK_DIRECTORY:-/mnt/d/ENV/Android/Sdk}"
AndroidNdkDirectory="${ANDROID_NDK_DIRECTORY:-/mnt/d/ENV/_wsl/android-ndk-r27c}"
RuntimeIdentifier="${ANDROID_RUNTIME_IDENTIFIER:-android-arm64}"
ArtifactsPath="${NATIVEAOT_ARTIFACTS_PATH:-$HOME/ngan.dict-nativeaot-artifacts}"

dotnet publish "$ProjectDir/Ngan.Dict.Android.csproj" \
	-c Release \
	-r "$RuntimeIdentifier" \
	--artifacts-path "$ArtifactsPath" \
	-p:Ngan.DictAndroidNativeAot=true \
	-p:AndroidSdkDirectory="$AndroidSdkDirectory" \
	-p:AndroidNdkDirectory="$AndroidNdkDirectory" \
	-p:AllowMissingPrunePackageData=true \
	-p:UseSharedCompilation=false \
	-nodeReuse:false

echo "NativeAOT publish completed: $ArtifactsPath"
