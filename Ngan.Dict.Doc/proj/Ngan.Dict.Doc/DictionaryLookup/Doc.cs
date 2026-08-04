namespace Ngan.Dict.Doc.DictionaryLookup;

using Tsinswreng.CsCore;

[Doc($$"""
#H[模塊概述][
	詞典查詢由
	{{nameof(Ngan.Dict.Backend.Domains.Dictionary.Svc.SvcDictionary)}}（後端）
	與
	{{nameof(Ngan.Dict.Ui.Views.Dictionary.VmDictionary)}}（前端）協作完成。
	其核心目標不是返回自由文本，
	而是生成可被程序直接解析的結構化詞典結果，
	以便後續直接轉為單詞對象並進入詞庫流程。
]

#H[請求與響應契約][
	查詞請求使用
	{{nameof(Ngan.Dict.Core.Shared.Dictionary.Models.IReqLlmDict)}}，
	其核心內容包含查詢詞、
	可選的上下文句子、
	源語言、
	目標語言列表以及若干預留偏好字段。

	其中，
	`Query.InputText` 用於描述本次輸入的源語言文本，可為詞或完整句子；
	模型依輸入語義自行決定生成詞典釋義或完整翻譯，翻譯文本仍放入既有的 `Descrs`；
	`OptLang.SrcLang` 是輸入語言的權威設定，模型不得依字形或拼寫改猜其他語言；
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
	{{nameof(Ngan.Dict.Core.Shared.Dictionary.Models.IRespLlmDict)}}，
	目前保留四類核心字段：
	規範化後的詞頭、
	模型最終採用的輸入語言判定、
	讀音列表、
	釋義列表。
	其中
	`DetectedInputLang`
	是響應專用對象，僅含
	`Code` / `NativeName`；
	它表明模型用哪種來源語言生成本次結果，
	不參與既有的詞典結果到單詞對象映射或保存流程。
	這一結構保持足夠通用，
	便於支撐任意語對查詞。
]

#H[語言配置持久化][
	當前源語言與當前目標語言由
	{{nameof(Ngan.Dict.Backend.Domains.Dictionary.Svc.SvcDictionary)}}
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
	{{nameof(Ngan.Dict.Core.Shared.Dictionary.Models.IReqLlmDictEvt)}}
	上的回調逐段通知前端。
	這使前端可以邊接收邊展示，
	減少用戶等待完整結果後再一次性刷新的停頓感。
	但前端把流式片段送回 UI 線程時，
	不得在單次 UI 回調內持續清空整個 backlog；
	應按批次分幀刷新，
	主動給 hover / click 等輸入事件留出插隊機會，
	避免查詞文本流量一大就把上方工具按鈕「卡成看似不可點」。

	查詞按鈕再次點擊時表示取消當前查詢。
	取消語義要求如下:
	- 若尚未收到任何流式片段，前端回滾到本次查詢前的結果狀態。
	- 若已收到部分流式片段，前端保留當前已展示的釋義文本，不再彈未知錯誤。
	- 後端流式讀取循環必須響應取消令牌，避免模型已開始輸出後出現「再次點擊取消沒有反應」。
	送入大模型的用戶提示詞
	不應直接把
	{{nameof(Ngan.Dict.Core.Shared.Dictionary.Models.IReqLlmDict)}}
	序列化成 JSON 後原樣透傳，
	而應整理為面向查詞任務的自然語言說明，
	至少明確：
	輸入文本、
	可選語境、
	源語言、
	目標語言、
	偏好配置、
	以及輸出必須爲可解析的 YamlMd。

	若某些 OpenAI-compatible provider
	需要額外的頂層請求體字段，
	可通過
	{{nameof(Ngan.Dict.Core.Infra.Cfg.KeysClientCfg.LlmDictionary.ExtraBodyJson)}}
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
	{{nameof(Ngan.Dict.Core.Infra.Errors.KeysErr.Dictionary.LlmResponseParseFailed)}}
	時，前端不再回滾查詢前快照。

	具體要求:
	- 保留流式階段已展示的文本（不清空結果區）。
	- 保留
	{{nameof(Ngan.Dict.Ui.Views.Dictionary.VmDictionary.LastLlmRawOutput)}}
	，供
	{{nameof(Ngan.Dict.Ui.Views.Dictionary.LlmRawOutputEdit.ViewLlmRawOutputEdit)}}
	查看與編輯原始響應後重解析。
]

#H[查詞頁首屏與語言按鈕][
	{{nameof(Ngan.Dict.Ui.Views.Dictionary.ViewDictionary)}} 的源/目標語言按鈕
	顯示格式為 `語言代碼 + UI 語言下的譯名`
	如 `en 英語`、`zh 中文`。

	譯名來源為
	{{nameof(Ngan.Dict.Core.Shared.Word.Svc.ISvcNormLang.OrdGetTranslatedName)}}。

	另:
	- 搜索按鈕位於輸入框左側。
	- 未查詞前，輸入框顯示 placeholder。
	- 未查詞前，結果區顯示灰色用法提示，而不是空白。
	- 收藏按鈕在已有查詞結果時，會直接快速保存到詞庫；成功後當前查詞結果下按鈕變綠且不再重複保存。
	- 當用戶重新查詞後，快速保存按鈕會重置為可點狀態。
	- 若希望先進 {{nameof(Ngan.Dict.Ui.Views.Word.WordEditV2.ViewWordEditV2)}} 再手動編輯保存，需改走菜單第一項入口。
	- 若尚未查詞，點收藏按鈕則直接進入自由加詞頁，並預填當前源語言。
]

#H[查詞釋義複製][
	{{nameof(Ngan.Dict.Ui.Views.Dictionary.SimpleWord.ViewSimpleWord)}}
	在釋義下方提供複製按鈕。
	點擊後由
	{{nameof(Ngan.Dict.Ui.Views.Dictionary.SimpleWord.VmSimpleWord)}}
	將已按換行合併的全部 `Descr` 寫入系統剪貼板；
	詞頭與讀音不納入複製文本。

	剪貼板通過
	{{nameof(Ngan.Dict.Core.Frontend.Clipboard.ISvcClipboard)}}
	抽象，由
	{{nameof(Ngan.Dict.Ui.SvcClipboard)}}
	從當前 Avalonia TopLevel 取得剪貼板服務，桌面與 Android 共用同一套適配邏輯。
]

#H[源語言快捷標籤欄][
	{{nameof(Ngan.Dict.Ui.Views.Dictionary.ViewDictionary)}}
	在原有源語言 / 目標語言選擇列上方提供可橫向捲動的源語言快捷標籤欄。

	快捷標籤規則如下：
	- 點擊標籤只切換源語言，目標語言保持不變。
	- 當前源語言標籤使用主色突出顯示。
	- 標籤按內容所需最小寬度顯示，不設數量上限。
	- 自定義短文字為空時回退顯示語言代碼。
	- 沒有配置時快捷欄為空，不自動生成預設標籤；右側編輯按鈕仍然可用。

	快捷標籤配置保存在本地配置文件的
	{{nameof(Ngan.Dict.Core.Infra.Cfg.KeysClientCfg.Dictionary.SrcLangTags)}}，
	每項使用
	{{nameof(Ngan.Dict.Core.Shared.Dictionary.Models.CfgDictionarySrcLangTag)}}
	保存語言標識類型、語言代碼與可選短文字；陣列順序即顯示順序。

	編輯頁允許從標準語言列表添加項目、刪除項目、編輯短文字，
	並以向上 / 向下按鈕調整順序；重複的語言身份不會再次加入。
]

""")]
file class _{
	
}
