namespace Ngan.Dict.Doc.Word.Svc;

using Tsinswreng.CsCore;

[Doc($$"""
#H[SvcWordV2 概覽][
	{{nameof(Ngan.Dict.Core.Shared.Word.Svc.ISvcWordV2)}} 是單詞領域的服務接口，
	實現為 {{nameof(Ngan.Dict.Backend.Domains.Word.Svc.SvcWordV2)}}。
	底層數據訪問在 {{nameof(Ngan.Dict.Backend.Domains.Word.Dao.DaoWordV2)}}。

	本模塊文檔描述 Svc 層的批處理範式與事務邊界，
	這是理解全部 SvcWordV2 寫方法(新增 / 更新 / 刪除 / 合併 / 同步)的總綱。
]

#H[批處理範式:函數邊界 = 批邊界][
	SvcWordV2 每個寫方法都有兩個重載:

	- 流式入口(IAsyncEnumerable 參數):對外 API。
	  內部先 `SqlCmdMkr.EnsureTxn` 包住整個寫操作(整操作一個事務)，
	  再用 `SqlFlow.Batches / BatchesInOnly` 把源流切成批、逐批調用 IList 批核心。
	- IList 批核心(IList 參數):函數只管一批。
	  內部零業務分批，整批一次完成(一次查、一次寫)，並自帶可嵌套的 EnsureTxn——
	  有外層事務時只加入、無外層時自開自提交，
	  因此批核心可被單獨調用與測試。

	接口上只暴露流式入口與 IList 批核心兩種形狀，
	前端遠程代理(HttpWordV2)的 IList 重載只是轉成流式轉發，不在前端做批處理。
]

#H[事務邊界:整個寫操作一個事務][
	「批 = 事務」是錯誤的。
	正確邊界:整個寫操作(一個入口調用)放在同一個事務裏，
	由流式入口的 EnsureTxn 包住完整 Batches 循環；
	批核心內部的 EnsureTxn 是為了可嵌套而存在，偵測到外層事務時只加入、不另開。

	規則:
	- 同一入口的所有批要麼全部提交、要麼全部回滾。
	- IList 批核心內部調用另一個 Svc 方法時，
	  必須用重組後的 `new DbUserCtx(Ctx.UserCtx, DbCtx)`(localCtx)傳遞，
	  讓內層 EnsureTxn 偵測到外層事務(Txn 非空)而只加入——
	  否則 sqlite 會以「SqliteConnection does not support nested transactions」失敗。
	- EnsureTxn lambda 內的 `DbCtx` 參數才是事務內上下文，
	  `Ctx.DbFnCtx` 在無外層時保持 null、不能用於 Dao 調用(會 NRE)。
	- 純讀方法(如 GetWordMergeResult)不開事務，
	  直接用 `Ctx.DbFnCtx ??= new DbFnCtx();` 惰性建一個裸上下文。
]

#H[批大小唯一出處][
	Svc 層所有 Batches / BatchesInOnly 調用不寫死批大小，
	由 `SqlFlow.DfltBatchSize(Db)/DfltInBatchSize(Db)` 按資料庫類型決定
	(sqlite 寫 1 / 讀 50、pg 500)。

	注意:庫版 SqlFlow 拿不到 DbSrcType 時退到 FallbackBatchSize=500，
	業務側如需精確策略可顯式傳 `SqlFlow.DfltBatchSize(SqlCmdMkr.DbSrcType)`。
]

#H[聚合修改收尾:觸碰 BizUpdatedAt][
	ISvcWordV2 文檔頭約定:凡動過 JnWord(含資產)必須經
	`DaoWordV2.BatAltWordAfterUpd(IList)` 觸碰根之 BizUpdatedAt——
	同步端以 BizUpdatedAt 判新舊，漏觸碰 = 丟改動。

	所有寫路徑(OrdUpdPoWord / OrdChangeId / OrdSync_IdNotEqual / BatSync_RemoteIsNewer
	/ MergeWord / DelWordPropInId / DelWordLearnInId 等)尾部都保留此收尾。
]

#H[同步分桶:五桶派發順序][
	{{nameof(Ngan.Dict.Core.Shared.Word.Svc.ISvcWordV2.OrdSyncByDto)}}
	把整批 Dto 按 DiffResult 歸入五桶、逐桶一個 IList 批核心處理。

	調用順序固定:
	NoChange → RemoteIsOlder → LocalNotExist → IdNotEqual → RemoteIsNewer。
	其中 IdNotEqual 會改主鍵(以 Id 較小者為基準 Id、先遷主鍵再整批寫回)，
	與後續 RemoteIsNewer 的按 Id 更新存在先後依賴，勿隨意調整。
]

#H[批量刪除語義][
	批量刪除一律是「IN 語義」(SoftDelInId / 同構批量)，不是「逐條 Ord 單刪」。
	OrdUpdPoWord 內部衝突分支的軟刪舊詞也是批量
	(`RepoWord.SoftDelInId`、`DaoWordV2.BatMoveAssetsToWordId`、`BatRestoreInId` 全 IList 版)。
]

#H[歷史唯一:單一實現軌跡][
	新範式已徹底取代舊的「接口吃 IAsyncEnumerable、實現內部 BatchCollector 手動分批」寫法:

	- Svc 層所有寫方法只有流式入口 + IList 批核心兩種形狀，無第三種實現軌跡。
	- Dao 層被 Svc 用到的批方法只保留 IList 版(與流式版同文件、同名重載)，
	  舊流式版(BatchCollector 手搓)已刪除。
	- 曾存在的 _2 原型(ISvcWordV2_2 / SvcWordV2.Sync._2 / DaoWordV2._2)已全部刪除。
]
""")]
file class _{
	
}
