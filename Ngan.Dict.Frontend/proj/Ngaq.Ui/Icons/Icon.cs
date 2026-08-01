namespace Ngaq.Ui.Icons;

using Avalonia.Controls;
using Avalonia.Media;
using Ngaq.Ui.Tools;
using Tsinswreng.AvlnTools.Dsl;
using SPath = Avalonia.Controls.Shapes.Path;
public class Icon:SPath{
	//AI曰SVG 的原生坐标系 Y 轴向下，而 Avalonia 的 Geometry 坐标系 Y 轴向上
	public static Icon FromSvg(Svg Svg){
		var R = new Icon();
		R.Data = Geometry.Parse(Svg);
		R.Height = R.Width = UiCfg.Inst.BaseFontSize;
		R.HAlign(x=>x.Center);
		R.VAlign(x=>x.Center);
		R.Fill = Brushes.White;
		R.Stretch = Stretch.Uniform;   // 让图形自动缩放到控件大小
		return R;
	}
	public static Icon Invert(Svg Svg){
		var R = FromSvg(Svg);
		R.RenderTransform = new ScaleTransform{ScaleY=-1};
		return R;
	}
	public static implicit operator Icon(str s){
		Svg svg = s;
		return svg.ToIcon();
	}
}

public static class ExtnIcon{
	extension(Icon z){
		public HoriCloseCtrls WithText(str Text){
			return HoriCloseCtrls.Mk(z, new TextBlock{Text=" "+Text});
		}
	}
	extension(Svg z){
		public HoriCloseCtrls WithText(str Text){
			return HoriCloseCtrls.Mk(z.ToIcon(), new TextBlock{Text=" "+Text});
		}
	}

}
