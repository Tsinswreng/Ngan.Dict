using Ngaq.Core.Frontend.User;

namespace Ngaq.Core.Shared.User.UserCtx;

/// 似乎唯前端用及
[Obsolete(@$"use {nameof(IFrontendUserCtxMgr)}")]
public partial interface IUserCtxMgr{
	IUserCtx GetUserCtx();
}

public partial interface IUserCtxMgr<T>
	where T : IUserCtx
{
	T GetUserCtxT();
}

