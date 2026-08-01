namespace Ngaq.Doc.Word.StudyPlan;

using Tsinswreng.CsCore;
using Ngaq.Core.Shared.StudyPlan.Models;
using Ngaq.Core.Shared.StudyPlan.Models.Po.PreFilter;
using Ngaq.Core.Shared.StudyPlan.Models.Po.StudyPlan;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightArg;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightCalculator;
using Ngaq.Core.Shared.Word;
using Ngaq.Core.Shared.Word.WeightAlgo;

[Doc($$"""
#Sum[
學習方案{{nameof(PoStudyPlan)}}, {{nameof(BoStudyPlan)}}
是背單詞流程的配置集合，包含組件：
#H[前置篩選器][
- {{nameof(Ngaq.Core.Shared.StudyPlan.Models.PreFilter.PreFilter)}}：
- {{nameof(PoPreFilter)}}
支持按語言、標籤、來源等條件篩選單詞，用戶可定義多個篩選條件及其組合邏輯，精確控制哪些單詞納入當前學習範圍；
應當在Sql層實現 而不是把單詞載入內存後再在內存中過濾篩選
]

#H[權重算法選擇][
{{nameof(PoWeightCalculator)}}
用戶可選擇使用系統內置算法或自定義JavaScript算法；
]

#H[算法參數配置][
{{nameof(PoWeightArg)}}
爲選定的算法設置具體參數，如基礎分值、衰減係數等。
]

通過學習方案管理，用戶可針對不同學習目標創建多套方案，如「日語N1備考」「英語閱讀詞彙」等，滿足個性化學習需求。

]
#H[模塊概述][
	StudyPlan 前端頁面的目標是:
	- 在同一套界面中完成學習方案的選擇、編輯與刪除。
	- 將前置篩選器與權重配置的編輯能力收斂到可視化流程中。
]

#H[主要頁面][
	- {{nameof(Ngaq.Ui.Views.Word.WordManage.StudyPlan.SetCurStudyPlan.ViewSetCurStudyPlan)}}:
		設置當前學習方案，支持從列表選擇或進入編輯；重新載入 / 還原內置操作位於頁面標題菜單。
	- {{nameof(Ngaq.Ui.Views.Word.WordManage.StudyPlan.StudyPlanEdit.ViewStudyPlanEdit)}}:
		學習方案主編輯頁，關聯前置篩選器、權重計算器與權重參數。
	- {{nameof(Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.PreFilterVisualEdit.ViewPreFilterVisualEdit)}}:
		前置篩選器一體化編輯頁，含核心篩選器與屬性篩選器分頁。
	- {{nameof(Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.PreFilterVisualEditV2.ViewPreFilterVisualEditV2)}}:
		前置篩選器 V2 編輯頁，統一為單列表編輯；字段與篩選項各限制單條，舊版入口保留於標題菜單。
	- {{nameof(Ngaq.Ui.Views.Word.WordManage.StudyPlan.WeightCalculatorEdit.ViewWeightCalculatorEdit)}}:
		權重計算器編輯頁，支持負載文本配置。
	- {{nameof(Ngaq.Ui.Views.Word.WordManage.StudyPlan.WeightArgPayloadJsonEdit.ViewWeightArgPayloadJsonEdit)}}:
		權重參數 JSON 配置頁。
]

#H[交互約定][
	- 同行同時存在刪除與保存按鈕時，刪除放在左側。
	- 需要用戶複製的 Id 字段使用可選中文本控件。
	- {{nameof(Ngaq.Ui.Views.Word.WordManage.StudyPlan.SetCurStudyPlan.ViewSetCurStudyPlan)}} 是展示頁，不用輸入框承載當前方案文本；展示字段應使用可選中文本控件。
	- 關聯選擇操作使用帶圖標按鈕，減少歧義。
	- 前置篩選器的字段 / 運算 / 值類型下拉框在界面上顯示本地化文案，但保存時仍使用原始字段鍵與枚舉值。
	- 前置篩選器可視化編輯頁的頂欄菜單提供 JSON 入口，可直接以格式化文本編輯 PoPreFilter.Text 載荷並執行保存 / 刪除。
]

#H[js 權重算法自定義][
	- 權重計算器的 `PayloadText` 用於承載自定義算法配置文本。
	- 權重參數 JSON 與計算器配置分離：
		- 計算器頁負責算法主配置（如策略、腳本內容、版本號）。
		- 參數頁負責可變參數（如閾值、倍率、白名單）並可獨立調整。
	- 建議約定一套穩定鍵名，避免運行期因鍵名漂移造成解析失敗。
]

#Descr[
	#H[學習方案][
		#P[一套學習方案由一種單詞預篩選器、
			一種單詞權重算法與一種權重算法參數組成。
			用戶可設置多套學習方案，
			靈活調節背單詞的策略。]
		#H[單詞預篩選器][
			#P[根據語言/標籤/來源等信息靜態篩選過濾出目標單詞]
			工作原理: 靜態翻譯成sql 查詢條件、在加載單詞旹篩選
			默認未設置篩選器旹 查詢用戶全量單詞 傳入 {{nameof(MgrLearn.LoadEtCalcWeight)}} 再算權重
		]
		#H[單詞權重算法][
			#P[根據單詞的添加次數、
				學習記錄等信息計算單詞權重，
				用戶開始背單詞時依據權重高低排序。
				每套權重算法支持調節不同的參數組合。
				除程序內置的權重算法與參數組合之外，
				支持用戶定義自己的權重算法與參數組合。
			]
		]
	]

	#H[權重算法][
		#P[程序需內置幾套權重算法，
			同時支持用戶自定義權重算法。]

		#P[內置權重算法的核心設計基於事件流模型。
			系統爲每個單詞維護一條按時間排序的事件序列，
			包括添加、記得、忘記三種事件。
			計算權重時，
			算法從最早的事件開始遍歷至當前時間，
			根據事件類型決定權重的增減方向，
			並根據時間間隔計算衰減因子。
			最終權重決定單詞在複習列表中的排序位置，
			權重越高的單詞優先展示。]

		#P[用戶自定義權重算法功能：
			程序內置一套JavaScript引擎(Jint)，
			將單詞信息及相關API暴露給用戶，
			支持用戶自己編寫JavaScript代碼來定義自己的權重算法。]

	#[
2. 权重计算模块
默認權重算法: {{nameof(DfltWeightCalculator)}}
  - 基于添加次数（AddCnt）的加成系数；
  - 基于时间间隔的记忆（Rmb）减权/忘记（Fgt）加权；
  - 末次添加时间的额外加成；
  - 长时间未复习的权重惩罚。
]

	]
]

""")]
file class _{
	
}




