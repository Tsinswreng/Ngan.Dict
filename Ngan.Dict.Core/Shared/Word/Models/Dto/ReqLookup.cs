using Ngaq.Core.Shared.Base.Models.Req;
using Ngaq.Core.Shared.Base.Models.Resp;
using Ngaq.Core.Shared.Word.Models.DictionaryApi;

namespace Ngaq.Core.Shared.Word.Models.Dto;
[Obsolete]
public class ReqLookup:IReq{
	public str SearchText{get;set;}
}

