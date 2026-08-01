using Ngaq.Core.Shared.StudyPlan.Models.PreFilter;

namespace Ngaq.Core.Shared.StudyPlan.Models.PreFilter;
[Doc($@"單條篩選條件")]
public class FilterItem{
	[Doc($@"比較/匹配操作")]
	public EFilterOperationMode Operation{get;set;}
	[Doc($@"比較值類型")]
	public EValueType ValueType{get;set;}
	[Doc($@"參與匹配的值列表")]
	public IList<obj?> Values{get;set;} = [];
}
//TODO 璫支持 且 或 非
