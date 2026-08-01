namespace Ngaq.Doc.Word.LearnFlow;

using Tsinswreng.CsCore;

[Doc($$"""
#H[模塊概述][
	背單詞流程模塊負責:
	- 依據當前學習方案加載待學習單詞。
	- 計算權重並生成學習順序。
	- 接收用戶對單詞的記得 / 忘記標記。
	- 保存學習結果並開始下一輪排序。
	- 提供從首頁底欄與「我的」頁快捷卡片進入背單詞頁的入口。
]

#H[加載流程][
	{{nameof(Ngaq.Ui.Views.Word.Learn.VmLearnWords)}}
	在開始學習時會:
	1. 讀取當前權重參數。
	2. 通過 {{nameof(Ngaq.Core.Shared.Word.Svc.ISvcWordV2.GetWordsToLearn)}} 取得待學習單詞集合。
	3. 將單詞集合交給 {{nameof(Ngaq.Core.Shared.Word.MgrLearn)}} 計算權重並啓動一輪學習。
]

#H[卡片交互約定][
	單詞卡片點擊狀態按如下順序循環:
	- 初次點擊: 標記為記得。
	- 再次點擊: 切換為忘記。
	- 第三次點擊: 清除當前未保存標記。

	界面以不同顏色表示不同學習狀態，
	並同步刷新當前單詞信息區。

	單詞信息區中的每一條
	{{nameof(Ngaq.Core.Shared.Word.Models.Po.Kv.PoWordProp)}}
	右上角都提供編輯圖標。
	點擊後會直接跳轉到
	{{nameof(Ngaq.Ui.Views.Word.WordEditV2.ViewWordEditV2)}}
	並打開對應的既有屬性編輯框。
	description 也按 WordProp 對待，
	不再只是純文本展示。
]

#H[保存與重算][
	當用戶執行保存並重新開始時，
	{{nameof(Ngaq.Core.Shared.Word.MgrLearn)}}
	會先落庫本輪學習記錄，
	再重新計算權重並啓動下一輪學習。

	這意味着:
	- 本輪學習結果會直接影響下一輪排序。
	- 背單詞不是靜態列表操作，
	而是事件流持續反饋的過程。
]

#H[附加能力][
	若配置
	{{nameof(Ngaq.Core.Infra.Cfg.KeysClientCfg.Word.EnableAutoPronounce)}}
	為真，
	則卡片點擊後還會觸發單詞自動朗讀。
]

""")]
file class _{
	
}
