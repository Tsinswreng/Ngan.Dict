using Tsinswreng.CsTools;

namespace Ngaq.Core.Tools.JsonMap;

public class IUiJsonMap{
	public IUiJsonMap(){}
	[Doc(@$"")]
	public Version Version{get;set;} = new Version(1,0,0);//2026_0103_105821
	public IJsonNode Raw{get;set;} = new JsonNode();
	public IJsonNode? Schema{get;set;} = null;
	public IDictionary<str, IUiJsonMapItem>? PathToUiMap{get;set;} = null;
	//public IList<IUiJsonMapItem>? Items{get;set;} = null;
	public str? I18nLang{get;set;} = null;// = "zh-CN";

/*
		"en":{
			"DfltAddBonus":{
				"DisplayName": "...",
				"Descr": "Default add event bonus value."
			}
		},
		"zh":{
			"DfltAddBonus":{
				"Descr": "默認加事件加成的數值。"
			}
		}
 */

	public IJsonNode? I18n{get;set;}
}

public class UiJsonMap:IUiJsonMap{

}


#if false
```json
{
	"Raw": {
		"AddCnt_Bonus": [0xff,0xfff,0xffff,0xfffff],
		"DfltAddBonus": 0xffffffff
	},
	"Schema": { //標準 Json Schema。用于驗證Raw。可選

	},
	"UiMap": [
		{
			"Path": "AddCnt_Bonus",
			"Type": "numberArr",
			"DisplayName": "加事件次數-加成數值",
			"Descr": "加事件次數-加成數值的數值陣列，用於計算加成的加成值。"
		},
		{
			"Path": "DfltAddBonus",//如果是深層的嵌套對象、就用正斜槓或點作分隔符
			"Type": "number",
			"DisplayName": {
				"Type": "RawText",
				"Data": "默認加事件加成",
			},
			"Descr": {
				"Type": "I18nKey",
				"Data": "DfltAddBonus.Descr"
			}
		},

	],
	"LangMap":{
		"en":{
			"DfltAddBonus":{
				"DisplayName": "...",
				"Descr": "Default add event bonus value."
			}
		},
		"zh":{
			"DfltAddBonus":{
				"Descr": "默認加事件加成的數值。"
			}
		}
	}

}

```
#endif
