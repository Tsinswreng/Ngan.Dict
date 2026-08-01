namespace Ngan.Dict.Windows;

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
using Serilog;
using Tsinswreng.CsCfg;
using Tsinswreng.CsTools;
using MediaManager;
using Serilog.Extensions.Logging;
using Tsinswreng.CsI18n;

sealed class Program {
	/// Windows 入口日誌文件夾名稱。
	/// 放在 exe 同目錄下，便於直接隨程序一起排障。
	private const str LogDirName = "logs";

	/// 日誌文件前綴。Serilog 會在 rolling file 模式下自動補日期後綴。
	private const str LogFileName = "ngan.dict-.log";

	/// 單個日誌文件大小上限: 10 MiB。
	/// 超限後會自動切分，避免單文件無限膨脹。
	private const i32 LogFileSizeLimitBytes = 10 * 1024 * 1024;

	/// 僅保留最近 14 份日誌文件，避免總體積無限增長。
	private const i32 RetainedFileCountLimit = 14;

	/// 根據命令行參數決定配置文件路徑。
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

	/// 初始化 Windows 入口的全局日誌源頭。
	/// 優先使用 Serilog 同時輸出到 console 與 rolling file；
	/// 若文件 sink 初始化失敗，則回退到僅 console，避免啓動流程直接中斷。
	static void InitAppLog() {
		try {
			var LoggerFactory = BuildWindowsLoggerFactory();
			AppLog.UseLoggerFactory(LoggerFactory, nameof(Ngan.Dict.Windows));
		} catch (Exception Err) {
			AppLog.UseConsoleLogger(nameof(Ngan.Dict.Windows), LogLevel.Information);
			AppLog.Inst.LogError(Err, "Failed to initialize Windows file logger. Falling back to console only.");
		}
	}

	/// 構建 Windows 專用 logger factory。
	/// 這裏把兩個 sink 都包進 async sink，盡量減少日誌 IO 對主線程的阻塞。
	static ILoggerFactory BuildWindowsLoggerFactory() {
		var LogDirPath = Path.Combine(AppContext.BaseDirectory, LogDirName);
		var LogFilePath = Path.Combine(LogDirPath, LogFileName);
		Directory.CreateDirectory(LogDirPath);

		var SerilogLogger = new global::Serilog.LoggerConfiguration()
			.MinimumLevel.Information()
			.Enrich.FromLogContext()
			.WriteTo.Async(Cfg => Cfg.Console())
			.WriteTo.Async(Cfg => Cfg.File(
				path: LogFilePath,
				rollingInterval: global::Serilog.RollingInterval.Day,
				retainedFileCountLimit: RetainedFileCountLimit,
				fileSizeLimitBytes: LogFileSizeLimitBytes,
				rollOnFileSizeLimit: true,
				shared: true
			))
			.CreateLogger();

		return new SerilogLoggerFactory(SerilogLogger, dispose: true);
	}

	// Initialization code. Don't use any Avalonia, third-party APIs or any
	// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
	// yet and stuff might break.

	[STAThread]
	public static void Main(string[] args) {
		// 先把全局日誌源頭接好，後面的配置讀取與 DI 啓動都可以直接復用同一個 logger。
		InitAppLog();
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
			AppLog.Inst.LogInformation("pwd: " + Directory.GetCurrentDirectory());
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
//TODO 統一 AppI18n 配置 勿只在windows專用入口配置
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
			.SetupWindows()
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
