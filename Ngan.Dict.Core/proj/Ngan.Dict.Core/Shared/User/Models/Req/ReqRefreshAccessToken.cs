namespace Ngan.Dict.Core.Shared.User.Models.Req;

using Ngan.Dict.Core.Shared.Base.Models.Req;
using Ngan.Dict.Core.Shared.User.Models.Po.User;



public class ReqRefreshTheToken:BaseReq{
	public str RefreshToken{get;set;} = "";
}
