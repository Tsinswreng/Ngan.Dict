using Ngaq.Core.Shared.StudyPlan.Models;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightArg;

namespace Ngaq.Ui.Views.Word.WordManage.StudyPlan;
file class DirDoc{
	str Doc =
$$"""
#Sum[
{{nameof(StudyPlan)}}相關編輯頁。

涉及實體:
- {{nameof(JnStudyPlan)}}
- {{nameof(BoStudyPlan)}}
]

#Descr[
頁面編寫規範:

- 先在外層 用TreeDataGrid 做 分頁頁面。 該頁面中有新增條目的按鈕。
- 點擊分頁表格中的某一項的時候即進入到這一項的特定的編輯頁面(ViewXxxEdit)。
- 點擊 新增按鈕 也能進到 編輯頁面 (即新增和修改是用同一個頁面的)。
- TreeDataGrid 列寬統一使用：前面的列 `Auto`，最後一列 `1 Star`。

需要在 ViewXxxEdit對應的Ctx 即 VmXxxEdit中 設一個字段、判斷用戶是通過 點擊新增按鈕 進入的、
還是通過 修改實體進入的。 根據這個字段的不同 可能調用不同的後端接口。比如新增就用Add相關接口、修改就用Upd相關接口。


WeightArg 編輯視圖 要求:
底欄兩個按鈕、右邊是保存、左邊是刪除。

中間 把各個字段顯示出來。
其中{{nameof(PoWeightArg.Text)}} 是文本格式的。Payload。
除了在主界面顯示之外、還要加一個按鈕、點擊該按鈕後進入 PayLoad的 Json編輯頁。


分頁的頁面 兼顧分頁功能和選擇功能。

WeightArg 是依賴 WeightCalculator 的
新增/修改 WeightArg 時需要設定一個 引用的WeightCalculator的名稱。
如果設定的引用的權重算法名稱 不存在 則無意義、現在的做法是直接讓用戶填寫文本的、這樣容易拼錯。
加一個功能、可以讓用戶直接選。

具體就是加一個按鈕、用戶點擊按鈕之後跳轉到 權重算法的分頁頁面、
在分頁頁面中選擇一項。

注意代碼復用！原本已經有 權重算法分頁頁面了，我希望你能把他改成 同時適用這兩種場景的、不要再單獨給我造一個。

StudyPlan同理。當前studyPlan的編輯頁是要自己填引用的Id的。
你也改成按按鈕之後能跳轉到分頁分頁來選擇。

在ViewStudyPlan上加一個頁面 叫 設置當前學習方案 。
進入之後就直接顯示到 StudyPlan的編輯頁。
然後內容是 調後端的 獲取當前學習方案接口。

原本 StudyPlanPage頁面上有一個 Restore 按鈕、
把這個按鈕移動到 設置當前學習方案 的頁面 的菜單裏。

前端涉及引用的部分、目前有兩種情況
一種情況是同時顯示被引用對象的UniqName和Id
一種情況是只顯示Id 不顯示UniqName
用戶操作GUI時本應無需關心Id。改成只顯示UniqName不顯示Id的。

前端可編輯Type的地方 不要顯示Unknown這一選項。
	
PreFilter 載荷GUI編輯頁不要顯示Version

新建PreFilter時 載荷json的默認值應該是這樣
```json
{
	"Version": "1.0.0.0",
	"CoreFilter": [],
	"PropFilter": []
}
```

CoreFilter的Fields
	只能是 PoWord 有 的字段
	(Head, Lang, BizCreatedAt, BizUpdatedAt)這四種
	選擇時 做成 既能下拉框選擇 也能手動輸入補全的。除此之外不接收其他無效值。注意用nameof關鍵字  禁止硬編碼。
	
	編輯每個 FilterItem 單獨一個頁面 不要把所有的 FilterItem 全塞一個頁面裏再用 scrollViewer顯示。
	
	具體 CoreFilter的視圖應該是這樣:
	
	頂上兩個標籤、Fields標籤和FilterItems標籤。
	在 Fields標籤頁裏就在滾動器裏放多個可選可填充的輸入框。
	
	FilterItems 標籤頁裏 先用TreeDataGrid顯示所有的 FilterItems、點擊之後纔 轉到 具體單個 FilterItem 的編輯頁。
	
	FilterItem中 編輯多個Value時默認只用換行符作分隔、不要支持逗號。
	編輯Value的輸出框要做個最大高度、不然我內容行數變多他就一直變高。
	
	PropFilter和CoreFilter類似、
	只是Field的編輯不同:
	PropFilter的 Field 編輯的時候 可選 E:\_code\CsNgaq\Ngaq.Core\Shared\Word\Models\Po\Kv\ConstPropKey.cs 這個下面的條目、也可以自己填別的。
]
""";
}
