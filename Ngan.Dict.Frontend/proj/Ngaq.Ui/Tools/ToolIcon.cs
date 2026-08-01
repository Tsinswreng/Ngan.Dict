namespace Ngaq.Ui.Tools;

using Avalonia.Controls;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;

public class ToolIcon{
	public static Panel IconWithTitle(
		Control Icon, str Title
	){
		var R = new GridStack(IsRow: false);
		R.ColDefs.AddRange([
			new(1, GUT.Auto),
			new(UiCfg.Inst.BaseFontSize, GUT.Pixel),
			new(1, GUT.Auto),
		]);
		R.A(Icon);
		R.Add();
		R.A(new TextBlock(), t=>{t.Text = Title;});
		return R.Grid;
	}
}
