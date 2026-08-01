namespace Ngaq.Ui.Views.User.Profile;
using System.Collections.ObjectModel;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Models.Sys.Req;
using Ngaq.Core.Shared.User.Svc;
using Ngaq.Core.Tools;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.I18n;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

using Ctx = VmUserProfile;
public partial class VmUserProfile: ViewModelBase{
	//蔿從構造函數依賴注入、故以靜態工廠代無參構造器
	protected VmUserProfile(){}
	public static Ctx Mk(){
		return new Ctx();
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmUserProfile(){
		#if DEBUG
		{
			var o = new Ctx();
			Samples.Add(o);
		}
		#endif
	}

	ISvcUser? SvcUser;
	IFrontendUserCtxMgr? UserCtxMgr;
	/// 登出成功後通知View層執行頁面跳轉。
	public event EventHandler<EvtArgMsg>? OnLogoutSucceeded;
	public VmUserProfile(
		ISvcUser? SvcUser
		,IFrontendUserCtxMgr? UserCtxMgr
	){
		this.SvcUser = SvcUser;
		this.UserCtxMgr = UserCtxMgr;
		RefreshByUserCtx();
	}

	/// 顯示在個人頁面的 UserId 文本。
	public str UserIdRepr{
		get;
		set{SetProperty(ref field, value);}
	} = AppI18n.Inst[K.NotLoggedIn];

	/// 根據當前登錄上下文刷新 UserId 顯示。
	protected nil RefreshByUserCtx(){
		if(UserCtxMgr is null){
			UserIdRepr = I18n[K.NotLoggedIn];
			return NIL;
		}
		var User = UserCtxMgr.GetUserCtx();
		if(User.LoginUserId.IsNullOrDefault()){
			UserIdRepr = I18n[K.NotLoggedIn];
		}else{
			UserIdRepr = User.LoginUserId+"";
		}
		return NIL;
	}

/*
	public str YYY{
		get;
		set{SetProperty(ref field, value);}
	}="";
 */

	public async Task<nil> LogoutAsy(CT Ct){
		if(AnyNull(SvcUser, UserCtxMgr)){
			return NIL;
		}
		var User = UserCtxMgr.GetUserCtx();
		await Task.Run(async()=>{
			await SvcUser.Logout(User, new ReqLogout{}, Ct);
		});
		OnLogoutSucceeded?.Invoke(this, new EvtArgMsg());
		return NIL;
	}
}

