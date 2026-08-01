namespace Ngaq.Ui.Tools;
using Avalonia.Controls;
using Avalonia.Media;
using Ngaq.Ui.Infra.Ctrls;
using Tsinswreng.AvlnTools.Controls;
using Tsinswreng.AvlnTools.Dsl;



public partial class ToolView{
	public static Control WithTitle(str Title, Control Target){
		var titled = new ViewTitle();
		titled.Body.Content = Target;
		//titled.Title.Content = new TextBlock(){Text = Title};
		titled.Title.SetContent(new TextBlock(), o=>{
			o.Text = Title;
			o.VerticalAlignment = VAlign.Center;
			o.HorizontalAlignment = HAlign.Center;
			o.FontSize = UiCfg.Inst.BaseFontSize*1.2;
		});
		titled.BdrTitle.Background = new SolidColorBrush(Color.FromRgb(32, 32, 32));

		return titled;
	}


}
