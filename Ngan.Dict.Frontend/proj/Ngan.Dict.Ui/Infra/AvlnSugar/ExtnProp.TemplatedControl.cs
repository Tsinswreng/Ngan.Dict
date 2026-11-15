// 從 Tsinswreng.AvlnTools 原樣搬入本專案（0.3 系）：屬性選擇器與常用樣式糖。
// 該庫尚未遷入 TswgLib；本項目用它寫界面，故只搬這幾個純糖檔案，行為不變。
// 全域以 `global using static ...ExtnProp;` 使用，故調用點寫法照舊（txt.PropText 等）。

namespace Ngan.Dict.Ui.Infra.AvlnSugar;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using C = Avalonia.Controls.Primitives.TemplatedControl;


public static partial class ExtnProp{
	extension(C c){
		public StyledProperty<IBrush?> PropBackground=>C.BackgroundProperty;
		public StyledProperty<IBrush?> PropForeground=>C.ForegroundProperty;
		public StyledProperty<f64> PropWidth => C.WidthProperty;
		public StyledProperty<f64> PropMinWidth => C.MinWidthProperty;
		public StyledProperty<f64> PropMaxWidth => C.MaxWidthProperty;

		public StyledProperty<f64> PropHeight => C.HeightProperty;
		public StyledProperty<f64> PropMinHeight => C.MinHeightProperty;
		public StyledProperty<f64> PropMaxHeight => C.MaxHeightProperty;

	}
	public static StyledProperty<IBrush?> PropBackground_(
		this C z
	){
		return C.BackgroundProperty;
	}
	public static StyledProperty<IBrush?> PropForeground_(
		this C z
	){
		return C.ForegroundProperty;
	}

	public static StyledProperty<f64> PropWidth_(
		this C z
	){
		return C.WidthProperty;
	}

	public static StyledProperty<f64> PropMinWidth_(
		this C z
	){
		return C.MinWidthProperty;
	}

	public static StyledProperty<f64> PropMaxWidth_(
		this C z
	){
		return C.MaxWidthProperty;
	}


	public static StyledProperty<f64> PropHeight_(
		this C z
	){
		return C.HeightProperty;
	}

	public static StyledProperty<f64> PropMinHeight_(
		this C z
	){
		return C.MinHeightProperty;
	}

	public static StyledProperty<f64> PropMaxHeight_(
		this C z
	){
		return C.MaxHeightProperty;
	}
}
