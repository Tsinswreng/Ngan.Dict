namespace Ngaq.Ui.Views.Settings;

using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Styling;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views.About;
using Ngaq.Ui.Views.Settings.LearnWord;
using Ngaq.Ui.Views.Settings.LlmDictionary;
using Ngaq.Ui.Views.Settings.ServerStorage;
using Ngaq.Ui.Views.Settings.Hotkey;
using Tsinswreng.AvlnTools.Controls;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.CsI18n;
using Ctx = VmSettings;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;
using Ngaq.Ui.Icons;
using Avalonia.Media;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.Avln.Navi;

public partial class ViewSettings
	:AppViewBase<Ctx>
{
	public ViewSettings(){
		Ctx = App.DiOrMk<Ctx>();
		Style();
		Render();
	}

	public partial class Cls{
		public const str CfgSp = nameof(CfgSp);
	}

	protected nil Style(){
		return NIL;
	}

	protected nil Render(){
		var _Item = FnSettingItem(this.ViewNavi);

		this.SetContent(new StackPanel(), S=>{
			S.A(_Item(I[K.About], new ViewAbout(), Icons.Info()));
			S.A(_Item(I[K.UIConfig], new ViewCfgUi(), Icons.SolidWindowAlt()));
			S.A(_Item(I[K.LearnWordSettings], new ViewCfgLearnWord(), Icons.Learn()));
			S.A(_Item(I[K.ServiceAndStorage], new ViewCfgServerStorage(), Icons.Server()));
			S.A(_Item(I[K.LlmDictionary], new ViewCfgLlmDictionary(), Icons.Dictionary()));
			S.A(_Item(I[K.HotkeyConfig], new ViewCfgHotkey(), Icons.KeyboardAltSharp()));
		});
		return NIL;
	}

	public static Func<
		str
		,Control
		,Icon
		,SwipeLongPressBtn
	> FnSettingItem(
		IViewNavi? ViewNavi
	){
		var Fn = (str Title, Control Target, Icon? Icon)=>{
			var R = new SwipeLongPressBtn();
			var titled = ToolView.WithTitle(Title, Target);
			titled.Styles.Add(
				Sty.OfType<TextBox>()
				.Set(x=>x.Background, new SolidColorBrush(Color.FromArgb(255, 32,32,32)))
			);
			R.HCAlign(x=>x.Left);
			R.SetContent(new StackPanel(), o=>{
				o.Orientation = Orientation.Horizontal;
				o.Spacing = UiCfg.Inst.BaseFontSize * 0.35;
				if(Icon is not null){
					o.A(Icon.ToIcon(), iconCtrl=>{
						iconCtrl.Width = UiCfg.Inst.BaseFontSize * 1.2;
						iconCtrl.Height = UiCfg.Inst.BaseFontSize * 1.2;
					});
				}
				o.A(new TextBlock(), t=>{
					t.Text = Title;
					t.FontSize = UiCfg.Inst.BaseFontSize*1.2;
				});
			});
			R.Click += (s,e)=>{
				ViewNavi?.GoTo(titled);
			};
			R.Styles.Add(
				Sty.Is<Control>()
				.BgTrnsp()
			);
			return R;
		};
		return Fn;
	}


}
