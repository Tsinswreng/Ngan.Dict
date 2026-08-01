namespace Ngaq.Core.Frontend.User;

using Ngaq.Core.Shared.User.Models.Po.Device;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.User.UserCtx;
using Ngaq.Core.Tools;
using Tsinswreng.CsTempus;

public class FrontendUserCtx : IFrontendUserCtx{
	[Obsolete("use LocalUserId or LoginUserId")]
	public IdUser UserId{get=>LocalUserId; set=>LocalUserId= value;}
	public IDictionary<str, obj?>? Props{get;set;}
	public str? AccessToken{get;set;}
	public UnixMs AccessTokenExpireAt{get;set;}
	public str? RefreshToken{get;set;}
	public UnixMs RefreshTokenExpireAt{get;set;}
	public IdUser LocalUserId{
		get;
		set{
			if(value.IsNullOrDefault()){
				throw new ArgumentNullException("LocalUserId cannot be empty or zero");
			}
			field = value;
		}
	}
	public IdUser LoginUserId{get;set;}
	public IdClient ClientId{get;set;}

}

// public static class ExtnUserCtxMgr{
// 	public static IFrontendUserCtx GetFrontendUserCtx(
// 		this IUserCtxMgr z
// 	){
// 		var userCtx = z.GetUserCtx();
// 		if(userCtx is IFrontendUserCtx f){
// 			return f;
// 		}
// 		throw new NotSupportedException("UserCtx is not a FrontendUserCtx");
// 	}
// }
