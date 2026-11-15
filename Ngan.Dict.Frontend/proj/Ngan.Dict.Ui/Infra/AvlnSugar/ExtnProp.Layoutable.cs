// 從 Tsinswreng.AvlnTools 原樣搬入本專案（0.3 系）：屬性選擇器與常用樣式糖。
// 該庫尚未遷入 TswgLib；本項目用它寫界面，故只搬這幾個純糖檔案，行為不變。
// 全域以 `global using static ...ExtnProp;` 使用，故調用點寫法照舊（txt.PropText 等）。

namespace Ngan.Dict.Ui.Infra.AvlnSugar;
using Avalonia;
using Avalonia.Controls;
using C = Avalonia.Layout.Layoutable;


public static partial class ExtnProp{
	extension(C z){
		public StyledProperty<f64> PropWidth=>C.WidthProperty;
		public StyledProperty<f64> PropHeight=>C.HeightProperty;
		public StyledProperty<f64> PropMinWidth=>C.MinWidthProperty;
		public StyledProperty<f64> PropMinHeight=>C.MinHeightProperty;
		public StyledProperty<Thickness> PropMargin=>C.MarginProperty;
		
	}
}
