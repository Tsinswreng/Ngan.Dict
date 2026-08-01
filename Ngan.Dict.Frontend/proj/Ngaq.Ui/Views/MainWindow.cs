using Avalonia.Controls;

namespace Ngaq.Ui.Views;

public partial class MainWindow : Window {
	public MainWindow() {
		//InitializeComponent();
		//Content = new MainView();
		Content = MainView.Inst;
	}
}
