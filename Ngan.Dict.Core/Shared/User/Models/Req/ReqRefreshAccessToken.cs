namespace Ngaq.Core.Shared.User.Models.Req;

using Ngaq.Core.Shared.Base.Models.Req;
using Ngaq.Core.Shared.User.Models.Po.User;



public class ReqRefreshTheToken:BaseReq{
	public str RefreshToken{get;set;} = "";
}
