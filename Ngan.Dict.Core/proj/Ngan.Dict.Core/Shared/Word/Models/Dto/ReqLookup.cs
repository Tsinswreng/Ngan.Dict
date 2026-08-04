using Ngan.Dict.Core.Shared.Base.Models.Req;
using Ngan.Dict.Core.Shared.Base.Models.Resp;
using Ngan.Dict.Core.Shared.Word.Models.DictionaryApi;

namespace Ngan.Dict.Core.Shared.Word.Models.Dto;
[Obsolete]
public class ReqLookup:IReq{
	public str SearchText{get;set;}
}

