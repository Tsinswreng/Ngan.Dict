namespace Ngaq.Ui.Views.User.ChangePassword;
using System.Collections.ObjectModel;
using Ngaq.Ui.Infra;
using Ngaq.Core.Frontend.User;
using Avalonia.Threading;
using Ngaq.Ui.Infra.I18n;
using Ctx = VmChangePassword;
public partial class VmChangePassword: ViewModelBase, IMk<Ctx>{
	//蔿從構造函數依賴注入、故以靜態工廠代無參構造器
	protected VmChangePassword(){}
	public static Ctx Mk(){
		return new Ctx();
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmChangePassword(){
		#if DEBUG
		{
			var o = new Ctx();
			Samples.Add(o);
		}
		#endif
	}

	#region 依賴注入
	//依賴字段聲明成這樣。全部聲明爲可空類型
	IFrontendUserCtxMgr? UserCtxMgr;
	//公開的有參構造器用于依賴注入。
	public VmChangePassword(
		IFrontendUserCtxMgr? UserCtxMgr
	){
		this.UserCtxMgr = UserCtxMgr;
	}
	#endregion 依賴注入

	public str OldPassword{
		get;
		set{SetProperty(ref field, value);}
	}="";

	public str NewPassword{
		get;
		set{SetProperty(ref field, value);}
	}="";

	public str ConfirmPassword{
		get;
		set{SetProperty(ref field, value);}
	}="";

//異步函數用于給OpBtn綁定
	public async Task<nil> ChangePassword(CT Ct){
		if(AnyNull(UserCtxMgr)){
			return NIL;
		}
		if(NewPassword != ConfirmPassword){
			ShowDialog(I18n.Get(KeysUiI18nCommon.PasswordMismatch));
			return NIL;
		}
		if(str.IsNullOrWhiteSpace(OldPassword) || str.IsNullOrWhiteSpace(NewPassword)){
			ShowDialog(I18n.Get(KeysUiI18nCommon.FillAllFields));
			return NIL;
		}
		try{
			await Task.Run(async ()=>{
				// TODO: 調用用戶服務修改密碼
				// 目前先模擬成功
				await Task.Delay(1000); // 模擬網絡延遲
				Dispatcher.UIThread.Post(()=>{
					ShowDialog(I18n.Get(KeysUiI18nCommon.PasswordChangeSuccess));

					ClearFields();
				});
			},Ct);
		}catch(Exception ex){
			HandleErr(ex);
		}
		return NIL;
	}

	private void ClearFields(){
		OldPassword = "";
		NewPassword = "";
		ConfirmPassword = "";
	}
}
