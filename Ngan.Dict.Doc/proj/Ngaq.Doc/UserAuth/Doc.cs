namespace Ngaq.Doc.UserAuth;

using Tsinswreng.CsCore;

[Doc($$"""
#H[模塊概述][
	用戶認證模塊負責:
	- 註冊新用戶。
	- 登錄並恢復當前登錄狀態。
	- 登出並清理當前雲端身份。
	- 區分本地用戶身份、
	登錄用戶身份與客戶端身份。
]

#H[本地身份與登錄身份][
	應用啟動時由
	{{nameof(Ngaq.Backend.AppIniter)}}
	恢復並初始化前端用戶上下文。

	其中:
	- 本地用戶身份用於離線資料歸屬。
	- 登錄用戶身份用於關聯雲端帳號。
	- 客戶端身份用於標識具體設備實例。

	若本地尚無當前用戶，
	系統會自動生成新的本地用戶標識並寫入鍵值存儲。

	依賴
	{{nameof(Ngaq.Backend.Di.DiLocalFrontend)}}
	的本地 token storage 回歸測試，
	屬於客戶端專用本地後端測試，
	不應放在公共
	`Ngaq.Backend.Test`
	程序集內，
	以免 Server 測試裝配公共後端測試時誤帶入前端專用依賴。
]

#H[註冊與登錄頁][
	{{nameof(Ngaq.Ui.Views.User.ViewLoginRegister)}}
	將登錄與註冊收斂到同一容器中，
	以標籤頁切換兩種操作。

	視圖模型
	{{nameof(Ngaq.Ui.Views.User.VmLoginRegister)}}
	負責:
	- 註冊前校驗電子郵件格式。
	- 註冊前校驗密碼非空與二次輸入一致。
	- 登錄前校驗帳號與密碼非空。
	- 登錄時補入當前客戶端標識。
]

#H[狀態通知約定][
	登錄成功後，
	{{nameof(Ngaq.Ui.Views.User.VmLoginRegister)}}
	不直接操作導航，
	而是通過
	`OnLoginSucceeded`
	通知視圖層執行頁面跳轉與提示。

	登出成功後，
	{{nameof(Ngaq.Ui.Views.User.Profile.VmUserProfile)}}
	同樣以事件方式通知視圖層刷新界面。

	此約定的目的在於：
	保持視圖模型只關心業務狀態，
	將具體導航與提示交由 View 層決定。
]

#H[個人頁展示約定][
	{{nameof(Ngaq.Ui.Views.User.Profile.VmUserProfile)}}
	根據當前前端用戶上下文決定頁面展示內容。

	- 未登錄時 顯示未登錄狀態。
	- 已登錄時 顯示當前登錄用戶標識。
	- 個人頁提供登錄、
	切換帳號與登出入口。
]

""")]
file class _{
	
}
