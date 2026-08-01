namespace Ngaq.Core.Shared.User.UserCtx;

public partial class UserCtxMgr : IUserCtxMgr {
	[Obsolete]
	protected static UserCtxMgr? _Inst = null;
	[Obsolete]
	public static UserCtxMgr Inst => _Inst??= new UserCtxMgr();

	public IUserCtx UserCtx = new UserCtx();
	public IUserCtx GetUserCtx() {
		return UserCtx;
	}
}

