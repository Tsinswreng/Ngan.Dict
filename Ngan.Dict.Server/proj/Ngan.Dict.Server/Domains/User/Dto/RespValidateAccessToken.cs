namespace Ngan.Dict.Server.Domains.User.Dto;

using System.Security.Claims;
using Ngan.Dict.Core.Shared.Base.Models.Req;
using Ngan.Dict.Core.Shared.Base.Models.Resp;
using Ngan.Dict.Core.Shared.User.Models.Po.User;

public partial class RespValidateAccessToken:BaseResp{
	public ClaimsPrincipal? ClaimsPrincipal{get;set;}
}

