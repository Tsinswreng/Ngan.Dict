namespace Ngan.Dict.Ui.Views.Settings.ServerStorage;

using Avalonia.Controls;
using Ngan.Dict.Ui.Infra;
using Ngan.Dict.Ui.Infra.Ctrls;
using Ngan.Dict.Ui.Tools;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsI18n;

using Ctx = VmCfgServerStorage;
using K = Ngan.Dict.Ui.Infra.I18n.KeysUiI18nCommon;

public partial class ViewCfgServerStorage: AppViewBase<Ctx>{

	public ViewCfgServerStorage(){
		Ctx = App.DiOrMk<Ctx>();
		Render();
		this.Classes.A(App.Cls.ViewPadding);
	}

	public GridStack Root = new(IsRow:true);

	protected nil Render(){
		this.SetContent(Root.Grid, o=>{
			o.SetRowDefs([
				new(1, GUT.Star),
				new(1, GUT.Auto),
			]);
		});
		Root.A(new StackPanel(), sp=>{
			sp.Classes.A(App.Cls.SpacedStackPanel);
			sp.A(new TextBlock(), o=>{
				o.Text = I[K.ServerBaseUrl];
			});
			sp.A(new TextBox(), o=>{
				o.CBind<Ctx>(o.PropText, x=>x.ServerBaseUrl);
			});
			sp.A(new TextBlock(), o=>{
				o.Text = I[K.SqlitePath];
			});
			sp.A(new TextBox(), o=>{
				o.CBind<Ctx>(o.PropText, x=>x.SqlitePath);
			});
		});
		Root.A(new OpBtn(), o=>{
			o._Button.StretchCenter();
			o.VerticalAlignment = VAlign.Bottom;
			o.BtnContent = I[K.Save];
			o._Button.Background = UiCfg.Inst.MainColor;
			o.SetExe(Ct=>Ctx?.Save(Ct));
		});
		return NIL;
	}
}


