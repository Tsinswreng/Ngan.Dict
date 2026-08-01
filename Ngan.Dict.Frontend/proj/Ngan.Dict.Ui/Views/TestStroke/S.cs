#if false

using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;

public class StrokeTextControl : Control {
	// 要顯示的文字
	public string Text { get; set; } = "描邊文字";

	// 內部填色
	public IBrush Fill { get; set; } = Brushes.Yellow;

	// 外框顏色
	public IBrush Stroke { get; set; } = Brushes.White;

	// 外框粗細
	public double StrokeThickness { get; set; } = 3;

	// 字體
	public Typeface Typeface { get; set; } =
		new Typeface("Microsoft YaHei", FontStyle.Normal, FontWeight.Normal);

	public override void Render(DrawingContext dc) {
		if (string.IsNullOrEmpty(Text)) return;

		// 1. 建立 FormattedText
		var ft = CreateFormattedText(Text, Typeface, 96); // 96 是 dpi，可換成實際值

		// 2. 轉成 Geometry（路徑）
		var textGeom = ft.BuildGeometry(new Point(10, 10)); // 左上角偏移 10,10

		// 3. 先畫外框
		var pen = new Pen(Stroke, StrokeThickness);
		dc.DrawGeometry(null, pen, textGeom);

		// 4. 再畫內部填色
		dc.DrawGeometry(Fill, null, textGeom);
	}

	// 輔助：建立 FormattedText
	private static FormattedText CreateFormattedText(
		string text,
		Typeface typeface,
		double dpi) {
		return new FormattedText(
			text,
			CultureInfo.CurrentCulture,
			FlowDirection.LeftToRight,
			typeface,
			72,              // 字號
			Brushes.Black   // 這裡顏色無所謂，等等會被取代
		);
	}
}

#endif
