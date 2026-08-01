namespace Ngaq.Core.Shared.User.Svc;

using Ngaq.Core.Shared.User.Models.Req;
using Ngaq.Core.Shared.User.Models.Resp;
using Ngaq.Core.Models.Sys.Req;
using Ngaq.Core.Shared.User.UserCtx;

public partial interface ISvcUser{
	public Task<nil> AddUser(
		IUserCtx User
		,ReqAddUser ReqAddUser
		,CT Ct
	);

	public Task<RespLogin> Login(IUserCtx User, ReqLogin ReqLogin, CT Ct);

	public Task<nil> Logout(IUserCtx User, ReqLogout ReqLogout, CT Ct);
	
}

