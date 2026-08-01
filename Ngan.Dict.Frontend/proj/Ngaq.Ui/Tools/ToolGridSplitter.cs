using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;

namespace Ngaq.Ui.Tools;

public static class ToolGridSplitter{
	extension<T>(T z)
		where T: GridSplitter
	{
		public T GrayBarWith3Dots(){
			// 1. 灰带
			var bar = new Border{
				Height = 6,
				Background = Brushes.Gray,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Center
			};

			// 2. 3 个白点
			const double d = 4;          // 直径
			const double g = 4;          // 间隔
			var dots = new StackPanel{
				Orientation = Orientation.Horizontal,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Spacing = g,
				Children ={
					new Border{ Width = d, Height = d, CornerRadius = new CornerRadius(d/2), Background = Brushes.White },
					new Border{ Width = d, Height = d, CornerRadius = new CornerRadius(d/2), Background = Brushes.White },
					new Border{ Width = d, Height = d, CornerRadius = new CornerRadius(d/2), Background = Brushes.White }
				}
			};

			// 3. 把灰带和点合成一个 Grid
			var root = new Grid();
			root.Children.Add(bar);
			root.Children.Add(dots);

			// 4. 用 ControlTemplate 替换整个视觉效果
			var tpl = new ControlTemplate{
				TargetType = typeof(GridSplitter),
				Content = new Func<IServiceProvider, TemplateResult<Control>>(_ =>new TemplateResult<Control>(root, null))
			};
			z.Template = tpl;
			return z;
		}
	}
}
