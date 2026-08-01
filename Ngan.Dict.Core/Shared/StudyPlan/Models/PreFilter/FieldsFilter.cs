namespace Ngaq.Core.Shared.StudyPlan.Models.PreFilter;

[Doc($@"字段集合對應的篩選條件集合")]
public class FieldsFilter{
	[Doc($@"要應用篩選的字段名列表。
	當設置了多個值時 即爲所有元素都應用相同的 {nameof(Filters)}
	")]
	public IList<str> Fields{get;set;} = [];
	[Doc($@"針對字段列表的條件列表")]
	public IList<FilterItem> Filters{get;set;} = [];
}
