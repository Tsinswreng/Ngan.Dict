namespace Ngan.Dict.Client.Svc;

using Tsinswreng.CsCore;
using Ngan.Dict.Core.Models.Sys.Req;
using Ngan.Dict.Core.Infra.Url;
using Ngan.Dict.Core.Shared.User.Models.Req;
using Ngan.Dict.Core.Shared.User.Models.Resp;
using Ngan.Dict.Core.Shared.User.Svc;
using Ngan.Dict.Core.Shared.User.UserCtx;
using Ngan.Dict.Core.Shared.User.Models.Po.User;
using Ngan.Dict.Core.Shared.Kv.Svc;
using Ngan.Dict.Core.Frontend.User.Svc;
using Ngan.Dict.Core.Frontend.User;
using Ngan.Dict.Core.Shared.Kv.Models;
using Ngan.Dict.Core.Shared.Word.Models.Po.Kv;
using Ngan.Dict.Core.Frontend.Kv;
using Ngan.Dict.Core.Infra;
using Ngan.Dict.Core.Infra.IF;
using Tsinswreng.CsTempus;

public partial class ClientUser
	:ISvcUser
{
	ISvcKv SvcKv;
	IHttpCaller HttpCaller;
	IFrontendUserCtxMgr UserCtxMgr;
	ISvcTokenStorage SvcTokenStorage;

	public ClientUser(
		ISvcKv SvcKv
		,IHttpCaller HttpCaller
		,IFrontendUserCtxMgr UserCtxMgr
		,ISvcTokenStorage SvcTokenStorage
	){
		this.SvcKv = SvcKv;
		this.HttpCaller = HttpCaller;
		this.UserCtxMgr = UserCtxMgr;
		this.SvcTokenStorage = SvcTokenStorage;
	}

	[Impl]
	public async Task<nil> AddUser(
		IUserCtx User
		,ReqAddUser Req
		,CT Ct
	){
		await HttpCaller.Post<ReqAddUser, nil>(
			KeysUrl.OpenUser.AddUser
			,Req
			,Ct
		);
		return NIL;
	}

	[Impl]
	public async Task<RespLogin> Login(
		IUserCtx User
		,ReqLogin ReqLogin, CT Ct
	){
		var R = await HttpCaller.Post<ReqLogin, RespLogin>(
			KeysUrl.OpenUser.Login
			,ReqLogin
			,Ct
		);
		var UserCtx = UserCtxMgr.GetUserCtx();

		UserCtx.RefreshToken = R.RefreshToken;
		UserCtx.RefreshTokenExpireAt = R.RefreshTokenExpireAt;
		UserCtx.AccessToken = R.AccessToken;
		UserCtx.AccessTokenExpireAt = R.AccessTokenExpireAt;

		UserCtx.LoginUserId = IdUser.FromLow64Base(R.UserId);
		UserCtx.Props??=new Dictionary<str,obj?>();
		//UserCtx.Props["PoUser"] = R.PoUser;
		UserCtxMgr.SetUserCtx(UserCtx);
		await SvcTokenStorage.SetRefreshToken(new ReqSetRefreshToken{
			RefreshToken = R.RefreshToken,
			RefreshTokenExpireAt = R.RefreshTokenExpireAt,
			LoginUserId = UserCtx.LoginUserId,
		}, Ct);
		return R;
	}


	[Impl]
	public async Task<nil> Logout(IUserCtx User, ReqLogout ReqLogout, CT Ct){
		await HttpCaller.Post<ReqLogout, nil>(
			KeysUrl.ApiUser.Logout
			,ReqLogout
			,Ct
		);
		await FrontendLogout(Ct);
		return NIL;
	}


	/// 僅前端操作、未調後端登出接口

	/// <param name="User"></param>
	/// <param name="ReqLogout"></param>
	/// <param name="Ct"></param>
	/// <returns></returns>
	[Impl]
	public async Task<nil> FrontendLogout(CT Ct){
		var OldCtx = UserCtxMgr.GetUserCtx();
		OldCtx.LoginUserId = IdUser.Zero;
		OldCtx.RefreshToken = null;
		OldCtx.RefreshTokenExpireAt = UnixMs.Zero;
		OldCtx.AccessToken = null;
		OldCtx.AccessTokenExpireAt = UnixMs.Zero;

		await SvcTokenStorage.SetRefreshToken(new ReqSetRefreshToken{
			RefreshToken = OldCtx.RefreshToken,
			RefreshTokenExpireAt = OldCtx.RefreshTokenExpireAt,
			LoginUserId = OldCtx.LoginUserId,
		}, Ct);
		UserCtxMgr.SetUserCtx(OldCtx);
		return NIL;
	}

}
