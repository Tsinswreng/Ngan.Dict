namespace Ngan.Dict.Server.Domains.User.Dto;

using Ngan.Dict.Core.Shared.Base.Models.Req;


public partial class ReqValidateAccessToken:BaseReq{
	public string AccessToken { get; set; } = "";
}

