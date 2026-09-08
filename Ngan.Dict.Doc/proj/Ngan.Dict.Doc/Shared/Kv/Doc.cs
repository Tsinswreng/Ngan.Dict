namespace Ngan.Dict.Doc.Shared.Kv;

using Tsinswreng.CsCore;

[Doc($$"""
#H[ISvcKv 概覽][
	{{nameof(Ngan.Dict.Core.Shared.Kv.Svc.ISvcKv)}} 是鍵值(KV)領域的服務接口，
	實現為 {{nameof(Ngan.Dict.Backend.Domains.Kv.Svc.SvcKv)}}。
	底層數據訪問在 {{nameof(Ngan.Dict.Backend.Domains.Kv.Dao.DaoKv)}}，
	實體為 {{nameof(Ngan.Dict.Core.Shared.Kv.Models.PoKv)}}(Owner + KStr/KI64 複合鍵)。

	接口形狀與 SvcWordV2 同款(批處理範式總綱見 Word/Svc/Doc.cs)：
	每方法都有流式入口(IAsyncEnumerable、對外)與 IList 批核心(同名重載、函數只管一批)兩個形狀。
]

#H[方法一覽][
	- OrdGetByOwnerEtKStr / OrdGetByOwnerEtKI64:純讀、不開事務。
	  流式入口 = SqlFlow.Batches 切流 → 逐批調 IList 批核心(DaoKv.IList 查、位置對齊、查無補 null)。
	- OrdSet(Upsert):寫操作、整操作一個事務(用戶定案:不是「批=事務」)。
	  流式入口 = EnsureTxn 整流 + SqlFlow.BatchesInOnly 逐批調 IList 批核心;
	  IList 批核心 = EnsureTxn(可嵌套)內 RepoKv.OrdUpsert IList 版。
]

#H[實現要點][
	- DaoKv.IList 查:Owner/Key 各 Many 同步綁定 → SqlSplicer.Build 產多語句一命令 → SqlCmdMkr.Get1d 逐結果集回讀。
	- 查詢帶 SqlIsNonDel() 過濾(軟刪行不返回,與流式版舊行為一致)。
	- 舊流式版與教學死碼(BatGetByOwnerEtKI64Wrong/StrWrong/DemoBatchSingleConsume)已刪除。
	- 批量刪除/寫入語義與 SvcWordV2 一致:吃 IList、整批一次事務、出參位置對齊。
]
""")]
file class _{
	
}