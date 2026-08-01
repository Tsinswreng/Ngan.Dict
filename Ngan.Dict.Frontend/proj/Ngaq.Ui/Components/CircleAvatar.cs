namespace Ngaq.Ui.Components;
// CircleAvatar.cs
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

/// 圆形头像控件，纯 C#，无 XAML。
public class CircleAvatar : Control {
	/// 定义图片来源属性。
	public static readonly StyledProperty<IImage?> SourceProperty =
		AvaloniaProperty.Register<CircleAvatar, IImage?>(nameof(Source));

	/// 定义边框粗细属性。
	public static readonly StyledProperty<double> BorderThicknessProperty =
		AvaloniaProperty.Register<CircleAvatar, double>(nameof(BorderThickness), 0);

	/// 定义边框颜色属性。
	public static readonly StyledProperty<IBrush?> BorderBrushProperty =
		AvaloniaProperty.Register<CircleAvatar, IBrush?>(nameof(BorderBrush));

	public IImage? Source {
		get => GetValue(SourceProperty);
		set => SetValue(SourceProperty, value);
	}

	public double BorderThickness {
		get => GetValue(BorderThicknessProperty);
		set => SetValue(BorderThicknessProperty, value);
	}

	public IBrush? BorderBrush {
		get => GetValue(BorderBrushProperty);
		set => SetValue(BorderBrushProperty, value);
	}

	static CircleAvatar() {
		// 图片或边框变化时重新渲染
		SourceProperty.Changed.AddClassHandler<CircleAvatar>((x, _) => x.InvalidateVisual());
		BorderThicknessProperty.Changed.AddClassHandler<CircleAvatar>((x, _) => x.InvalidateVisual());
		BorderBrushProperty.Changed.AddClassHandler<CircleAvatar>((x, _) => x.InvalidateVisual());
	}

	public override void Render(DrawingContext ctx) {
		if (Source == null) return;

		var bounds = new Rect(Bounds.Size);
		var min = Math.Min(bounds.Width, bounds.Height);
		var radius = min / 2;
		var center = new Point(bounds.Width / 2, bounds.Height / 2);

		// 1. 构造圆形几何
		var circle = new EllipseGeometry {
			Center = center,
			RadiusX = radius,
			RadiusY = radius
		};

		// 2. 画边框
		if (BorderThickness > 0 && BorderBrush != null) {
			ctx.DrawGeometry(null, new Pen(BorderBrush, BorderThickness), circle);
		}

		// 3. 用 using 做圆形裁剪
		using (ctx.PushClip(new RoundedRect(new Rect(center.X - radius,
														center.Y - radius,
														radius * 2,
														radius * 2),
											radius))) {
			ctx.DrawImage(Source, new Rect(center.X - radius,
											center.Y - radius,
											radius * 2,
											radius * 2));
		}
	}

	/// 从文件快速加载头像（可选辅助方法）。
	public static Bitmap LoadFromFile(string path) {
		using var stream = System.IO.File.OpenRead(path);
		return new Bitmap(stream);
	}
}

public static class ExtnCircleAvatar{
/*
public static readonly StyledProperty<double> BorderThicknessProperty =
		AvaloniaProperty.Register<CircleAvatar, double>(nameof(BorderThickness), 0);

	/// 定义边框颜色属性。
	public static readonly StyledProperty<IBrush?> BorderBrushProperty =
		AvaloniaProperty.Register<CircleAvatar, IBrush?>(nameof(BorderBrush));
 */
	extension(CircleAvatar z){
		public StyledProperty<IImage?> PropSource => CircleAvatar.SourceProperty;
		public StyledProperty<double> PropBorderThickness => CircleAvatar.BorderThicknessProperty;
		public StyledProperty<IBrush?> PropBorderBrush => CircleAvatar.BorderBrushProperty;
	}
}
