-Sesn[
	-T[2026_0804_222504][
		先看skill和記憶
		然後看 大模型詞典
		`E:\_code\Ngan\Ngan.Dict\Ngan.Dict.Frontend\proj\Ngan.Dict.Ui\Views\Dictionary\ViewDictionary.cs`。
		我想添加標準語言的時候 他報錯 我沒權限。
		看看怎麼回事。
		現在還不准改代碼
	][
````md
我查完了，沒有修改任何代碼。

問題不在 `ViewDictionary.cs` 本身，而在標準語言新增時的 Owner 權限校驗鏈：

1. 詞典頁的語言入口只負責打開語言列表  
   [ViewDictionary.Impl.cs:299](E:/_code/Ngan/Ngan.Dict/Ngan.Dict.Frontend/proj/Ngan.Dict.Ui/Views/Dictionary/ViewDictionary.Impl.cs:299)

2. 點「新增」後進入標準語言編輯頁  
   [ViewNormLangPage.cs:121](E:/_code/Ngan/Ngan.Dict/Ngan.Dict.Frontend/proj/Ngan.Dict.Ui/Views/Word/WordManage/NormLang/NormLangPage/ViewNormLangPage.cs:121)

3. 前端新增時把 `Owner` 設為 `dbCtx.UserCtx.UserId`  
   [VmNormLangEdit.cs:126-129](E:/_code/Ngan/Ngan.Dict/Ngan.Dict.Frontend/proj/Ngan.Dict.Ui/Views/Word/WordManage/NormLang/NormLangEdit/VmNormLangEdit.cs:126)

4. 但前端的 `UserId` 實際上是 `LocalUserId`  
   [FrontendUserCtx.cs:10-11](E:/_code/Ngan/Ngan.Dict/Ngan.Dict.Core/proj/Ngan.Dict.Core/Frontend/User/FrontendUserCtx.cs:10)

5. 後端則從登入 Token 取得登入用戶 ID，並要求新增資料的 `Owner` 必須等於該 ID  
   [SvcNormLang.cs:199](E:/_code/Ngan/Ngan.Dict/Ngan.Dict.Backend/proj/Ngan.Dict.Backend/Domains/Word/Svc/SvcNormLang.cs:199)

因此，只要：

- `LocalUserId != LoginUserId`，或
- 尚未登入、Token 對應到零 ID，

就會觸發 `PermissionDenied`。

根本矛盾是：目前「標準語言」資料被實作成 Owner 隔離資料，但前端新增使用的是本地用戶身份，後端權限使用的是登入用戶身份。

另外，詞典頁的快捷語言編輯本身只保存本地配置，不會新增標準語言；真正新增標準語言是在標準語言管理頁點「＋」後發生的。

這裡需要你確認設計意圖：

- 標準語言是否應該是「全局共享資料」？
- 還是每個登入用戶各自擁有一套標準語言？

目前代碼的 `InitBuiltinNormLang` 也使用同一套 Owner 校驗，所以這不是單一按鈕問題，而是標準語言資料權限模型的問題。
````
	]
	
	-T[2026_0804_225850][
		看一下 當前項目的 LocalUser 和 LoginUser的用法、說說有甚麼區別
	][
````md
當前項目裡，兩者設計上是不同身份：

| 身份 | 含義 | 生命週期 | 主要用途 |
|---|---|---|---|
| `LocalUserId` | 本機這個應用實例的本地用戶 | 首次啟動生成，之後持久化 | 離線資料、本地資料歸屬 |
| `LoginUserId` | 當前登入的雲端帳號 | 登入後設定，登出後清零 | 雲端認證、顯示當前帳號 |

`UserId` 現在只是 `LocalUserId` 的舊別名：

[FrontendUserCtx.cs:10-11](E:/_code/Ngan/Ngan.Dict/Ngan.Dict.Core/proj/Ngan.Dict.Core/Frontend/User/FrontendUserCtx.cs:10)

```csharp
UserId => LocalUserId
```

### LocalUserId

應用啟動時從 `CurLocalUserId` 恢復；不存在時自動生成新的 ID：

[AppIniter.cs:67-94](E:/_code/Ngan/Ngan.Dict/Ngan.Dict.Backend/proj/Ngan.Dict.Backend/AppIniter.cs:67)

它不會因為登入或登出而改變，所以切換帳號時仍然保持原本的本地身份。

### LoginUserId

登入成功後由服務器返回並保存：

[ClientUser.cs:69-79](E:/_code/Ngan/Ngan.Dict/Ngan.Dict.Frontend/proj/Ngan.Dict.Client/Svc/ClientUser.cs:69)

同時寫入本地 token storage 的 `CurLoginUserId`。登出時只清除 `LoginUserId` 與 Token：

[ClientUser.cs:101-116](E:/_code/Ngan/Ngan.Dict/Ngan.Dict.Frontend/proj/Ngan.Dict.Client/Svc/ClientUser.cs:101)

個人頁也只用 `LoginUserId` 判斷是否已登入：

[VmUserProfile.cs:50-59](E:/_code/Ngan/Ngan.Dict/Ngan.Dict.Frontend/proj/Ngan.Dict.Ui/Views/User/Profile/VmUserProfile.cs:50)

### 後端實際使用

服務器收到 HTTP 請求後，從 Access Token 解析用戶 ID：

[ExtnServerUserCtx.cs:35-40](E:/_code/Ngan/Ngan.Dict/Ngan.Dict.Server/proj/Ngan.Dict.Server.Http/Infra/ExtnServerUserCtx.cs:35)

所以遠程服務器端的：

```csharp
Ctx.UserCtx.UserId
```

實際代表「Token 對應的登入用戶」，也就是 `LoginUserId`，不是前端本地的 `LocalUserId`。

### 當前存在的核心不一致

前端的 `GetDbUserCtx()` 使用的是前端上下文，而前端的 `UserId` 等於 `LocalUserId`：

[DbUserCtx.cs:35](E:/_code/Ngan/Ngan.Dict/Ngan.Dict.Core/proj/Ngan.Dict.Core/Infra/DbUserCtx.cs:35)

但後端服務統一用登入 Token 的用戶 ID 做 Owner 過濾與權限檢查，例如：

[SvcNormLang.cs:199](E:/_code/Ngan/Ngan.Dict/Ngan.Dict.Backend/proj/Ngan.Dict.Backend/Domains/Word/Svc/SvcNormLang.cs:199)

這就造成：

```text
前端新增資料 Owner = LocalUserId
後端校驗身份 = LoginUserId
```

兩者不同時，就會報「沒有權限」。

所以你之前新增標準語言失敗，正是這個身份模型不一致造成的。

另外，`IFrontendUserCtx` 的註釋仍寫著 `RemoteUserId`，但實際屬性名稱是 `LoginUserId`，這也是一處文檔/命名殘留：

[IFrontendUserCtx.cs:9](E:/_code/Ngan/Ngan.Dict/Ngan.Dict.Core/proj/Ngan.Dict.Core/Frontend/User/IFrontendUserCtx.cs:9)

目前最需要確認的是：詞庫、標準語言、用戶語言等資料的 Owner，究竟應該跟 `LocalUserId`，還是跟 `LoginUserId`。
````
	]
]
