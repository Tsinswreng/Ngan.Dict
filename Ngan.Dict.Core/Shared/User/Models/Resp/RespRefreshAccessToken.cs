namespace Ngaq.Core.Shared.User.Models.Resp;

using Ngaq.Core.Shared.Base.Models.Resp;
using Tsinswreng.CsTempus;

public class RespRefreshBothToken:BaseResp{
	public str AccessToken{get;set;} = "";
	public UnixMs AccessTokenExpireAt{get;set;}
	public str RefreshToken{get;set;} = "";
	public UnixMs RefreshTokenExpireAt{get;set;}
}
