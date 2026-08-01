namespace Ngaq.Android;

file class DirDoc{
	str Doc =
"""
#Sum[
Android 平臺入口與平臺級交互。
]

#Descr[
`MainActivity` 負責承接 Android 系統事件，再轉交給 Avalonia UI。

目前已接入兩類平臺級入口：
- 持續通知點擊後的剪貼簿查詞
- 系統返回鍵，優先關閉 `MainView` 彈窗，否則回到上一級導航
- 入口按配置項 `Lang` 載入 `Languages/<Lang>.json`，並把 `II18n` 註冊進 DI

音頻播放相關注意：
- 在線 TTS 音頻的下載與釋放不能依賴 Android UI 主線程，否則可能觸發 `NetworkOnMainThreadException`
]
""";
}
