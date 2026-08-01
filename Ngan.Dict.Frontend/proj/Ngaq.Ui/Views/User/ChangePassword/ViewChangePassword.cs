namespace Ngaq.Ui.Views.User.ChangePassword;

using Avalonia.Controls;
using Avalonia.Styling;
using Ngaq.Ui;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.Ctrls;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsI18n;
using Ctx = VmChangePassword;
public partial class ViewChangePassword
	:AppViewBase<Ctx>
{

	public ViewChangePassword(){
		Ctx = App.DiOrMk<Ctx>();
		Style();
		Render();
	}

	public partial class Cls{

	}


	protected nil Style(){
		return NIL;
	}

	GridStack Root = new(IsRow: true);
	public void Render(){
		this.Content = Root.Grid;
		Root.SetRowDefs([
			new(1, GUT.Star)
		]);
		Root.A(new StackPanel(), Sp=>{
			Sp.A(new TextBlock(), o=>{
				o.Text = "舊密碼";
				o.VerticalAlignment = VAlign.Center;
			})
			.A(new TextBox(), o=>{
				o.PasswordChar = '*';
				o.CBind<Ctx>(o.PropText,x=>x.OldPassword);
				o.VerticalAlignment = VAlign.Center;
			})
			.A(new TextBlock(), o=>{
				o.Text = "新密碼";
				o.VerticalAlignment = VAlign.Center;
			})
			.A(new TextBox(), o=>{
				o.PasswordChar = '*';
				o.CBind<Ctx>(o.PropText,x=>x.NewPassword);
				o.VerticalAlignment = VAlign.Center;
			})
			.A(new TextBlock(), o=>{
				o.Text = "確認新密碼";
				o.VerticalAlignment = VAlign.Center;
			})
			.A(new TextBox(), o=>{
				o.PasswordChar = '*';
				o.CBind<Ctx>(o.PropText,x=>x.ConfirmPassword);
				o.VerticalAlignment = VAlign.Center;
			})
			.A(new OpBtn(), o=>{
				o._Button.Content = "修改密碼";
				o.SetExe(Ct=>Ctx?.ChangePassword(Ct));
			})
			;
		});
	}
}

