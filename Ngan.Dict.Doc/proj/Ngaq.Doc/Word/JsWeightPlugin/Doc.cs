namespace Ngaq.Doc.Word.JsWeightPlugin;

using Tsinswreng.CsCore;

[Doc($$"""
#H[自定義 js 權重算法插件格式][
	本文面向插件開發者，說明如何編寫可被
	{{nameof(Ngaq.Core.Shared.Word.Svc.JsWeightCalctr)}}
	執行的 js 算法腳本。
]

#H[掛載方式][
	- 在 {{nameof(Ngaq.Core.Shared.StudyPlan.Models.Po.WeightCalculator.PoWeightCalculator)}} 中：
		- {{nameof(Ngaq.Core.Shared.StudyPlan.Models.Po.WeightCalculator.PoWeightCalculator.Type)}} 設為 {{nameof(Ngaq.Core.Shared.StudyPlan.Models.Po.WeightCalculator.EWeightCalculatorType.JsV1)}}。
		- {{nameof(Ngaq.Core.Shared.StudyPlan.Models.Po.WeightCalculator.PoWeightCalculator.Text)}} 填入完整 js 腳本。
	- 運行時由 {{nameof(Ngaq.Backend.Domains.StudyPlan.Svc.SvcStudyPlan)}} 構造算法實例。
]

#H[運行時上下文][
	腳本在 Jint 引擎中執行，可直接使用以下全局變量：
	- {{nameof(Ngaq)}}.WordsJson:
		- 字符串，內容是單詞列表 JSON。
		- 建議使用 `const words = JSON.parse(Ngaq.WordsJson ?? "[]")` 解析。
	- {{nameof(Ngaq)}}.CalcArgJson:
		- 字符串，內容是權重參數 JSON，可能為 `"null"`。
		- 建議使用 `const arg = JSON.parse(Ngaq.CalcArgJson ?? "{}")` 解析。
	- console.log(...args):
		- 可輸出運行日誌到後端 logger。
]

#H[返回值契約][
	- 腳本必須 `return` 一個 JSON 字符串。
	- 返回空字符串會觸發業務錯誤。
	- 返回內容可被反序列化到 {{nameof(Ngaq.Core.Shared.Word.Svc.JsWeightResult)}}。

	推薦返回結構：
	```json
	{
	  "Opt": {
	    "SortBy": "Weight",
	    "ResultType": "AsyEIWordWeightResult"
	  },
	  "Results": [
	    {
	      "StrId": "word-id",
	      "Weight": 1.23,
	      "Index": 0
	    }
	  ],
	  "Props": {
	    "Algo": "YourPluginName"
	  }
	}
	```

	字段說明：
	- Opt.SortBy:
		- 支持 `Weight` 或 `Index`。
		- 對應 {{nameof(Ngaq.Core.Shared.Word.Models.Weight.ESortBy)}}。
	- Opt.ResultType:
		- 建議固定填 `AsyEIWordWeightResult`。
	- Results:
		- 每項對應 {{nameof(Ngaq.Core.Word.Models.Weight.WordWeightResult)}}。
		- `StrId` 必填；`Weight` 建議為有限數字；`Index` 可用於穩定排序。
	- Props:
		- 可選。用於回傳插件自定義診斷信息。
]

#H[最小可用示例][
	```js
	const words = JSON.parse(Ngaq.WordsJson ?? "[]");
	const arg = JSON.parse(Ngaq.CalcArgJson ?? "{}");
	const baseWeight = Number(arg.BaseWeight ?? 0);
	const step = Number(arg.Step ?? 1);

	const results = words.map((w, i) => ({
	  StrId: String(w.Id ?? w.StrId ?? ""),
	  Weight: baseWeight + i * step,
	  Index: i
	}));

	return JSON.stringify({
	  Opt: {
	    SortBy: "Weight",
	    ResultType: "AsyEIWordWeightResult"
	  },
	  Results: results,
	  Props: {
	    Algo: "SampleFromWordsAndArg",
	    Count: results.length
	  }
	});
	```
]

#H[常見錯誤][
	- 腳本為空或全空白：
		- 會觸發 `JsWeightCalcCodeEmpty`。
	- 腳本返回空字符串：
		- 會觸發 `JsWeightCalcReturnedEmpty`。
	- 返回內容不是合法 JSON 或結構不匹配：
		- 會觸發 `JsWeightCalcReturnedInvalidJson`。
	- 腳本內拋異常：
		- 會被包裝為 `JsWeightCalcExecFailed`。
]

#H[開發建議][
	- 對 `Ngaq.CalcArgJson` 的每個字段做兜底處理，避免 `NaN`。
	- 對輸入單詞數組做空值與字段缺失防禦。
	- 先保證 `Results` 中每項都有 `StrId`，再逐步增加複雜策略。
	- 在算法迭代時保留 `Props` 版本字段，便於排查線上問題。
]

""")]
file class _{
	
}
