namespace Ngaq.Client.Svc;

using Tsinswreng.CsCore;
using Ngaq.Core.Models.Sys.Req;
using Ngaq.Core.Infra.Url;
using Ngaq.Core.Shared.User.Models.Req;
using Ngaq.Core.Shared.User.Models.Resp;
using Ngaq.Core.Shared.User.Svc;
using Ngaq.Core.Shared.User.UserCtx;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Kv.Svc;
using Ngaq.Core.Frontend.User.Svc;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Shared.Kv.Models;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Frontend.Kv;
using Ngaq.Core.Infra;
using Ngaq.Core.Infra.IF;
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
