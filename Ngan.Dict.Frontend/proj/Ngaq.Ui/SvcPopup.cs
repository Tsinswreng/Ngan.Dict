namespace Ngaq.Ui;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Ngaq.Ui.Tests;
using Ngaq.Ui.Views;

public class SvcPopup {
	private Panel Overlay{get{
		return FnMkOverlay?.Invoke()??new Grid();
	}}
	public Func<Panel> FnMkOverlay{get;set;}
	public Panel? RootPanel{get;set;}
	public Stack<Control> Popups = new Stack<Control>();
	public SvcPopup(Panel RootPanel){
		this.RootPanel = RootPanel;
		if(FnMkOverlay is null){
			FnMkOverlay = ()=> new Grid {
				Background = Brushes.Black,
				Opacity = 0.5,
				HorizontalAlignment = HAlign.Stretch,
				VerticalAlignment = VAlign.Stretch
			};
		}
	}


	/// Ctrl勿多次傳同一實例、除非先關彈窗

	/// <param name="Ctrl"></param>
	public void ShowPopup(Control Ctrl){
		try{
			if(RootPanel is null){
				return;
			}
			Popups.Push(Ctrl);
			RootPanel.Children.Add(Overlay);
			RootPanel.Children.Add(Ctrl);
		}catch(Exception e){//The control Grid already has a visual parent Grid while trying to add it as a child of Grid.
			System.Console.WriteLine(e);//t
		}

	}

	// public void ShowPopupMsg(Window parentWindow, string message) {

	// 	// 创建弹窗内容
	// 	var popup = new Border {
	// 		Background = Brushes.White,
	// 		BorderBrush = Brushes.Gray,
	// 		BorderThickness = new(2),
	// 		CornerRadius = new CornerRadius(8),
	// 		Padding = new(20),
	// 		Child = new StackPanel {
	// 			Children =
	// 			{
	// 				new TextBlock { Text = message, FontSize = 16 },
	// 				new Button
	// 				{
	// 					Content = "关闭",
	// 					Margin = new(0, 10, 0, 0),
	// 					HorizontalAlignment = HAlign.Right
	// 				}
	// 			}
	// 		},
	// 		HorizontalAlignment = HAlign.Center,
	// 		VerticalAlignment = VAlign.Center
	// 	};

	// 	// 关闭按钮事件
	// 	var closeButton = (Button)((StackPanel)popup.Child).Children[1];
	// 	closeButton.Click += (_, __) => ClosePopup();

	// 	// 添加到窗口
	// 	var mainView = parentWindow.Content as MainView;
	// 	if(mainView is not null){
	// 		var root = mainView.Root;
	// 		root.Children.Add(_overlay);
	// 		root.Children.Add(popup);
	// 	}
	// }

	public void ClosePopup() {
		// if (_overlay?.Parent is Panel root) {
		// 	root.Children.Remove(_overlay);
		// 	// 移除弹窗控件（假设它是最后一个添加的）
		// 	if (root.Children.Count > 0 && root.Children[^1] is Border)
		// 		root.Children.RemoveAt(root.Children.Count - 1);
		// }
		if(RootPanel is null){
			return;
		}
		if(RootPanel.Children.Count >= 3){
			RootPanel.Children.RemoveAt(RootPanel.Children.Count-1);
			RootPanel.Children.RemoveAt(RootPanel.Children.Count-1);
			Popups.Pop();
		}
		// RootPanel.Children.Remove(RootPanel.Children[^1]);
		// RootPanel.Children.Remove(_overlay);

	}
}
