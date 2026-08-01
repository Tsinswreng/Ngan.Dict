namespace Ngan.Dict.Core.Shared.User.Models.Resp;

using Ngan.Dict.Core.Shared.Base.Models.Resp;
using Ngan.Dict.Core.Shared.User.Models.Po.User;
using Tsinswreng.CsTempus;

public partial class RespLogin: BaseResp{
	public str AccessToken{get;set;} = "";
	public UnixMs AccessTokenExpireAt{get;set;}
	public str RefreshToken{get;set;} = "";
	public UnixMs RefreshTokenExpireAt{get;set;}
	public str UserId{get;set;} = "";
}
