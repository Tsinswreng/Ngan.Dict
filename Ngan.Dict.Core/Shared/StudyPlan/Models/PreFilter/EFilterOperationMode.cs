namespace Ngaq.Core.Shared.StudyPlan.Models.PreFilter;
[Doc($@"篩選操作模式")]
[DoNotRenameMembers]
public enum EFilterOperationMode{
	//無效值
	[Doc($@"空操作")]
	Null=0,
	///Lang IncludeAny [en, jp] -> Lang == 'en' OR Lang == 'jp'
	[Doc($@"任一值匹配")]
	IncludeAny,
	/// Lang IncludeAll [en, jp] -> Lang == 'en' AND Lang == 'jp'
	[Doc($@"全部值同時匹配")]
	IncludeAll,
	/// 暫不設 ExcludeAny
	[Doc($@"排除全部值")]
	ExcludeAll,
	/// >
	[Doc($@"大於")]
	Gt,
	/// >=
	[Doc($@"大於等於")]
	Ge,
	/// <
	[Doc($@"小於")]
	Lt,
	/// <=
	[Doc($@"小於等於")]
	Le,
	/// =     Eq Null -> `IS NULL`
	[Doc($@"等於")]
	Eq,
	/// !=    Ne Null -> `IS NOT NULL`
	[Doc($@"不等於")]
	Ne,
}
