using Ngan.Dict.Core.Shared.Word.Models.Dto;
using Ngan.Dict.Core.Shared.Word.Models.Po.Word;

namespace Ngan.Dict.Core.Shared.Word.Svc;

public class RespUpdPoWord{
	public bool HasUpdatedBizId{get;set;}
	public RespUpdBizId? RespUpdBizId{get;set;}
	public IdWord FinalId{get;set;}
}
