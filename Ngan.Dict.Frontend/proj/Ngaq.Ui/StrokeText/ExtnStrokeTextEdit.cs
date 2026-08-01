namespace Ngaq.Ui.StrokeText;

using Avalonia;
using Avalonia.Media;
using Tsinswreng.Avln.StrokeText;

public static class ExtnStrokeTextBlock {
	extension(StrokeTextBlock z){
		public StyledProperty<string> PropText => StrokeTextBlock.TextProperty;

		public StyledProperty<IBrush> PropForeground => StrokeTextBlock.ForegroundProperty;

		public StyledProperty<IBrush> PropFill => StrokeTextBlock.FillProperty;

		public StyledProperty<IBrush> PropStroke => StrokeTextBlock.StrokeProperty;

		public StyledProperty<double> PropFontSize => StrokeTextBlock.FontSizeProperty;

		public StyledProperty<double> PropStrokeThickness => StrokeTextBlock.StrokeThicknessProperty;

		public StyledProperty<VAlign> PropVerticalContentAlignment => StrokeTextBlock.VerticalContentAlignmentProperty;

		public StyledProperty<TextWrapping> PropTextWrapping => StrokeTextBlock.TextWrappingProperty;
		public StyledProperty<FontFamily> PropFontFamily => StrokeTextBlock.FontFamilyProperty;
	}
	/* 按照 PropText_() 的命名风格，把其余属性一次性补全 */
	public static StyledProperty<string> PropText_(this StrokeTextBlock z) => StrokeTextBlock.TextProperty;

	public static StyledProperty<IBrush> PropForeground_(this StrokeTextBlock z) => StrokeTextBlock.ForegroundProperty;

	public static StyledProperty<IBrush> PropFill_(this StrokeTextBlock z) => StrokeTextBlock.FillProperty;

	public static StyledProperty<IBrush> PropStroke_(this StrokeTextBlock z) => StrokeTextBlock.StrokeProperty;

	public static StyledProperty<double> PropFontSize_(this StrokeTextBlock z) => StrokeTextBlock.FontSizeProperty;

	public static StyledProperty<double> PropStrokeThickness_(this StrokeTextBlock z) => StrokeTextBlock.StrokeThicknessProperty;

	public static StyledProperty<VAlign> PropVerticalContentAlignment_(this StrokeTextBlock z) => StrokeTextBlock.VerticalContentAlignmentProperty;

	public static StyledProperty<TextWrapping> PropTextWrapping_(this StrokeTextBlock z) => StrokeTextBlock.TextWrappingProperty;
	public static StyledProperty<FontFamily> PropFontFamily_(this StrokeTextBlock z)=> StrokeTextBlock.FontFamilyProperty;
}

