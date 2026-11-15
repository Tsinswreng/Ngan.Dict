namespace Ngan.Dict.Linux;

using System.Diagnostics;
using Avalonia;
using Avalonia.Markup.Declarative;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ngan.Dict.Client;
using Ngan.Dict.Core;
using Ngan.Dict.Core.Infra.Cfg;
using Ngan.Dict.Core.Infra.Url;
using Ngan.Dict.Backend;
using Ngan.Dict.Backend.Di;
using Ngan.Dict.Backend.Sql;
using Ngan.Dict.Core.Infra.Log;
using Ngan.Dict.Ui;
using Ngan.Dict.Ui.Infra.I18n;
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
			CfgFilePath = "Ngan.Dict.jsonc";
#else
			CfgFilePath = "Ngan.Dict.jsonc";
#endif
		}
		return CfgFilePath;
	}

	// Initialization code. Don't use any Avalonia, third-party APIs or any
	// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
	// yet and stuff might break.

	[STAThread]
	public static void Main(string[] args) {
		AppLog.UseConsoleLogger(nameof(Ngan.Dict.Linux), LogLevel.Information);
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
			// 配置＝出廠（只讀）＋日常（可寫）兩源疊加；入口不再自己拼路徑、開關檔。
			AppCfg.Inst.Setup(BaseDirMgr.Inst);
			System.Console.WriteLine(
				$"cfg loaded: ro={AppCfg.Inst.RoCfgPath}, rw={AppCfg.Inst.RwCfgPath}"
			);
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
