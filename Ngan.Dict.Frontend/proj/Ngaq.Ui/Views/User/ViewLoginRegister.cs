namespace Ngaq.Ui.Views.User;

using Avalonia.Controls;
using Avalonia.Threading;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views.User.AboutMe;
using Ngaq.Ui.Views.User.Login;
using Ngaq.Ui.Views.User.Register;
using Tsinswreng.CsI18n;
using Ctx = VmLoginRegister;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;
public partial class ViewLoginRegister
	:AppViewBase
{


	public Ctx? Ctx{
		get{return DataContext as Ctx;}
		set{
			if(ReferenceEquals(DataContext, value)){
				return;
			}
			BindVm(DataContext as Ctx, false);
			DataContext = value;
			BindVm(value, true);
		}
	}

	public ViewLoginRegister(){
		//Ctx = new Ctx();
		Ctx=App.GetRSvc<Ctx>();
		Style();
		Render();
		DetachedFromVisualTree += (s,e)=>{
			BindVm(Ctx, false);
		};
	}

	protected nil BindVm(Ctx? Vm, bool Enable){
		if(Vm is null){
			return NIL;
		}
		if(Enable){
			Vm.OnLoginSucceeded += HandleLoginSucceeded;
		}else{
			Vm.OnLoginSucceeded -= HandleLoginSucceeded;
		}
		return NIL;
	}

	protected void HandleLoginSucceeded(object? Sender, EvtArgMsg Evt){
		Dispatcher.UIThread.Post(()=>{
			Ctx?.ShowToast(I[K.LoginSucceeded]);
			if(ViewNavi?.Back() == true){
				return;
			}
			ViewNavi?.GoTo(ToolView.WithTitle("", new ViewAboutMe()));
		});
	}

	public  partial class Cls_{

	}
	public Cls_ Cls{get;set;} = new Cls_();

	protected nil Style(){
		return NIL;
	}

	protected nil Render(){
		var Switch = new TabControl();
		Content = Switch;
		{{
			var Login = new TabItem();
			Switch.Items.Add(Login);
			{var o = Login;
				o.Header = I[K.Login];
				o.Content = new ViewLogin(){Ctx=Ctx};
			}

			var Register = new TabItem();
			Switch.Items.Add(Register);
			{var o = Register;
				o.Header = I[K.Register];
				o.Content = new ViewRegister(){Ctx=Ctx};
			}
		}}
		return NIL;
	}
}
