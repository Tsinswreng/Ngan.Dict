namespace Ngaq.Linux;

using System.Diagnostics;
using Avalonia;
using Avalonia.Markup.Declarative;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ngaq.Client;
using Ngaq.Core;
using Ngaq.Core.Infra.Cfg;
using Ngaq.Core.Infra.Url;
using Ngaq.Backend;
using Ngaq.Backend.Di;
using Ngaq.Backend.Sql;
using Ngaq.Core.Infra.Log;
using Ngaq.Ui;
using Ngaq.Ui.Infra.I18n;
using Tsinswreng.CsCfg;
using Tsinswreng.CsTools;
using MediaManager;
using Tsinswreng.CsI18n;

sealed class Program {

	static str GetCfgFilePath(string[] args) {
		var CfgFilePath = "";
		if (args.Length > 0) {
			CfgFilePath = args[0];
		} else {
#if DEBUG
			CfgFilePath = "Ngaq.jsonc";
#else
			CfgFilePath = "Ngaq.jsonc";
#endif
		}
		return CfgFilePath;
	}

	// Initialization code. Don't use any Avalonia, third-party APIs or any
	// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
	// yet and stuff might break.

	[STAThread]
	public static void Main(string[] args) {
		AppLog.UseConsoleLogger(nameof(Ngaq.Linux), LogLevel.Information);
		if(args.Length > 1 && args[0] == "--version"){
			System.Console.WriteLine(1757779054280);
		}
		BaseDirMgr.Inst._BaseDir = Directory.GetCurrentDirectory();
		try {
/*
改潙勿合併配置字典
緣: 初始化旹各從用戶配置文件與Gui配置文件讀、汶合併。保存旹 併ᵣ後ˌʹ配置對象ˋ被寫入Gui配置文件。
下次初始化旹又各取ⁿ合併、則列表合併旹有褈、益益多矣
當改作 設多源配置、訪問配置項時 優先讀用戶配置、尋不見汶尋Gui配置。
保存旹只寫入Gui配置、緣用戶配置潙只讀。
 */
			System.Console.WriteLine(
				"pwd: " + Directory.GetCurrentDirectory()
			);
			var DualSrcCfg = AppCfg.Inst;
			var CfgPath = GetCfgFilePath(args);

			//用戶手寫的配置文件、不允許 在運行時 由程序 更改
			var RoCfg = new JsonFileCfgAccessor();
			DualSrcCfg.RoCfg = RoCfg;
			RoCfg.FromFile(CfgPath);

			var GuiCfgPath = KeysClientCfg.RwCfgPath.GetFrom(DualSrcCfg) ?? "";
			ToolFile.EnsureFile(GuiCfgPath);
			var GuiCfg = new JsonFileCfgAccessor();
			DualSrcCfg.RwCfg = GuiCfg;
			GuiCfg.FromFile(GuiCfgPath);

			var Lang = KeysClientCfg.Lang.GetFrom(AppCfg.Inst)??"default";

			var I18nCfg = new JsonFileCfgAccessor();
			AppI18n.Inst=new(I18nCfg);
#if DEBUG
			AppI18n.Inst.OnKeyNotFound = (self, key, args)=>{
				var msg = "❗"+key.GetFullPath()+"❗";
				System.Console.WriteLine(msg);
				return msg;
				//throw new Exception();
			};
#endif
//TODO 統一 AppI18n 配置 勿只在Linux專用入口配置
			try{
				I18nCfg.FromFile($"Languages/{Lang}.json");
			}catch{
				System.Console.Error.WriteLine($"Failed to load language file: {Lang}");
			}


		} catch (System.Exception e) {
			System.Console.Error.WriteLine("Failed to load config file: " + e);
		}

		var svc = new ServiceCollection();
		svc.AddSingleton<II18n>(AppI18n.Inst);
		svc
			.SetupCore()
			.SetupLocal()//TODO 改成按需API調用
			.SetupLocalFrontend()
			.SetupClient()
			.SetupUi()
			.SetupLinux()
		;
		var svcProvider = svc.BuildServiceProvider();
		BuildAvaloniaApp()
#if DEBUG
			.UseHotReload()
			.UseRiderHotReload()
#endif
			.AfterSetup(e => {
				App.SetSvcProvider(svcProvider);
				AppIniter.Inst.Sp = svcProvider;
				_ = AppIniter.Inst.Init(default).Result;
			})
			.StartWithClassicDesktopLifetime(args)
		;
	}

	// Avalonia configuration, don't remove; also used by visual designer.
	public static AppBuilder BuildAvaloniaApp(){
		return AppBuilder.Configure<App>()
			.UsePlatformDetect()
			//.WithInterFont()
			.LogToTrace()
			;
	}
}
