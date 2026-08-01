namespace Ngaq.Ui.Views.Word.Pronunciation;

using Avalonia.Controls;
using Ngaq.Ui;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.Ctrls;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsI18n;
using Ctx = VmPronunciation;
public partial class ViewPronunciation
	:AppViewBase<Ctx>
{

	public ViewPronunciation(){
		Ctx = App.DiOrMk<Ctx>();
		Style();
		Render();
	}

	public partial class Cls{

	}


	protected nil Style(){
		return NIL;
	}

	SelectableTextBlock Txt(){
		return new SelectableTextBlock();
	}
	GridStack Root = new(IsRow: false);
	protected nil Render(){
		this.Content = Root.Grid;
		Root.Grid.SetColDefs([
			new(1, GUT.Auto),
			new(1, GUT.Auto),
		]);
		Root.A(new OpBtn(), o=>{
			var icon = Icons.VolHigh().ToIcon();
			icon.Height = icon.Width = UiCfg.Inst.BaseFontSize*0.8;
			o.BtnContent = icon;
			o.SetExe(Ct=>Ctx?.Play(Ct));
		})
		.A(Txt(), o=>{
			o.CBind<Ctx>(o.PropText,x=>x.Text);
		})
		;
		return NIL;
	}
}



