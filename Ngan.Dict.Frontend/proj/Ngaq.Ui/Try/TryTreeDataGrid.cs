using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;

namespace Ngaq.Ui.Try;

public class TryTreeDataGrid {
	private static Window? _TreeDemoWnd;
	public Control? Try() {
		if (Application.Current is null) {
			return null;
		}

		var demoItems = new List<MyTreeRow>{
		new("Root A", "folder", new List<MyTreeRow>{ new("A-1", "leaf"), new("A-2", "leaf") }),
		new("Root B", "folder", new List<MyTreeRow>{ new("B-1", "leaf"), new("B-2", "leaf"), new("B-3", "leaf") }),
	};

		var source = new HierarchicalTreeDataGridSource<MyTreeRow>(demoItems) {
			Columns = {
				new HierarchicalExpanderColumn<MyTreeRow>(
					new TextColumn<MyTreeRow, string>("Name", x=>x.Name), x=>x.Children
				),
				new TextColumn<MyTreeRow, string>("Type", x=>x.Type),
			},
		};

		var treeDataGrid = new TreeDataGrid {
			Source = source,
			MinHeight = 280,
			Margin = new(12),
		};

		var host = new Grid {
			RowDefinitions = new("Auto,*"),
		};
		host.Children.Add(new TextBlock {
			Text = "TreeDataGrid Demo",
			Margin = new(12, 10, 12, 0),
			Foreground = Brushes.White,
		});
		Grid.SetRow(treeDataGrid, 1);
		host.Children.Add(treeDataGrid);

		Dispatcher.UIThread.Post(() => {
			_TreeDemoWnd ??= new Window {
				Title = "TreeDataGrid Demo",
				Width = 640,
				Height = 420,
				RequestedThemeVariant = ThemeVariant.Dark,
				Background = new SolidColorBrush(Color.FromRgb(32, 32, 32)),
			};
			_TreeDemoWnd.Content = host;
			_TreeDemoWnd.Show();
		}, DispatcherPriority.Background);

		return treeDataGrid;
	}

	private sealed class MyTreeRow {
		public string Name { get; }
		public string Type { get; }
		public IReadOnlyList<MyTreeRow> Children { get; }

		public MyTreeRow(string name, string type, IReadOnlyList<MyTreeRow>? children = null) {
			Name = name;
			Type = type;
			Children = children ?? [];
		}
	}

}
