using CommunityToolkit.Mvvm.ComponentModel;
using Ngan.Dict.Ui.Infra;

namespace Ngan.Dict.Ui.ViewModels;

public partial class MainViewModel : AppVmBase {
	[ObservableProperty]
	private string _greeting = "Welcome to Avalonia!";
}
