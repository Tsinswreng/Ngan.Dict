namespace Ngaq.Core.Frontend.User;
public class FrontendUserCtxMgr:
	IFrontendUserCtxMgr
{
	protected static FrontendUserCtxMgr? _Inst = null;
	public static FrontendUserCtxMgr Inst => _Inst??= new FrontendUserCtxMgr();
	public IFrontendUserCtx UserCtx{get;set;} = new FrontendUserCtx();
	public IFrontendUserCtx GetUserCtx(){
		return UserCtx;
	}

	public nil SetUserCtx(IFrontendUserCtx UserCtx){
		this.UserCtx = UserCtx;
		OnUserCtxChanged?.Invoke(this, new EvtArgUserCtxChanged{
			UserCtx = UserCtx
		});
		return NIL;
	}
	public event EventHandler<EvtArgUserCtxChanged>? OnUserCtxChanged;
}

