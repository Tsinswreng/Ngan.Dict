namespace Ngaq.Core.Frontend.User;

public class EvtArgUserCtxChanged : EventArgs{
	public IFrontendUserCtx? UserCtx{get;set;}
}


/// 前端ViewModel 每次取UserCtx璫用 UserCtxMgr 洏非直ᵈ依賴注入UserCtx
/// 勿遷至Ngaq.Ui程序集、緣其Di在Ngaq.Backend中 配
public interface IFrontendUserCtxMgr{
	/// 每次當返同一引用
	public IFrontendUserCtx GetUserCtx();
	public nil SetUserCtx(IFrontendUserCtx UserCtx);
	public event EventHandler<EvtArgUserCtxChanged>? OnUserCtxChanged;

}


