namespace Ngaq.Ui.Views.User.AboutMe;
using System.Collections.ObjectModel;
using Avalonia.Media;
using Avalonia.Threading;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Shared.User.UserCtx;
using Ngaq.Core.Tools;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.I18n;
using Ngaq.Ui.User;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;
using Ctx = VmAboutMe;
public partial class VmAboutMe: ViewModelBase{
	protected VmAboutMe(){}
	public static Ctx Mk(){
		return new Ctx();
	}
	public static ObservableCollection<Ctx> Samples = [];
	static VmAboutMe(){
		#if DEBUG
		{
			var o = new Ctx();
			Samples.Add(o);
		}
		#endif
	}

	IFrontendUserCtxMgr? UserCtxMgr;
	public VmAboutMe(
		IFrontendUserCtxMgr? UserCtxMgr
	){
		this.UserCtxMgr = UserCtxMgr;
		if(this.UserCtxMgr is not null){
			this.UserCtxMgr.OnUserCtxChanged += HandleUserCtxChanged;
		}
		Init();
	}

	public nil Init(){
		RefreshByUserCtx();
		return NIL;
	}

	/// 監聽登錄上下文變更後，刷新界面關聯字段。
	protected void HandleUserCtxChanged(object? Sender, EvtArgUserCtxChanged Evt){
		Dispatcher.UIThread.Post(()=>{
			RefreshByUserCtx(Evt.UserCtx);
		});
	}

	protected nil RefreshByUserCtx(IFrontendUserCtx? User = null){
		if(User is null){
			if(UserCtxMgr is null){
				return NIL;
			}
			User = UserCtxMgr.GetUserCtx();
		}

		if(User.LoginUserId.IsNullOrDefault()){
			IsLoggedIn = false;
			UserIdRepr = I18n[K.NotLoggedIn];
		}else{
			IsLoggedIn = true;
			UserIdRepr = $"{I18n[K.UserId]}: {User.LoginUserId}";
		}
		AvatarImg = DfltAvatar.Img;
		return NIL;
	}

	/// 控制 UserId 區域點擊後的跳轉目標。
	public bool IsLoggedIn{
		get;
		set{SetProperty(ref field, value);}
	}

	protected str _UserIdRepr = AppI18n.Inst[K.NotLoggedIn];
	public str UserIdRepr{
		get{return _UserIdRepr;}
		set{SetProperty(ref _UserIdRepr, value);}
	}

	protected IImage? _AvatarImg = DfltAvatar.Img;
	public IImage? AvatarImg{
		get{return _AvatarImg;}
		set{SetProperty(ref _AvatarImg, value);}
	}





}
