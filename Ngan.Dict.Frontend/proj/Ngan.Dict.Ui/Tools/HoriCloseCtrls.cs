namespace Ngan.Dict.Ui.Tools;
using Avalonia.Controls;
using Avalonia.Layout;

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
