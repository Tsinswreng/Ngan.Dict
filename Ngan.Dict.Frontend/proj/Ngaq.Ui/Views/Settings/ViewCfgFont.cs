namespace Ngaq.Ui.Views.Settings;

using Avalonia.Controls;
using Ngaq.Ui.Infra;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Controls;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Ctx = VmCfgFont;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;
public partial class ViewCfgFont
	:AppViewBase<Ctx>
{

	public ViewCfgFont(){
		Ctx = Ctx.Mk();
		Style();
		Render();
	}

	public partial class Cls{

	}
	public GridStack Root = new(IsRow: true);

	protected nil Style(){
		this.Classes.A(App.Cls.ViewPadding);
		return NIL;
	}

	protected Control InputFontSize(){
		var R = new GridStack(IsRow:false);
		R.SetColDefs([
			new(1, GUT.Star),
			new(1, GUT.Auto),
		]);
		R.A(new TextBox(), o=>{
			o.CBind<Ctx>(o.PropText,x=>x.InputFontSize);
		});
		return R.Grid;
	}

	protected nil Render(){
		this.SetContent(Root.Grid, o=>{
			o.SetRowDefs([
				new(1, GUT.Auto),
				new(1, GUT.Auto),
				new(1, GUT.Auto),
				new(1, GUT.Auto),
				new(1, GUT.Star),
			]);
		});
		Root.A(new TextBlock(), o=>{
			o.Text = I[K.BaseFontSize];
		});
		Root.A(InputFontSize())
		.A(new SwipeLongPressBtn(), o=>{
			o.Content = I[K.Try];
			o.Click += (s,e)=>{
				Ctx?.TryNeoFontSize();
			};
		})
		.A(new SwipeLongPressBtn(), o=>{
			o.Content = I[K.Apply];
			o.Click += (s,e)=>{
				Ctx?.ApplyNeoFontSize();
			};
		})
		.A(new TextBlock(), o=>{
			o.Text = SampleText;
			o.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
			o.CBind<Ctx>(o.PropFontSize,x=>x.FontSize);
		});

		return NIL;
	}

str SampleText =>
	I[K.FontChangeRelaunchNotice_] +
"""
!"#$%&'()*+,-./0123456789:;<=>?@
ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`
abcdefghijklmnopqrstuvwxyz{|}~
一去二三里 煙村四五家
亭臺六七座 八九十枝花
""";

}
