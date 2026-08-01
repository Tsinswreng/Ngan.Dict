namespace Ngaq.Ui.Views.Settings.Lang;

using Avalonia.Controls;
using Avalonia.Data;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.Ctrls;
using Ngaq.Ui.Tools;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsI18n;

using Ctx = VmCfgLang;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

public partial class ViewCfgLang: AppViewBase<Ctx>{

	public ViewCfgLang(){
		Ctx = App.DiOrMk<Ctx>();
		Render();
		Loaded += async (s,e)=>{
			if(Ctx is null){
				return;
			}
			await Ctx.InitUiLangOptions(default);
		};
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
			sp.A(new TextBlock(), o=>{
				o.Text = I[K.Lang];
			})
			.A(new AutoCompleteBox(), o=>{
				o.FilterMode = AutoCompleteFilterMode.StartsWith;
				o.MinimumPrefixLength = 0;
				o.Bind(
					AutoCompleteBox.ItemsSourceProperty,
					CBE.Mk<Ctx>(x=>x.UiLangOptions, Mode: BindingMode.OneWay)
				);
				o.Bind(
					AutoCompleteBox.TextProperty,
					CBE.Mk<Ctx>(x=>x.LangInput, Mode: BindingMode.TwoWay)
				);
			})
			.A(new TextBlock(), o=>{
				o.Text = I[K.TakeEffectAfterTheProgramIsRestarted];
				o.FontSize = UiCfg.Inst.BaseFontSize * 0.9;
				o.Foreground = UiCfg.Inst.MainColor;
			})
			;
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

