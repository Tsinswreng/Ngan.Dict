namespace Ngaq.Doc.Word.Pronunciation;

using Tsinswreng.CsCore;

[Doc($$"""
#H[模塊概述][
	單詞發音模塊負責:
	- 將單詞詞頭轉為音頻。
	- 在詞典頁與背單詞頁中播放單詞發音。
	- 處理用戶語言到標準語言的映射。
]

#H[業務流程][
	{{nameof(Ngaq.Ui.Views.Word.WordCard.SvcWordCardPronounceBiz)}}
	負責單詞卡片朗讀編排，
	流程為:
	1. 校驗單詞是否存在以及語言字段是否為空。
	2. 查找當前單詞語言對應的用戶語言映射。
	3. 將映射結果轉為標準語言。
	4. 調用 {{nameof(Ngaq.Core.Shared.Dictionary.Svc.ISvcTts)}} 取得音頻。
	5. 調用 {{nameof(Ngaq.Core.Shared.Audio.IAudioPlayer)}} 播放音頻。
]

#H[語言映射約束][
	若單詞語言尚未映射到可識別的標準語言，
	則朗讀流程直接返回
	`UserLangNotMapped`
	狀態，
	不進入音頻下載與播放。

	此約束保證:
	- 界面可明確提示用戶缺少語言映射。
	- 不把語言資料問題延後為播放器或網絡錯誤。
]

#H[音頻生成服務][
	默認文字轉語音服務由
	{{nameof(Ngaq.Backend.Domains.Dictionary.Svc.Gtts)}}
	提供。

	其特點包括:
	- 輸入為文本與標準語言。
	- 輸出為可重讀的音頻對象。
	- 以 `語言碼 + 文本` 作為內存緩存鍵，
	避免重複下載。
]

#H[平臺播放實現][
	播放端經由
	{{nameof(Ngaq.Core.Shared.Audio.IAudioPlayer)}}
	抽象分發到各平臺實作。

	當前約定為:
	- Windows 使用 NAudio，並在部分情況下回退到系統原生播放路徑。
	- Linux 把音頻寫入暫存文件後，調用系統現成命令行播放器。
	- Android 使用系統
	`MediaPlayer`
	播放暫存音頻文件。
]

#H[跨頁面復用][
	單詞發音能力不是詞典頁私有功能。
	在
	{{nameof(Ngaq.Ui.Views.Dictionary.SimpleWord.VmSimpleWord)}}
	中可播放查詞結果的讀音；
	在
	{{nameof(Ngaq.Ui.Views.Word.Learn.VmLearnWords)}}
	中可作為背單詞卡片的自動朗讀能力被調用。
]

#H[信息頁展示規則][
	在
	{{nameof(Ngaq.Ui.Views.Word.WordInfo.ViewWordInfo)}}
	中，
	若
	`KStr = pronunciation`
	且值為 JSON，
	則僅展示其中的
	`Text`
	字段，
	避免把原始結構直接暴露到 UI。
]

""")]
file class _{
	
}
