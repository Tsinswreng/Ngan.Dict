namespace Ngan.Dict.Core.Frontend.User;

using Ngan.Dict.Core.Shared.User.Models.Po.Device;
using Ngan.Dict.Core.Shared.User.Models.Po.User;
using Ngan.Dict.Core.Shared.User.UserCtx;
using Tsinswreng.CsTempus;


/// do not use UserId in IFrontendUserCtx. Use LocalUserId or RemoteUserId

public interface IFrontendUserCtx: IUserCtx{
	public str? AccessToken{get;set;}
	public UnixMs AccessTokenExpireAt{get;set;}
	public str? RefreshToken{get;set;}
	public UnixMs RefreshTokenExpireAt{get;set;}
	public IdUser LocalUserId{get;set;}
	public IdUser LoginUserId{get;set;}
	public IdClient ClientId{get;set;}
}
