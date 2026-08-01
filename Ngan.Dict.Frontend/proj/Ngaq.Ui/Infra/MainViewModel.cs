using CommunityToolkit.Mvvm.ComponentModel;
using Ngaq.Ui.Infra;

namespace Ngaq.Ui.ViewModels;

public partial class MainViewModel : ViewModelBase {
	[ObservableProperty]
	private string _greeting = "Welcome to Avalonia!";
}
