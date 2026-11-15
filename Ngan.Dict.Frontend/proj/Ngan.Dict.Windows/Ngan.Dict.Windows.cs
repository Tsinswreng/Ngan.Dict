namespace Ngan.Dict.Windows;

using System.Diagnostics;
using Avalonia;
using Avalonia.Markup.Declarative;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ngan.Dict.Client;
using Ngan.Dict.Core;
using Ngan.Dict.Core.Infra;
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
			AppLog.Inst.LogInformation("pwd: " + Directory.GetCurrentDirectory());
			// 配置＝出廠（只讀）＋日常（可寫）兩源疊加，位置與檔名全由 AppCfg 決定；
			// 入口不再自己拼路徑、也不再自己開關檔。
			AppCfg.Inst.Setup(BaseDirMgr.Inst);
			AppLog.Inst.LogInformation(
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
