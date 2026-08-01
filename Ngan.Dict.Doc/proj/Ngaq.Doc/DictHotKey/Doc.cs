namespace Ngaq.Doc.DictHotKey;

using Tsinswreng.CsCore;

[Doc($$"""
#H[模塊概述][
	查詞快捷鍵模塊的目標是:
	- 使用全局快捷鍵觸發查詞。
	- 讀取剪貼板文本作爲查詞輸入。
	- 切換到首頁底欄中的字典標簽，不破壞底欄結構。
]

#H[配置項][
	快捷鍵配置定義在
	- {{nameof(Ngaq.Core.Infra.Cfg.KeysClientCfg.Hotkey)}}
	- {{nameof(Ngaq.Core.Infra.Cfg.KeysClientCfg.Hotkey.DictionaryLookup)}}
	- {{nameof(Ngaq.Core.Infra.Cfg.KeysClientCfg.Hotkey.DictionaryLookup.Modifiers)}}
	- {{nameof(Ngaq.Core.Infra.Cfg.KeysClientCfg.Hotkey.DictionaryLookup.Key)}}
]

#H[職責分層][
	#H[平台註冊層][
		{{nameof(Ngaq.Windows.Domains.Hotkey.WinGlobalHotkeyRegistrar)}} /
		{{nameof(Ngaq.Linux.Domains.Hotkey.LinuxGlobalHotkeyRegistrar)}}
		只負責:
		- 從配置解析快捷鍵。
		- 向 {{nameof(Ngaq.Core.Frontend.Hotkey.IHotkeyListener)}} 註冊熱鍵。
		- 觸發時喚起主窗口。

		不再直接處理:
		- 剪貼板讀取
		- 字典頁導航細節
		- 查詞執行細節
	]

	#H[Android通知入口層][
		{{nameof(Ngaq.Android.MainActivity)}}
		負責:
		- 發佈常駐通知（ongoing notification）。
		- 監聽通知點擊意圖並觸發查詞動作。

		不直接處理:
		- 字典頁導航細節
		- 查詞執行細節
	]

	#H[通用業務層][
		{{nameof(Ngaq.Ui.Views.Dictionary.IHotkeyDictionaryLookupAction)}} /
		{{nameof(Ngaq.Ui.Views.Dictionary.HotkeyDictionaryLookupAction)}}
		負責業務編排:
		- 讀 {{nameof(Ngaq.Core.Frontend.Clipboard.ISvcClipboard)}}。
		- 調用 {{nameof(Ngaq.Ui.Views.Dictionary.ISvcDictionaryHotkeyNavigator.OpenDictionary)}}。
	]

	#H[頁面切換層][
		{{nameof(Ngaq.Ui.Views.Dictionary.ISvcDictionaryHotkeyNavigator)}} /
		{{nameof(Ngaq.Ui.Views.Dictionary.SvcDictionaryHotkeyNavigator)}}
		負責:
		- 回到 {{nameof(Ngaq.Ui.Views.Home.ViewHome)}}。
		- 切換到底欄字典標簽。

		切換行爲由
		{{nameof(Ngaq.Ui.Views.Home.ViewHome.OpenDictionaryTab)}} 與
		{{nameof(Ngaq.Ui.Views.BottomBar.ViewBottomBar.SelectControl)}} 完成。
	]
]

#H[執行流程][
	#H[Windows路徑][
	0. 啓動時由 {{nameof(Ngaq.Windows.Domains.Hotkey.WinGlobalHotkeyRegistrar)}} 讀取配置並註冊熱鍵。
	0. 註冊委託給 {{nameof(Ngaq.Windows.Domains.Hotkey.WinHotkeyListener)}}。
	0. 監聽器使用專用 Win32 消息線程，同時處理註冊工作隊列與 `WM_HOTKEY`，避免 AOT 發佈時因消息循環提前阻塞而卡住啓動。
	1. 用戶按下全局快捷鍵。
	2. {{nameof(Ngaq.Windows.Domains.Hotkey.WinGlobalHotkeyRegistrar)}} 接收事件並喚起主窗口。
	3. 調用 {{nameof(Ngaq.Ui.Views.Dictionary.HotkeyDictionaryLookupAction.Run)}}。
	4. 讀取剪貼板文本。
	5. 調用 {{nameof(Ngaq.Ui.Views.Dictionary.SvcDictionaryHotkeyNavigator.OpenDictionary)}}。
	6. 回到首頁並切到字典標簽。
	7. 文本非空時，觸發 {{nameof(Ngaq.Ui.Views.Dictionary.ViewDictionary.ClickLookupBtn)}} 查詞。
	]

	#H[Linux路徑][
	1. 啓動時由 {{nameof(Ngaq.Linux.Domains.Hotkey.LinuxGlobalHotkeyRegistrar)}} 讀取配置並註冊熱鍵。
	2. 註冊委託給 {{nameof(Ngaq.Linux.Domains.Hotkey.LinuxHotkeyListener)}}。
	3. 監聽器通過 X11 `XGrabKey` 註冊全局熱鍵並輪詢按鍵事件，命中後執行查詞回調。
	]

	#H[Android路徑][
	1. 用戶點擊常駐通知。
	2. {{nameof(Ngaq.Android.MainActivity)}} 收到通知意圖。
	3. 若 Activity 尚未拿到窗口焦點，先掛起查詞請求。
	4. 拿到窗口焦點後，調用 {{nameof(Ngaq.Ui.Views.Dictionary.HotkeyDictionaryLookupAction.Run)}}。
	5. 讀取剪貼板文本。
	6. 調用 {{nameof(Ngaq.Ui.Views.Dictionary.SvcDictionaryHotkeyNavigator.OpenDictionary)}}。
	7. 回到首頁並切到字典標簽。
	8. 文本非空時，觸發 {{nameof(Ngaq.Ui.Views.Dictionary.ViewDictionary.ClickLookupBtn)}} 查詞。
	]
]

#H[當前約束][
	- 快捷鍵查詞會優先保留首頁底欄體驗，不再通過 `ToolView` 額外包裝字典頁。
	- 剪貼板服務保持接口抽象；Windows/Avalonia 由 {{nameof(Ngaq.Ui.SvcClipboard)}} 實現，Android 由 {{nameof(Ngaq.Android.Domains.Clipboard.AndroidSvcClipboard)}} 實現。
	- Android 通知查詞必須等 Activity 拿到窗口焦點後再讀剪貼板，否則新系統版本上可能因系統剪貼板限制拿到空值。
	- Linux 路徑依賴 X11/XWayland；純 Wayland 且無 XWayland 時，全局熱鍵註冊會失敗並返回錯誤。
]

""")]
file class _{
	
}
