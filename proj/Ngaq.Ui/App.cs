using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Styling;
using Live.Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ngaq.Core.Frontend.Hotkey;
using Ngaq.Core.Infra.Log;
using Ngaq.Ui.Infra;
using Ngaq.Ui.ViewModels;
using Ngaq.Ui.Views;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;

namespace Ngaq.Ui;


public partial class App :Application
#if DEBUG
	,ILiveView
#endif
{
	public static ILogger? Logger{get;protected set;} = AppLog.Inst;
	public static T GetRSvc<T>()
		where T : class
	{
		Logger ??= AppLog.Inst;
		Logger?.LogInformation("GetRSvc: "+typeof(T));
		return App.SvcProvider.GetRequiredService<T>();
	}
	public static T? GetSvc<T>()
		where T : class
	{
		Logger ??= AppLog.Inst;
		Logger?.LogInformation("GetRSvc: "+typeof(T));
		return App.SvcProvider.GetService<T>();
	}

	public static T DiOrNew<T>()
		where T : class, new()
	{
		var Svc = App.SvcProvider?.GetService<T>();
		return Svc ?? new T();
	}

	public static T DiOrMk<T>()
		where T : class, IMk<T>
	{
		var Svc = App.SvcProvider?.GetService<T>();
		return Svc ?? T.Mk();
	}

	public static IServiceProvider SvcProvider { get; private set; } = null!;
	public static void SetSvcProvider(IServiceProvider SvcProvider){
		App.SvcProvider = SvcProvider;
		Logger = AppLog.Inst;
	}


	/// App 初始化入口。
	/// 保持主要 UI 仍爲純 C#；
	/// 但第三方控件全局主題改由極小的 App.axaml 在編譯期註冊，避免運行時動態載入主題失敗。
	/// App.xaml中的
	/// <StyleInclude Source="avares://Avalonia.Controls.TreeDataGrid/Themes/Fluent.axaml" />
	/// 目前找不到辦法用等價c#表示 故此項目仍需少量xaml

	public override void Initialize() {
		this.RequestedThemeVariant = ThemeVariant.Dark;
		AvaloniaXamlLoader.Load(this);
		var Sty = MkrStyle.MkStyForAnyControl();
		Styles.Add(MkrStyle.NoCornerRadius(Sty));
		// 在 App 初始化时添加资源（如 App.axaml.cs 的构造函数）
		App.Current?.Resources.Add(KeysRsrc.ControlContentThemeFontSize, UiCfg.Inst.BaseFontSize);
		_Style();
		//只在windows下可用。在安卓中debug旹需手動註釋掉此。
		//不能運行旹判斷平臺、緣無此符號則編譯不通
		//增 && WINDOWS亦不效 緣Ngaq.Ui中無斯預處理ˉ符號
		#if DEBUG
			if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)){
				// Avalonia 12 uses DiagnosticsSupport; the former DevToolsExtensions entry point no longer exists.
			}
		#endif
	}


	public override void OnFrameworkInitializationCompleted() {
		GlobalExceptionGuard.Install();

		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
			// Avoid duplicate validations from both Avalonia and the CommunityToolkit.
			// More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
			DisableAvaloniaDataAnnotationValidation();

			#region 全局基字體 2025-05-29T17:14:54.155+08:00_W22-4
			// 创建资源字典并添加资源
			var resources = new ResourceDictionary();
			resources.Add("ControlContentThemeFontSize", 14.0);
			// 确保主资源字典存在
			if (App.Current != null && App.Current.Resources == null){
				App.Current.Resources = new ResourceDictionary();
			}
			// 合并新资源到全局字典
			App.Current?.Resources.MergedDictionaries.Add(resources);
			#endregion #region 全局基字體 2025-05-29T17:14:54.155+08:00_W22-4
			desktop.MainWindow = MkWindow();
			// var liveViewHost = new LiveViewHost(this, Console.WriteLine);
			// liveViewHost.StartWatchingSourceFilesForHotReloading();
			// liveViewHost.Show();

			var RegisterGlobalHotkeys = App.GetSvc<I_RegisterGlobalHotKeys>();
			RegisterGlobalHotkeys?.RegisterGlobalHotKeys();
		} else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform) {
			singleViewPlatform.MainView = MainView.Inst;
		}

 		// var overrideStyle = new Style();
		// overrideStyle.Resources.Add("ButtonBackground", new SolidColorBrush(Colors.Black));
		// // overrideStyle.Resources.Add("ButtonBackgroundPointerOver", new SolidColorBrush(Colors.DarkGray));

		// // 3. 将这个 Style 添加到 Application.Styles 集合中，并确保它在 FluentTheme 之后
		// //    注意：如果 App.Styles 中已有 FluentTheme，直接 Add 即可
		// Application.Current?.Styles.Add(overrideStyle);


		base.OnFrameworkInitializationCompleted();
	}



	private void DisableAvaloniaDataAnnotationValidation() {
		// Avalonia 12 internalized BindingPlugins; validation configuration is no longer changed here.
	}


	public Window MkWindow(){
		var Cfg = UiCfg.Inst;
		return new MainWindow{
			DataContext = new MainViewModel()
			,Title= "ŋaʔ"
			,Width = Cfg.WindowWidth
			,Height = Cfg.WindowHeight
			,Foreground = Brushes.White
		};
	}

	public object CreateView(Window window) {
		return MkWindow();
	}
}
