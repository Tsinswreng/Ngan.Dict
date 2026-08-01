namespace Ngaq.Ui.Views.About;

using Avalonia.Controls;
using Ngaq.Core.Infra;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Views.Settings;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsI18n;

//using Ctx = VmXxx;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;
public partial class ViewAbout
	:AppViewBase
{

	// public Ctx? Ctx{
	// 	get{return DataContext as Ctx;}
	// 	set{DataContext = value;}
	// }
	public ViewAbout(){
		//Ctx = Ctx.Mk();
		Style();
		Render();
	}

	public partial class Cls{

	}

	protected nil Style(){
		this.Classes.A(App.Cls.ViewPadding);
		return NIL;
	}
	GridStack Root = new (IsRow: true);

	protected nil Render(){
		this.SetContent(Root.Grid, o=>{
			o.SetRowDefs([
				new(1, GUT.Auto),
			]);
		});
		Root.A(new StackPanel(), sp=>{
			sp.Classes.A(App.Cls.SpacedStackPanel);
			//sp.Classes.A(ViewSettings.Cls.CfgSp);
			sp.A(new SelectableTextBlock(), o=>{
				o.Text = I[K.AppVersion];
			})
			.A(new SelectableTextBlock(), o=>{
				o.Text = AppVer.Inst.Ver+"";
			})

			.A(new SelectableTextBlock(), o=>{
				o.Text = I[K.Website];
			})
			.A(new HyperlinkButton(), o=>{
				var url = "https://github.com/Tsinswreng/CsNgaq";
				o.NavigateUri = new Uri(url);
				o.SetContent(new SelectableTextBlock(), o=>{
					o.Text = url;
				});
			})
			.A(new HyperlinkButton(), o=>{
				var url = WeightAlgorithmPluginDocUrl;
				o.NavigateUri = new Uri(url);
				o.SetContent(I[K.WeightAlgorithmPluginDoc], o=>{

				});
			})
			;
		});
		return NIL;
	}
	public const str WeightAlgorithmPluginDocUrl = "https://github.com/Tsinswreng/CsNgaq/blob/master/Doc/Prod/en/JsWeight.md";


}
