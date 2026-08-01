namespace Ngaq.Doc.DictionaryLookup;

using Tsinswreng.CsCore;

[Doc($$"""
#H[模塊概述][
	詞典查詢由
	{{nameof(Ngaq.Backend.Domains.Dictionary.Svc.SvcDictionary)}}（後端）
	與
	{{nameof(Ngaq.Ui.Views.Dictionary.VmDictionary)}}（前端）協作完成。
	其核心目標不是返回自由文本，
	而是生成可被程序直接解析的結構化詞典結果，
	以便後續直接轉為單詞對象並進入詞庫流程。
]

#H[請求與響應契約][
	查詞請求使用
	{{nameof(Ngaq.Core.Shared.Dictionary.Models.IReqLlmDict)}}，
	其核心內容包含查詢詞、
	可選的上下文句子、
	源語言、
	目標語言列表以及若干預留偏好字段。

	其中，
	`Query` 用於描述本次要查的詞與消歧語境；
	`OptLang` 用於描述源語言與目標語言；
	`Preferences` 目前主要作為協議預留字段，
	用於為例句、
	近義詞、
	詞源等附加信息保留擴展位置，
	當前實現尚未實際驅動查詞生成邏輯。

	前端在構造查詞請求時，
	應盡量補齊
	`OptLang.SrcLang`
	與
	`OptLang.TgtLangs`
	中的
	`Code` /
	`NativeName` /
	`EnglishName`。
	其中 `Code` 爲必填，
	後兩者雖不直接作爲數據庫主鍵，
	但可幫助後端構造更穩定、可讀性更高的 LLM prompt，
	避免只把裸語言代碼傳給模型。

	模型響應最終會被解析為
	{{nameof(Ngaq.Core.Shared.Dictionary.Models.IRespLlmDict)}}，
	目前保留三類核心字段：
	規範化後的詞頭、
	讀音列表、
	釋義列表。
	這一結構保持足夠通用，
	便於支撐任意語對查詞。
]

#H[語言配置持久化][
	當前源語言與當前目標語言由
	{{nameof(Ngaq.Backend.Domains.Dictionary.Svc.SvcDictionary)}}
	經由鍵值存儲保存。
	若用戶尚未設置，
	系統會初始化為英文作為源語言、
	中文作為目標語言。

	語言配置在保存前會被規整，
	包括補全所有者、
	清理首尾空白、
	以及在缺省時回退到 BCP 47 類型。
	這使查詞頁在重新打開時可以延續上次的語言選擇，
	避免每次重新配置。
]

#H[流式查詞流程][
	後端調用大模型接口時，
	統一以流式方式讀取 SSE 響應。
	即使調用方本身不顯式要求流式回調，
	後端也會先按流式方式接收模型輸出，
	再在內部拼接為完整文本。

	當讀取到新的內容片段時，
	會經由
	{{nameof(Ngaq.Core.Shared.Dictionary.Models.IReqLlmDictEvt)}}
	上的回調逐段通知前端。
	這使前端可以邊接收邊展示，
	減少用戶等待完整結果後再一次性刷新的停頓感。

	查詞按鈕再次點擊時表示取消當前查詢。
	取消語義要求如下:
	- 若尚未收到任何流式片段，前端回滾到本次查詢前的結果狀態。
	- 若已收到部分流式片段，前端保留當前已展示的釋義文本，不再彈未知錯誤。
	- 後端流式讀取循環必須響應取消令牌，避免模型已開始輸出後出現「再次點擊取消沒有反應」。

	送入大模型的用戶提示詞
	不應直接把
	{{nameof(Ngaq.Core.Shared.Dictionary.Models.IReqLlmDict)}}
	序列化成 JSON 後原樣透傳，
	而應整理為面向查詞任務的自然語言說明，
	至少明確：
	查詢詞、
	可選語境、
	源語言、
	目標語言、
	偏好配置、
	以及輸出必須爲可解析的 YamlMd。

	若某些 OpenAI-compatible provider
	需要額外的頂層請求體字段，
	可通過
	{{nameof(Ngaq.Core.Infra.Cfg.KeysClientCfg.LlmDictionary.ExtraBodyJson)}}
	配置一個 JSON 對象透傳。
	該對象會 merge 到默認請求體頂層，
	同名字段以配置值覆蓋，
	用於例如關閉 provider 默認開啟的 thinking / reasoning 模式。
]

#H[YamlMd 輸出約定與寬容解析][
	模型並不直接返回 JSON，
	而是返回 YamlMd 文本。
	YamlMd 是本系統為查詞輸出自行設計並命名的一種混合格式。
	此格式在頂部放置 yaml 代碼塊，
	再使用 markdown 一級標題加代碼塊承載多行長文本，
	從而減少多行內容在 JSON 或普通 YAML 中的大量轉義與縮進負擔。

	後端解析時使用
	{{nameof(Tsinswreng.CsYamlMd.ExtnYamlMd.TryToYamlLenient)}}。
	除了正常的 YamlMd 之外，
	還會寬容處理兩類常見偏差：
	其一是模型把整段 YamlMd 又額外包進一層 markdown 代碼塊；
	其二是模型在正文末尾追加結論性解釋文本。
	只要核心結構仍可識別，
	系統就盡量完成解析，
	以提高查詞成功率。
]

#H[解析失敗處理約定][
	當後端拋出
	{{nameof(Ngaq.Core.Infra.Errors.KeysErr.Dictionary.LlmResponseParseFailed)}}
	時，前端不再回滾查詢前快照。

	具體要求:
	- 保留流式階段已展示的文本（不清空結果區）。
	- 保留
	{{nameof(Ngaq.Ui.Views.Dictionary.VmDictionary.LastLlmRawOutput)}}
	，供
	{{nameof(Ngaq.Ui.Views.Dictionary.LlmRawOutputEdit.ViewLlmRawOutputEdit)}}
	查看與編輯原始響應後重解析。
]

#H[查詞頁首屏與語言按鈕][
	{{nameof(Ngaq.Ui.Views.Dictionary.ViewDictionary)}} 的源/目標語言按鈕
	顯示格式為 `語言代碼 + UI 語言下的譯名`
	如 `en 英語`、`zh 中文`。

	譯名來源為
	{{nameof(Ngaq.Core.Shared.Word.Svc.ISvcNormLang.OrdGetTranslatedName)}}。

	另:
	- 搜索按鈕位於輸入框左側。
	- 未查詞前，輸入框顯示 placeholder。
	- 未查詞前，結果區顯示灰色用法提示，而不是空白。
	- 收藏按鈕在已有查詞結果時，會把結果轉成 {{nameof(Ngaq.Ui.Views.Word.WordEditV2.ViewWordEditV2)}} 草稿。
	- 若尚未查詞，點收藏按鈕則直接進入自由加詞頁，並預填當前源語言。
]

""")]
file class _{
	
}
