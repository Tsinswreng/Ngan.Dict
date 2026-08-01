#define Impl
using Ngaq.Core.Shared.Word.Models.Weight;

namespace Ngaq.Core.Word.Models.Weight;
public partial class WeightResult: IWeightResult{
	public IOptWeightResult Opt{get;set;}
	#if Impl
		 = new OptWeightResult();
	#endif
	public IAsyncEnumerable<IWordWeightResult>? Results{get;set;}
	public IDictionary<str, obj?>? Props{get;set;}

}
