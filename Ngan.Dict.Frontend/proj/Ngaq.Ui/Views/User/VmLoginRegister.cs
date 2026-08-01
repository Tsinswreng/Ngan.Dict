namespace Ngaq.Ui.Views.User;

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Ngaq.Core.Frontend.Kv;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Shared.Kv.Svc;
using Ngaq.Core.Shared.User.Models.Po.Device;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.User.Models.Req;
using Ngaq.Core.Shared.User.Svc;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Ui.Infra;
using Tsinswreng.CsErr;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;
using Ctx = VmLoginRegister;
public partial class VmLoginRegister: ViewModelBase{
	protected VmLoginRegister(){}
	public static Ctx Mk(){
		return new Ctx();
	}
	protected ISvcUser? SvcUser;
	IFrontendUserCtxMgr? UserCtxMgr;
	ISvcKv? SvcKv;
	public VmLoginRegister(
		ISvcUser SvcUser
		,IFrontendUserCtxMgr? UserCtxMgr
		,ISvcKv? SvcKv
	){
		this.SvcUser = SvcUser;
		this.UserCtxMgr = UserCtxMgr;
		this.SvcKv = SvcKv;
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmLoginRegister(){
		{
			var o = new Ctx();
			Samples.Add(o);
		}
	}

	protected str _Email="";
	public str Email{
		get{return _Email;}
		set{SetProperty(ref _Email, value);}
	}

	protected str _Password="";
	public str Password{
		get{return _Password;}
		set{SetProperty(ref _Password, value);}
	}

	protected str _ConfirmPassword = "";
	public str ConfirmPassword{
		get{return _ConfirmPassword;}
		set{SetProperty(ref _ConfirmPassword, value);}
	}

	protected ICollection<object?> _Msgs = new ObservableCollection<object?>();
	public ICollection<object?> Msgs{
		get{return _Msgs;}
		set{SetProperty(ref _Msgs, value);}
	}

	public ViewModelBase AddMsg(object? Msg){
			Msgs.Add(Msg);
	#if DEBUG
			Console.WriteLine(Msg);
	#endif
			return this;
		}

	/// 登錄成功事件。由View層決定跳轉行爲，避免在Vm層直接操作導航。
	public event EventHandler<EvtArgMsg>? OnLoginSucceeded;


	public bool CheckRegister(){
		//TODO 一次校驗多條
		this.Msgs.Clear();
		if(str.IsNullOrEmpty(Password)){
			this.AddMsg(I18n.Get(K.__CannotBeEmpty, I18n[K.Password]));
			return false;
		}
		if(Password != ConfirmPassword){
			this.AddMsg(I18n[K.PasswordMismatch]);
			return false;
		}
		if(ValidateEmail(Email) == false){
			this.AddMsg(I18n[K.EmailIsNotValid]);
			return false;
		}
		return true;
	}


	public async Task<nil> Register(CT Ct){
		if(!CheckRegister()){
			return NIL;
		}
		await Task.Run(async()=>{
			if(SvcUser is null|| UserCtxMgr is null){
				return;
			}
			var User = UserCtxMgr.GetUserCtx();
			var reqAddUser = new ReqAddUser{
				Email = Email
				,Password = Password
			};
			await SvcUser.AddUser(User, reqAddUser, Ct);
		});
		ShowToast(I18n[K.RegistrationSucceededPleaseLogIn]);
		return NIL;
	}
	public async Task<nil> LoginAsy(CT Ct){
		Msgs.Clear();
		if(str.IsNullOrWhiteSpace(Email)){
			AddMsg(I18n.Get(K.__CannotBeEmpty, I18n[K.Email]));
			return NIL;
		}
		if(str.IsNullOrWhiteSpace(Password)){
			AddMsg(I18n.Get(K.__CannotBeEmpty, I18n[K.Password]));
			return NIL;
		}

		var z = this;
		try{
			await Task.Run(async()=>{
				if(z.UserCtxMgr is null
					|| z.SvcUser is null || z.SvcKv is null
				){
					return;
				}
				var User = z.UserCtxMgr.GetUserCtx();

				var ClientId = await z.SvcKv.GetByOwnerEtKStr(IdUser.Zero, KeysClientKv.ClientId, Ct);
				if(ClientId is null){
					throw new FatalLogicErr("ClientId is null. ClientId should be in Db when App is launched");
				}
				var reqLogin = new ReqLogin{
					Email = Email
					,Password = Password
					,KeepLogin = true
					,UserIdentityMode = ReqLogin.EUserIdentityMode.Email
					,CliendId = IdClient.FromLow64Base(ClientId.GetVStr()??"")
				};
				await z.SvcUser.Login(User, reqLogin, Ct);
			}, Ct);
		}
		catch(Exception Ex){
			AddMsg(Ex.Message);
			HandleErr(Ex);
			return NIL;
		}

		OnLoginSucceeded?.Invoke(this, new EvtArgMsg());
		return NIL;
	}


	//校驗郵箱格式
	public static bool ValidateEmail(str Email){
		var R = MyRegex();
		return R.IsMatch(Email);
	}

	[System.Text.RegularExpressions.GeneratedRegex(@"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$")]
	private static partial System.Text.RegularExpressions.Regex MyRegex();
}

