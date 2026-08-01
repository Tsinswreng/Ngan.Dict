using Avalonia.Media;
using Ngaq.Ui.Icons;

namespace Ngaq.Ui.User;

public class DfltAvatar{
	public static IImage Img{get;} = new DrawingImage{
		Drawing = new GeometryDrawing{
			Geometry = Icons.Icons.User().Data,
			Brush = Brushes.White,
		}
	};
	static DfltAvatar(){
	// 	#if DEBUG
	// 	try{
	// 		using var stream = File.OpenRead(
	// @"E:\_\視聽\圖\甘城猫猫合集\完整图包\甘城猫猫合集，密码somo(1)\甘城猫猫合集，密码somo，后缀改为zip.adb\甘城主图\近期图\043.png"
	// 		);
	// 		Img = new Bitmap(stream);
	// 	}catch{
	// 		Img=null!;
	// 	}
	// 	#else

	// 	#endif
	}
}
