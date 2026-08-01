namespace Ngaq.Core.Frontend.User.Svc;
using Ngaq.Core.Shared.User.Models.Po.User;

using Tsinswreng.CsTempus;

//[MkFactory(For=typeof(ReqSetRefreshToken))]
public partial class ReqSetRefreshToken{
	public IdUser LoginUserId{get;set;}
	public str RefreshToken{get;set;} = "";
	public UnixMs RefreshTokenExpireAt{get;set;}
}

public interface ISvcTokenStorage{
	public Task<str?> GetRefreshToken(CT Ct);
	public Task<nil> SetRefreshToken(ReqSetRefreshToken Req, CT Ct);
}

