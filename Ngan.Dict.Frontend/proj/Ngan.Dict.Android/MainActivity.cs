using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Util;
using Avalonia;
using Avalonia.Android;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ngan.Dict.Backend;
using Ngan.Dict.Backend.Di;
using Ngan.Dict.Client;
using Ngan.Dict.Core;
using Ngan.Dict.Core.Infra.Cfg;
using Ngan.Dict.Core.Infra.Log;
using Ngan.Dict.Core.Infra.Url;
using Ngan.Dict.Ui;
using Ngan.Dict.Ui.Infra.I18n;
using Ngan.Dict.Ui.Views;
using Ngan.Dict.Ui.Views.Dictionary;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tsinswreng.CsCfg;
using Tsinswreng.CsI18n;
using Tsinswreng.CsTools;
using K = Ngan.Dict.Ui.Infra.I18n.KeysUiI18nCommon;

namespace Ngan.Dict.Android;

// [Activity] 是 Android 的 Activity 屬性，用來定義這個 Activity 的
// 標籤(Label)、主題(Theme)、圖示(Icon)、啟動模式(LaunchMode)等。
[Activity(
	Label = "Ngan.Dict",
	Theme = "@style/MyTheme.NoActionBar",
	Icon = "@drawable/icon",
	MainLauncher = true, //表示應用程式從這個 Activity 開始執行。
	LaunchMode = LaunchMode.SingleTop,
	ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
// Avalonia 12 的 AppBuilder 由 AvaloniaAndroidApplication<App> 配置，Activity 只負責窗口生命週期。
public partial class MainActivity : AvaloniaMainActivity{
	private const int DictionaryLookupNotificationId = 43001;
	private const int NotificationPermissionRequestCode = 43002;
	private const string DictionaryLookupNotificationChannelId = "ngan.dict_dictionary_lookup";
	private const string DictionaryLookupNotificationAction = "Tsinswreng.Ngan.Dict.action.DICTIONARY_LOOKUP_FROM_CLIPBOARD";
	private const string DictionaryLookupIntentFlagKey = "dictionary_lookup_from_clipboard";

	private readonly CancellationTokenSource _cts = new();
	private bool _hasWindowFocus = false;
	private bool _pendingLookupFromNotificationTap = false;

	// OnCreate 是 Android Activity 的第一個生命週期方法，當 Activity 被建立時呼叫。
	// 參數 Bundle 儲存了先前被銷毀前的狀態。一定要呼叫 base.OnCreate 以確保 Avalonia 正常初始化。
	protected override void OnCreate(Bundle? savedInstanceState){
		Log.Info("Ngan.Dict.Android", "MainActivity.OnCreate");
		CaptureDictionaryLookupIntent(Intent);
		base.OnCreate(savedInstanceState);
		EnsureDictionaryLookupNotification();
		TryConsumePendingDictionaryLookupIntent();
	}

	protected override void OnDestroy(){
		_cts.Cancel();
		_cts.Dispose();
		base.OnDestroy();
	}

	/// 攔截 Android 系統返回鍵，優先交給 Avalonia 內部導航處理。
	/// 只有當前端沒有彈窗且也無上一級頁面可退時，纔回落到系統默認行爲。
	public override void OnBackPressed(){
		if(MainView.Inst.TryHandleBackNavigation()){
			return;
		}
		base.OnBackPressed();
	}

	// OnNewIntent 在 Activity 使用 SingleTop 啟動模式且已存在於回退棧頂時觸發。
	// 透過這個方法可以接收到新的 Intent（例如從點擊通知重新喚起 Activity）。
	protected override void OnNewIntent(Intent? intent){
		base.OnNewIntent(intent);
		CaptureDictionaryLookupIntent(intent);
		TryConsumePendingDictionaryLookupIntent();
	}

	public override void OnWindowFocusChanged(bool hasFocus){
		base.OnWindowFocusChanged(hasFocus);
		_hasWindowFocus = hasFocus;
		AppLog.Inst.LogInformation($"MainActivity.OnWindowFocusChanged hasFocus={hasFocus}");
		if(hasFocus){
			TryConsumePendingDictionaryLookupIntent();
		}
	}

	// OnRequestPermissionsResult 是處理 Android 執行時期權限請求結果的 callback。
	// 這裡用來處理 POST_NOTIFICATIONS 權限（Android 13+ 需要）。
	public override void OnRequestPermissionsResult(
		int requestCode, string[]? permissions, Permission[]? grantResults
	){
		base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		if(requestCode != NotificationPermissionRequestCode){
			return;
		}
		if(grantResults is null || grantResults.Length == 0){
			return;
		}
		if(grantResults[0] != Permission.Granted){
			Log.Warn("Ngan.Dict.Android", "POST_NOTIFICATIONS denied, persistent dictionary notification disabled.");
			return;
		}
		EnsureDictionaryLookupNotification();
	}

	// EnsureDictionaryLookupNotification 建立一則持續顯示的通知，讓使用者點擊後可以查詢剪貼簿內容。
	// 主要 Android 機制：
	// - Android 8.0+ 必須建立 NotificationChannel，並指定重要性等級。
	// - Android 13+ 需要動態請求 POST_NOTIFICATIONS 權限（透過 RequestPermissions）。
	// - 使用 PendingIntent 包裝點擊意圖，以便在通知被點擊時啟動 MainActivity。
	// - Notification.Builder 用來組合通知的標題、內容、小圖示、觸發意圖等。
	private void EnsureDictionaryLookupNotification(){
		if(
			Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu
			&& CheckSelfPermission(Manifest.Permission.PostNotifications) != Permission.Granted
		){
			RequestPermissions([Manifest.Permission.PostNotifications], NotificationPermissionRequestCode);
			return;
		}

		var nm = GetSystemService(NotificationService) as NotificationManager;
		if(nm is null){
			Log.Warn("Ngan.Dict.Android", "NotificationManager unavailable; skip dictionary lookup notification.");
			return;
		}

		if(Build.VERSION.SdkInt >= BuildVersionCodes.O){
			var channel = new NotificationChannel(
				DictionaryLookupNotificationChannelId,
				"Ngan.Dict Dictionary",
				NotificationImportance.Low
			){
				Description = "Tap to lookup clipboard text in Ngan.Dict dictionary."
			};
			nm.CreateNotificationChannel(channel);
		}

		var tapIntent = new Intent(this, typeof(MainActivity));
		tapIntent.SetAction(DictionaryLookupNotificationAction);
		tapIntent.PutExtra(DictionaryLookupIntentFlagKey, true);
		tapIntent.AddFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);

		var pendingIntentFlags = PendingIntentFlags.UpdateCurrent;
		if(Build.VERSION.SdkInt >= BuildVersionCodes.M){
			pendingIntentFlags |= PendingIntentFlags.Immutable;
		}
		var tapPendingIntent = PendingIntent.GetActivity(
			this,
			DictionaryLookupNotificationId,
			tapIntent,
			pendingIntentFlags
		);

		var builder = Build.VERSION.SdkInt >= BuildVersionCodes.O
			? new Notification.Builder(this, DictionaryLookupNotificationChannelId)
			: new Notification.Builder(this);
		builder
			.SetContentTitle(AppI18n.Inst[K.NgaqDictionaryLookup])
			.SetContentText(AppI18n.Inst[K.TapReadClipboardAndAutoLookup])
			.SetSmallIcon(global::Android.Resource.Drawable.IcMenuSearch)
			.SetOngoing(true)
			.SetOnlyAlertOnce(true)
			.SetAutoCancel(false)
			.SetContentIntent(tapPendingIntent);

		nm.Notify(DictionaryLookupNotificationId, builder.Build());
	}

	// CaptureDictionaryLookupIntent 檢查傳入的 Intent 是否來自通知點擊（透過 Action 或 Extra 標記），並設定旗標 _pendingLookupFromNotificationTap。
	private void CaptureDictionaryLookupIntent(Intent? intent){
		if(intent is null){
			return;
		}
		var fromAction = intent.Action == DictionaryLookupNotificationAction;
		var fromExtra = intent.GetBooleanExtra(DictionaryLookupIntentFlagKey, false);
		if(fromAction || fromExtra){
			Log.Info(
				"Ngan.Dict.Android",
				$"Dictionary lookup notification intent captured. action={intent.Action}, fromExtra={fromExtra}, hasWindowFocus={_hasWindowFocus}"
			);
			_pendingLookupFromNotificationTap = true;
		}
	}

	// TryConsumePendingDictionaryLookupIntent 在 Activity 準備好（App.SvcProvider 非 null）且旗標為 true 時，
	// 透過 Dispatcher.UIThread 切換到 UI 執行緒執行實際的查詞動作（IHotkeyDictionaryLookupAction）。
	private void TryConsumePendingDictionaryLookupIntent(){
		if(!_pendingLookupFromNotificationTap){
			return;
		}
		if(App.SvcProvider is null){
			Log.Info("Ngan.Dict.Android", "Dictionary lookup pending: App.SvcProvider is not ready yet.");
			return;
		}
		if(!_hasWindowFocus){
			Log.Info("Ngan.Dict.Android", "Dictionary lookup pending: waiting for window focus before reading clipboard.");
			return;
		}

		_pendingLookupFromNotificationTap = false;
		Dispatcher.UIThread.Post(async ()=>{
			try{
				Log.Info("Ngan.Dict.Android", "Consuming dictionary lookup notification after window focus is ready.");
				var action = App.GetSvc<IHotkeyDictionaryLookupAction>();
				if(action is null){
					Log.Warn("Ngan.Dict.Android", "IHotkeyDictionaryLookupAction unavailable.");
					return;
				}
				await action.Run(_cts.Token);
			}catch(Exception ex){
				Log.Error("Ngan.Dict.Android", "Dictionary lookup from notification failed: " + ex);
			}
		});
	}

}
