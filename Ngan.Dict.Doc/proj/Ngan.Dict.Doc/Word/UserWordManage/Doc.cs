namespace Ngan.Dict.Doc.Word.UserWordManage;

using Tsinswreng.CsCore;

[Doc($$"""
#H[模塊概述][
	用戶詞庫管理頁以
	{{nameof(Ngan.Dict.Ui.Views.Word.WordManage.SearchWords.ViewSearchWords)}}
	為入口，負責搜索既有單詞、打開單詞編輯頁，以及承接手動新增單詞。

	搜索頁當前已切到
	{{nameof(Ngan.Dict.Core.Shared.Word.Svc.ISvcWordV2)}} 的
	{{nameof(Ngan.Dict.Core.Shared.Word.Svc.ISvcWordV2.PageSearch)}}，
	前端直接消費
	{{nameof(Ngan.Dict.Core.Shared.Word.Models.Dto.DtoWordSearchHit)}}，
	不再走舊版 {{nameof(Ngan.Dict.Core.Shared.Word.Svc.ISvcWord)}} 的搜索返回模型。
]

#H[搜索命中信息保留][
	搜索結果列表不只拿完整 {{nameof(Ngan.Dict.Core.Shared.Word.Models.JnWord)}}，
	還保留命中來源：
	- 命中整詞；
	- 命中某條 {{nameof(Ngan.Dict.Core.Shared.Word.Models.Po.Kv.PoWordProp)}}；
	- 命中某條 {{nameof(Ngan.Dict.Core.Shared.Word.Models.Po.Learn.PoWordLearn)}}。

	這樣做的目的是避免前端把“是詞頭命中、還是資產 Id 命中”這類信息抹平，
	便於後續在搜索頁直接做差異展示或按命中類型分流交互。
]

#H[自由加詞入口][
	當前提供兩個直接進入空白單詞編輯頁的入口：
	- {{nameof(Ngan.Dict.Ui.Views.Word.WordManage.SearchWords.ViewSearchWords)}} 頂部工具列採用「左搜索、中輸入框、右自由加詞」布局。
	- {{nameof(Ngan.Dict.Ui.Views.Dictionary.ViewDictionary)}} 在尚未查詞時的收藏按鈕。

	兩者都會打開
	{{nameof(Ngan.Dict.Ui.Views.Word.WordEditV2.ViewWordEditV2)}}，
	並調用
	{{nameof(Ngan.Dict.Ui.Views.Word.WordEditV2.VmWordEditV2.InitFreeAddDraft)}}
	建立空白草稿。
]

#H[空白草稿初始化約定][
	自由加詞建立的草稿要求：
	- 聚合根使用空白詞頭，等待用戶手動輸入。
	- 默認不帶屬性，但會預先補一條 ELearn.Add 學習記錄。
	- 時間字段在前端先初始化為當前時間，避免編輯頁首屏出現無效時間文本。
	- 詞典頁入口會把當前源語言預填到草稿語言字段；搜索頁入口則保持空白。
]

#H[詞典頁保存入口調整][
	{{nameof(Ngan.Dict.Ui.Views.Dictionary.ViewDictionary)}} 的收藏按鈕現改為快速保存入口：
	- 若已有查詞結果，直接把詞典結果合併保存到詞庫，不先進編輯頁。
	- 同一輪查詞結果下，快速保存成功後按鈕會鎖定；重新查詞後才恢復可點。
	- 若需要多次保存或先修改內容，則改走菜單第一項，進入 {{nameof(Ngan.Dict.Ui.Views.Word.WordEditV2.ViewWordEditV2)}} 後再保存。
]

#H[屬性與學習記錄新增交互][
	在
	{{nameof(Ngan.Dict.Ui.Views.Word.WordEditV2.ViewWordEditV2)}}
	中，`WordProp` 與 `WordLearn` 分頁的「新增」按鈕會先插入一條默認行，再立即打開對應單行編輯頁。

	這樣做的目的是讓：
	- 新增與修改復用同一套表單；
	- 用戶不必先回到列表再手動點擊新行；
	- 避免空白默認行只出現在表格里、造成“按了新增但沒有成功”的錯覺。
]

""")]
file class _{
	
}


