#define Impl
using Ngan.Dict.Core.Shared.Word.Models.Weight;

namespace Ngan.Dict.Core.Word.Models.Weight;
public partial class WeightResult: IWeightResult{
	public IOptWeightResult Opt{get;set;}
	#if Impl
		 = new OptWeightResult();
	#endif
	public IAsyncEnumerable<IWordWeightResult>? Results{get;set;}
	public IDictionary<str, obj?>? Props{get;set;}

}
