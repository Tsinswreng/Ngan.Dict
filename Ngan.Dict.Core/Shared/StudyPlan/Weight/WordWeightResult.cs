using Ngaq.Core.Infra.IF;

namespace Ngaq.Core.Word.Models.Weight;


public partial struct WordWeightResult
	:IWordWeightResult
	,IAppSerializable
{
	public string StrId{get;set;}
	public f64 Weight{get;set;}
	public u64 Index{get;set;}
}
