using Ngaq.Core.Word.Models.Weight;

namespace Ngaq.Core.Shared.Word.Models.Weight;

public enum ESortBy{
	Weight = 1,
	[Doc(@$"#See[{nameof(IWordWeightResult.Index)}]")]
	Index = 2,
}

public enum EResultType{
	AsyEIWordWeightResult
}


public partial interface IOptWeightResult{
	public ESortBy SortBy{get;set;}
	#if Impl
		= ESortBy.Weight;
	#endif
	public EResultType ResultType{get;set;}
	#if Impl
		= EResultType.Enumerable;
	#endif
}
