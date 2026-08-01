namespace Ngan.Dict.Android;

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
using Tsinswreng.CsCfg;
using Tsinswreng.CsI18n;
using Tsinswreng.CsTools;

/// Avalonia 12 Android 應用入口實現。
public sealed partial class Application{
	/// 在 Avalonia 建立 Activity 前完成原 MainActivity.Init 的工作。
	public partial Application(nint JavaReference, JniHandleOwnership Transfer)
		:base(JavaReference, Transfer){
		InitServices();
	}

	/// 保留原 WithInterFont 與 AppIniter 初始化流程。
	protected override partial AppBuilder CustomizeAppBuilder(AppBuilder Builder){
		return base.CustomizeAppBuilder(Builder)
			.WithInterFont()
			.AfterSetup(async _=>{
				AppIniter.Inst.Sp = App.SvcProvider;
				await AppIniter.Inst.Init(InitCts.Token);
			});
	}

	/// 初始化 Android 日誌、文件配置、I18n 與平台服務。
	partial void InitServices(){
		AppLog.UseLogger(new AndroidLogger("Tsinswreng.Ngan.Dict"));
		var appContext = global::Android.App.Application.Context;
		var externalFileDirObj = appContext.GetExternalFilesDir(null)
			?? throw new Exception("Cannot get ExternalFilesDir");
		if(appContext.Assets is null){
			throw new Exception("Cannot get Assets");
		}
		if(appContext.FilesDir is null){
			throw new Exception("Cannot get FilesDir");
		}

		var externalFileDir = externalFileDirObj.AbsolutePath;
		BaseDirMgr.Inst._BaseDir = externalFileDir;
		var baseDir = BaseDirMgr.Inst;
		var roCfgPath = baseDir.Combine("Ngan.Dict.jsonc");
		var dfltRwCfgPath = baseDir.Combine("Ngan.Dict.Rw.jsonc");
		if(!File.Exists(roCfgPath)){
			CopyAssetToDirectory("Ngan.Dict.jsonc", externalFileDir);
			ToolFile.EnsureFile(roCfgPath);
			File.WriteAllText(roCfgPath, "{}");
		}

		var dualSrcCfg = AppCfg.Inst;
		var roCfg = new JsonFileCfgAccessor();
		dualSrcCfg.RoCfg = roCfg;
		roCfg.FromFile(roCfgPath);

		var rwCfgPath = KeysClientCfg.RwCfgPath.GetFrom(dualSrcCfg) ?? dfltRwCfgPath;
		rwCfgPath = baseDir.Combine(rwCfgPath);
		ToolFile.EnsureFile(rwCfgPath);
		var rwCfg = new JsonFileCfgAccessor();
		dualSrcCfg.RwCfg = rwCfg;
		rwCfg.FromFile(rwCfgPath);

		InitI18n(baseDir, dualSrcCfg);
		var services = new ServiceCollection();
		services.AddSingleton<II18n>(AppI18n.Inst);
		services
			.SetupCore()
			.SetupLocal()
			.SetupLocalFrontend()
			.SetupClient()
			.SetupUi()
			.SetupAndroid();
		var serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions{
			ValidateOnBuild = false,
			ValidateScopes = false,
		});
		App.SetSvcProvider(serviceProvider);
	}

	/// 載入配置指定的語言文件，缺失時先從 APK Assets 複製。
	partial void InitI18n(BaseDirMgr BaseDir, AppCfg DualSrcCfg){
		var lang = KeysClientCfg.Lang.GetFrom(DualSrcCfg) ?? "default";
		var i18nCfg = new JsonFileCfgAccessor();
		AppI18n.Inst = new(i18nCfg);
#if DEBUG
		AppI18n.Inst.OnKeyNotFound = (Self, Key, Args)=>{
			var msg = "!" + Key.GetFullPath() + "!";
			System.Console.WriteLine(msg);
			return msg;
		};
#endif
		var langDir = BaseDir.Combine("Languages");
		Directory.CreateDirectory(langDir);
		var langRelativeAssetPath = Path.Combine("Languages", lang + ".json");
		var langFilePath = BaseDir.Combine(langRelativeAssetPath);
		if(!File.Exists(langFilePath)){
			CopyAssetToDirectory(langRelativeAssetPath, langDir);
		}
		try{
			i18nCfg.FromFile(langFilePath);
		}catch{
			System.Console.Error.WriteLine($"Failed to load language file: {lang}");
		}
	}

	/// 複製單一 Asset；失敗時記錄錯誤並讓原啓動流程繼續處理缺失文件。
	private static partial bool CopyAssetToDirectory(str AssetFileName, str TargetDir){
		try{
			var context = global::Android.App.Application.Context;
			if(!Directory.Exists(TargetDir)){
				Directory.CreateDirectory(TargetDir);
			}
			var targetFilePath = Path.Combine(TargetDir, Path.GetFileName(AssetFileName));
			if(File.Exists(targetFilePath)){
				File.Delete(targetFilePath);
			}
			using var assetStream = context.Assets.Open(AssetFileName);
			using var targetStream = new FileStream(targetFilePath, FileMode.CreateNew);
			assetStream.CopyTo(targetStream);
			return true;
		}catch(Exception Err){
			Console.WriteLine($"Copy asset failed: {Err.Message}");
			return false;
		}
	}

	/// Android 通常不調用 OnTerminate，但保留正確的取消與釋放語義。
	public override partial void OnTerminate(){
		InitCts.Cancel();
		InitCts.Dispose();
		base.OnTerminate();
	}
}
