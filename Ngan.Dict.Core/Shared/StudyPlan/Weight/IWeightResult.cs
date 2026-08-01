using Ngaq.Core.Shared.Word.Models.Weight;
using Tsinswreng.CsTools;

namespace Ngaq.Core.Word.Models.Weight;


public partial interface IWeightResult{
	[Doc(@$"Option")]
	public IOptWeightResult Opt{get;set;}
	[Doc(@$"Results of each word")]
	public IAsyncEnumerable<IWordWeightResult>? Results{get;set;}
	public IDictionary<str, obj?>? Props{get;set;}
}
