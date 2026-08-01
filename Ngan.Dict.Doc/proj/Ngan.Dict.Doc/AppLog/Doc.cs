namespace Ngan.Dict.Doc.AppLog;

using Tsinswreng.CsCore;

[Doc($$"""
#H[模塊概述][
	全局日誌入口的目標是把以下兩條路徑收斂到同一個源頭：
	- 構造函數中注入的 `ILogger`
	- 不方便走依賴注入時使用的全局靜態入口

	當前這個全局源頭是
	{{nameof(Ngan.Dict.Core.Infra.Log.AppLog)}}。
]

#H[核心約束][
	- {{nameof(Ngan.Dict.Core.Infra.Log.AppLog)}} 是唯一源頭。
	- 各入口程序集只負責在啟動時初始化
	  {{nameof(Tsinswreng.CsLog.DelegatingLogger.InnerLogger)}}。
	- DI 中註冊給業務代碼使用的 `ILogger` 必須指向
	  {{nameof(Ngan.Dict.Core.Infra.Log.AppLog)}} 的單例實例。
	- {{nameof(Ngan.Dict.Ui.App.Logger)}} 也必須指向同一個單例實例，
	  不能再單獨持有另一個 logger。
]

#H[當前實現分層][
	#H[全局入口層][
		{{nameof(Ngan.Dict.Core.Infra.Log.AppLog)}} 繼承
		{{nameof(Tsinswreng.CsLog.DelegatingLogger)}}，
		負責持有全局唯一實例與切換底層實際 logger。
	]

	#H[UI 靜態訪問層][
		{{nameof(Ngan.Dict.Ui.App)}} 的
		{{nameof(Ngan.Dict.Ui.App.Logger)}}、
		{{nameof(Ngan.Dict.Ui.App.GetRSvc)}}、
		{{nameof(Ngan.Dict.Ui.App.GetSvc)}}
		都以 {{nameof(Ngan.Dict.Core.Infra.Log.AppLog)}} 爲共同出口。
	]

	#H[DI 註冊層][
		前後端 DI 中對 `ILogger` 的註冊，
		都應返回 {{nameof(Ngan.Dict.Core.Infra.Log.AppLog)}} 的單例，
		而不是直接返回平台 logger 實例。
	]

	#H[入口初始化層][
		各入口程序集在啓動時調用
		{{nameof(Ngan.Dict.Core.Infra.Log.AppLog.UseLogger)}}
		或
		{{nameof(Ngan.Dict.Core.Infra.Log.AppLog.UseConsoleLogger)}}，
		把平台實際 logger 接到全局源頭上。
	]
]

#H[入口初始化規則][
	- Windows / Linux / Server 這類沒有專用平台 logger 的入口，
	  可先接 console logger。
	- Android / Browser 這類已有平台 logger 的入口，
	  應把既有平台 logger 接到 {{nameof(Ngan.Dict.Core.Infra.Log.AppLog)}}。
	- 後續如果新增入口程序集，
	  必須同步完成 `AppLog` 初始化與 `ILogger` 註冊，
	  否則日誌出口會分叉。
	- Windows 入口目前使用 Serilog 在啓動早期構建
	  `Microsoft.Extensions.Logging.ILoggerFactory`，
	  再通過 {{nameof(Ngan.Dict.Core.Infra.Log.AppLog.UseLoggerFactory)}}
	  接到全局源頭。
	- Windows 入口的文件日誌位於 `exe` 同目錄下的 `logs/`，
	  並同時啓用按日滾動、單文件體積上限、保留文件數上限，
	  以避免日誌無限增長。
	- Windows 入口把 console sink 與 file sink 都包在 async sink 中，
	  目的是儘量減少日誌 IO 對啓動主線程的阻塞。
]

#H[修改約束][
	後續若調整日誌方案：
	- 不要直接修改業務代碼中的現有調用方式來繞過全局入口。
	- 優先保持 `ILogger` 注入方式與 {{nameof(Ngan.Dict.Ui.App.Logger)}} 的兼容性。
	- 若入口初始化方式改動，需同步更新本篇文檔。
]

""")]
file class _{
	
}
