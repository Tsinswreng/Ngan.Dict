namespace Ngaq.Ui.Infra;

using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using Ngaq.Core.Infra.Errors;
using Ngaq.Ui.Infra.I18n;
using Ngaq.Ui.Views;
using Tsinswreng.CsErr;

/// 統一攔截前端未處理異常。
/// 優先處理可恢復的 UI 線程與未觀察 Task 異常，避免直接把整個程序帶崩。
public static class GlobalExceptionGuard{
	static i32 _IsInstalled = 0;

	/// 註冊全局異常事件。多次調用只生效一次。
	public static nil Install(){
		if(Interlocked.Exchange(ref _IsInstalled, 1) == 1){
			return NIL;
		}

		Dispatcher.UIThread.UnhandledException += OnUiThreadUnhandledException;
		TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
		AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
		return NIL;
	}

	/// UI 線程異常可顯式標記已處理，從而避免 Avalonia 直接退出。
	static void OnUiThreadUnhandledException(object? Sender, DispatcherUnhandledExceptionEventArgs E){
		E.Handled = true;
		ReportException(E.Exception, nameof(Dispatcher) + "." + nameof(Dispatcher.UIThread), CanContinue: true);
	}

	/// 後臺 Task 若未被觀察，這裏兜底記錄並提示，同時標記已觀察。
	static void OnUnobservedTaskException(object? Sender, UnobservedTaskExceptionEventArgs E){
		E.SetObserved();
		ReportException(E.Exception, nameof(TaskScheduler) + "." + nameof(TaskScheduler.UnobservedTaskException), CanContinue: true);
	}

	/// AppDomain 級別異常通常已不可恢復；此處主要負責最後的日誌與提示。
	static void OnCurrentDomainUnhandledException(object Sender, UnhandledExceptionEventArgs E){
		var Ex = E.ExceptionObject as Exception ?? new Exception(E.ExceptionObject?.ToString());
		ReportException(Ex, nameof(AppDomain) + "." + nameof(AppDomain.UnhandledException), CanContinue: !E.IsTerminating);
	}

	/// 將異常同時寫入日誌與 UI 提示。若 UI 尚未就緒，至少保留控制檯輸出。
	static nil ReportException(Exception? Ex, str Source, bool CanContinue){
		var SafeEx = Ex ?? new Exception("Unknown exception");
		var Prefix = CanContinue
			? I18nPrefixRecoverable()
			: I18nPrefixFatal();
		var Msg = Prefix + "\n[" + Source + "]\n" + SafeEx;

		try{
			App.Logger?.LogError(SafeEx, "Unhandled exception from {Source}", Source);
		}catch{
			System.Console.Error.WriteLine(Msg);
		}

		try{
			Dispatcher.UIThread.Post(()=>{
				MainView.Inst.ShowDialog(Msg);
			});
		}catch{
			System.Console.Error.WriteLine(Msg);
		}

		return NIL;
	}

	/// 可恢復異常提示前綴。
	static str I18nPrefixRecoverable(){
		return AppI18n.Inst.Get(KeysErr.Common.UnknownErr.ToI18nKey())
			+ "\nThe program intercepted this exception and will try to continue running.";
	}

	/// 不可恢復異常提示前綴。
	static str I18nPrefixFatal(){
		return AppI18n.Inst.Get(KeysErr.Common.UnknownErr.ToI18nKey())
			+ "\nA fatal exception was intercepted and the process may still terminate.";
	}
}
