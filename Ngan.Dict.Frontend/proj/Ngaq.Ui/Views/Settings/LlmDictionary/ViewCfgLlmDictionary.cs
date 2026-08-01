namespace Ngaq.Ui.Views.Settings.LlmDictionary;

using Avalonia.Controls;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.Ctrls;
using Ngaq.Ui.Tools;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsI18n;

using Ctx = VmCfgLlmDictionary;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

public partial class ViewCfgLlmDictionary: AppViewBase<Ctx>{

	public ViewCfgLlmDictionary(){
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
		Root.A(new ScrollViewer(), sv=>{
			sv.SetContent(new StackPanel(), sp=>{
				sp.Classes.A(App.Cls.SpacedStackPanel);
				sp.Spacing = UiCfg.Inst.BaseFontSize/2;
				sp
				.A(new TextBlock(), o=>{o.Text = I[K.ApiUrl];})
				.A(new TextBox(), o=>{o.CBind<Ctx>(o.PropText, x=>x.ApiUrl);})
				.A(new TextBlock(), o=>{o.Text = I[K.ApiKey];})
				.A(new TextBox(), o=>{o.CBind<Ctx>(o.PropText, x=>x.ApiKey);})
				.A(new TextBlock(), o=>{o.Text = I[K.Model];})
				.A(new TextBox(), o=>{o.CBind<Ctx>(o.PropText, x=>x.Model);})
				.A(new TextBlock(), o=>{o.Text = I[K.ExtraBodyJson];})
				.A(new TextBox(), o=>{
					o.AcceptsReturn = true;
					o.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
					o.MinHeight = UiCfg.Inst.BaseFontSize*6;
					o.CBind<Ctx>(o.PropText, x=>x.ExtraBodyJson);
				})
				.A(new TextBlock(), o=>{o.Text = I[K.Prompt];})
				.A(new TextBox(), o=>{
					o.AcceptsReturn = true;
					o.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
					o.MinHeight = UiCfg.Inst.BaseFontSize*10;
					o.CBind<Ctx>(o.PropText, x=>x.Prompt);
				});
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

