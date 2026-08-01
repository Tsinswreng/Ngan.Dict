namespace Ngaq.Ui.Tools;
using Avalonia.Controls;
using Avalonia.Layout;
using Tsinswreng.AvlnTools.Dsl;

public class HoriCloseCtrls:ContentControl{
	public static HoriCloseCtrls Mk(params Control[] Ctrls){
		var z = new HoriCloseCtrls();
		z.SetContent(new StackPanel(), Sp=>{
			Sp.Orientation = Orientation.Horizontal;
			foreach(Control c in Ctrls){
				Sp.A(c);
			}
		});
		return z;
	}
}
