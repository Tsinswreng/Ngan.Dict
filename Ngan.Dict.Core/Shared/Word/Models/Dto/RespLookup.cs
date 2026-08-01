using Ngaq.Core.Shared.Base.Models.Req;
using Ngaq.Core.Shared.Base.Models.Resp;
using Ngaq.Core.Shared.Word.Models.DictionaryApi;

namespace Ngaq.Core.Shared.Word.Models.Dto;

public class RespLookup:IResp{
	public IList<DictionaryApiWord> DictionaryApiWords{get;set;}
}

