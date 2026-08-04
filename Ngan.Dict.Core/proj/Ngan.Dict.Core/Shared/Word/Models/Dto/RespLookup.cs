using Ngan.Dict.Core.Shared.Base.Models.Req;
using Ngan.Dict.Core.Shared.Base.Models.Resp;
using Ngan.Dict.Core.Shared.Word.Models.DictionaryApi;

namespace Ngan.Dict.Core.Shared.Word.Models.Dto;

public class RespLookup:IResp{
	public IList<DictionaryApiWord> DictionaryApiWords{get;set;}
}

