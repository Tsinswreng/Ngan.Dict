namespace Ngan.Dict.Android;

using global::Android.App;
using global::Android.Runtime;
using Avalonia;
using Avalonia.Android;
using Microsoft.Extensions.DependencyInjection;
using Ngan.Dict.Backend;
using Ngan.Dict.Backend.Di;
using Ngan.Dict.Client;
using Ngan.Dict.Core;
using Ngan.Dict.Core.Infra.Cfg;
using Ngan.Dict.Core.Infra.Log;
using Ngan.Dict.Core.Infra.Url;
using Ngan.Dict.Ui;
using Ngan.Dict.Ui.Infra.I18n;
using System.IO;
using System.Threading;
using Tsinswreng.CsCfg;
using Tsinswreng.CsI18n;
using Tsinswreng.CsTools;

/// Avalonia 12 Android 應用入口，負責在 Activity 建立前準備配置、I18n 與依賴注入。
[Application]
public sealed partial class Application: AvaloniaAndroidApplication<Ngan.Dict.Ui.App>{
	/// 應用生命週期內共用的初始化取消令牌。
	readonly CancellationTokenSource InitCts = new();

	/// Android 系統建立 Application 時初始化前端所需的全局基礎設施。
	public partial Application(nint JavaReference, JniHandleOwnership Transfer);

	/// 配置 Avalonia AppBuilder，並在 Avalonia 完成設置後執行應用初始化器。
	protected override partial AppBuilder CustomizeAppBuilder(AppBuilder Builder);

	/// 建立配置、I18n 與依賴注入容器。
	partial void InitServices();

	/// 按配置載入 Android 語言資源。
	partial void InitI18n(BaseDirMgr BaseDir, AppCfg DualSrcCfg);

	/// 將 APK Assets 中的文件複製到應用外部文件目錄。
	private static partial bool CopyAssetToDirectory(str AssetFileName, str TargetDir);

	/// 應用終止時取消仍在運行的初始化工作。
	public override partial void OnTerminate();
}
