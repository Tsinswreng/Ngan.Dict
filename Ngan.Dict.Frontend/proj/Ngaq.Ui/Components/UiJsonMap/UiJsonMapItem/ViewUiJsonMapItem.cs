namespace Ngaq.Ui.Components.KvMap;

using Avalonia.Controls;
using Avalonia.Media;
using Ngaq.Core.Tools.JsonMap;
using Ngaq.Ui;
using Ngaq.Ui.Infra;
using ScottPlot.Plottables;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsI18n;
using Ctx = VmJsonMapItem;
public partial class ViewJsonMapItem
	:AppViewBase<Ctx>
{
	public ViewJsonMapItem(){
		Ctx = App.DiOrMk<Ctx>();
		Style();
		Render();
	}

	public partial class Cls{

	}


	protected nil Style(){
		return NIL;
	}

	TextBlock Txt(){
		return new();
	}

	GridStack Root = new(IsRow: true);
	protected nil Render(){
		this.Content = Root.Grid;
		Root.SetRowDefs([
			new(1, GUT.Auto),
			new(1, GUT.Auto),
			new(1, GUT.Auto),
		]);
		Root
		.A(Txt(), o=>{
			o.CBind<Ctx>(o.PropText,x=>x.DisplayName);
		})
		.A(Txt(), o=>{
			o.CBind<Ctx>(o.PropText,x=>x.Descr);
			o.FontSize = UiCfg.Inst.BaseFontSize * 0.8;
			o.Foreground = Brushes.LightGray;
			//TODO 處理文字過多
		})
		.A(new TextBox(), o=>{
			o.CBind<Ctx>(o.PropText,x=>x.RawInput);

		})
		//TODO 增一詳情頁、即把只一項置全屏㕥編輯、中ʸ用大ʹ可換行ʹTextBox作輸入框?


		;
		return NIL;
	}
}

