namespace Ngan.Dict.Server.Domains.User.Svc;

using Ngan.Dict.Server.Domains.User.Dto;
using Ngan.Dict.Core.Shared.User.Models.Resp;
using Ngan.Dict.Core.Shared.User.UserCtx;
using Ngan.Dict.Backend.Db.TswG;
using Tsinswreng.CsErr;
using Tsinswreng.CsSql;

public interface ISvcToken{
	public Task<RespGenRefreshToken> GenEtStoreRefreshToken(
		IDbFnCtx Ctx
		,IUserCtx User
		,CT Ct
	);

	public RespGenAccessToken GenAccessToken(
		ReqGenAccessToken Req
	);

	public Task<IAnswer<RespValidateAccessToken>> ValidateAccessToken(
		ReqValidateAccessToken Req, CT Ct
	);

	public Task<IAnswer<RespRefreshBothToken>> ValidateEtRefreshTheToken(
		IUserCtx User, str RefreshToken, CT Ct
	);

	public Task<nil> RevokeTokensForLogout(
		IDbFnCtx Ctx
		,IUserCtx User
		,CT Ct
	);

}

