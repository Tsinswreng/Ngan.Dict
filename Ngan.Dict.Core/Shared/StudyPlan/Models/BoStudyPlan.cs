using Ngaq.Core.Shared.StudyPlan.Models.Po.PreFilter;
using Ngaq.Core.Shared.StudyPlan.Models.Po.StudyPlan;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightArg;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightCalculator;
using Ngaq.Core.Shared.StudyPlan.Models.PreFilter;
using Ngaq.Core.Shared.Word.Svc;
using Ngaq.Core.Tools;
using Ngaq.Core.Tools.Json;
using Ngaq.Core.Word.Svc;
using System.Text.Json;
using Tsinswreng.CsTools;

namespace Ngaq.Core.Shared.StudyPlan.Models;

[Doc($$"""
#Sum[學習方案聚合模型]
#Descr[聚合一套學習方案在運行期所需的持久化實體、業務模型與權重參數。]
""")]
public class BoStudyPlan{
	[Doc($@"學習方案實體")]
	public PoStudyPlan? PoStudyPlan{get;set;} = null;
	[Doc($@"前置篩選器實體")]
	/// 數據庫實體
	public PoPreFilter? PoPreFilter{get;set;} = null;
	[Doc($@"前置篩選器業務模型")]
	/// 業務模型
	public PreFilter.PreFilter? PreFilter{get;set;} = null;

	[Doc($@"權重算法實體")]
	public PoWeightCalculator? PoWeightCalculator{get;set;} = null;
	[Doc($@"運行期權重算法實例")]
	public IWeightCalctr? WeightCalctr{get;set;} = null;

	[Doc($@"權重參數實體")]
	public PoWeightArg? PoWeightArg{get;set;} = null;

	[Doc($@"反序列化後的權重參數節點")]
	public IDictionary<str, obj?>? WeightArg{get;set;} = null;
}

public static class ExtnBoStudyPlan{
	extension(BoStudyPlan z){
		[Doc(@$"原地修改self 不變地址")]
		public void FromJnStudyPlan(
			JnStudyPlan JnStudyPlan
		){
			z.PoStudyPlan = JnStudyPlan.StudyPlan;
			z.PoPreFilter = JnStudyPlan.PreFilter;
			z.PoWeightCalculator = JnStudyPlan.WeightCalculator;
			z.PoWeightArg = JnStudyPlan.WeightArg;

			z.PreFilter = null;
			if(z.PoPreFilter is { } poPreFilter){
				var preFilter = new PreFilter.PreFilter();
				preFilter.FromPo(poPreFilter);
				z.PreFilter = preFilter;
			}

			z.WeightArg = null;
			if(
				z.PoWeightArg is { } poWeightArg
				&& poWeightArg.Type == EWeightArgType.Json
				&& !string.IsNullOrWhiteSpace(poWeightArg.Text)
			){
				var json = poWeightArg.Text;
				if(!string.IsNullOrWhiteSpace(json)){
					z.WeightArg = ParseJsonObjDict(json);
				}
			}

			z.WeightCalctr = null;
			if(
				z.PoWeightCalculator is { } poWeightCalculator
				&& poWeightCalculator.Type == EWeightCalculatorType.JsV1
				&& !string.IsNullOrWhiteSpace(poWeightCalculator.Text)
			){
				var jsCode = poWeightCalculator.Text;
				if(!string.IsNullOrWhiteSpace(jsCode)){
					z.WeightCalctr = new JsWeightCalctr(AppJsonSerializer.Inst, jsCode);
				}
			}
		}

		static IDictionary<str, obj?> ParseJsonObjDict(str Json){
			using var doc = JsonDocument.Parse(Json);
			if(doc.RootElement.ValueKind != JsonValueKind.Object){
				return new Dictionary<str, obj?>();
			}
			return (Dictionary<str, obj?>)JsonElementToObj(doc.RootElement)!;
		}

		static obj? JsonElementToObj(JsonElement Element){
			switch(Element.ValueKind){
				case JsonValueKind.Object:{
					var dict = new Dictionary<str, obj?>();
					foreach(var p in Element.EnumerateObject()){
						dict[p.Name] = JsonElementToObj(p.Value);
					}
					return dict;
				}
				case JsonValueKind.Array:{
					var list = new List<obj?>();
					foreach(var x in Element.EnumerateArray()){
						list.Add(JsonElementToObj(x));
					}
					return list;
				}
				case JsonValueKind.String:
					return Element.GetString();
				case JsonValueKind.Number:
					if(Element.TryGetInt64(out var i64v)){
						return i64v;
					}
					if(Element.TryGetDouble(out var f64v)){
						return f64v;
					}
					return Element.ToString();
				case JsonValueKind.True:
					return true;
				case JsonValueKind.False:
					return false;
				case JsonValueKind.Null:
					return null;
				default:
					return Element.ToString();
			}
		}
	}
}


// //只是草稿
// namespace Ngaq.Core.Shared.Word.Models;

// using Ngaq.Core.Tools.Json;
// using Ngaq.Core.Word.Svc;
// using Tsinswreng.CsTools;

// public struct IdStudyPlan{

// }

// public class FilterInExClude<T>{
// 	public ISet<T> Include{get;set;}
// 	public ISet<T> Exclude{get;set;}
// }

// public class FieldWithFilter<T>{
// 	
// 	/// WordProp中ʹKStr
// 	
// 	public str Field{get;set;}
// 	public FilterInExClude<T> Filter{get;set;}
// }

// 
// /// 宜作用于 從數據庫取詞旹、即拼條件于sql中。故不適 用腳本過濾表達式㕥叶㞢
// 
// public class PreFilter{
// 	public IList<FieldWithFilter<str>> FieldFilter{get;set;}
// }
// //TODO 按時間, 各事件ʹ次 等

// public class StudyPlan{
// 	public IdStudyPlan Id{get;set;}
// 	// public IList<str> LangIncluded{get;set;}
// 	// public IList<str> LangExcluded{get;set;}
// 	public str UniqueName{get;set;}
// 	public str Descr{get;set;}
// 	// public IWeightCalctr WeightCalctr{get;set;}
// 	// public IKvNode CfgWeightCalctr{get;set;}
// }

// public enum EWeightCalctrType{
// 	Cli,
// 	Dll,//不支持
// 	Js,
// }

// public class CfgWeight{
// 	public IWeightCalctr WeightCalctr{get;set;}
// 	public EWeightCalctrType Type{get;set;}
// 	public IJsonNode Arg{get;set;}
// }


// #if false
// {
// 	Id: "",
// 	UniqueName: "EnglishDefault",
// 	Descr: "",
// 	WeightCalctr: {
// 		Type: "Cli",
// 		Content: "./NgaqWeight.exe",
// 		Arg: {
// 			AddCnt_Bonus: [0xff,0xfff,0xffff,0xfffff],
// 			DebuffNumerator: 36
// 		}
// 	},
// 	PreFilter: {

// 	},
// }

// #endif
