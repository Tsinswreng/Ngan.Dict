using Foundation;
using UIKit;
using Avalonia;
using Avalonia.Controls;
using Avalonia.iOS;
using Avalonia.Media;
using Microsoft.Extensions.Logging;
using Ngaq.Core.Infra.Log;

namespace Ngaq.Ui.iOS;

// The UIApplicationDelegate for the application. This class is responsible for launching the 
// User Interface of the application, as well as listening (and optionally responding) to 
// application events from iOS.
[Register("AppDelegate")]
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public partial class AppDelegate : AvaloniaAppDelegate<App>
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
{
	protected override void OnFrameworkInitializationCompleted(){
		AppLog.UseConsoleLogger(nameof(Ngaq.Ui.iOS), LogLevel.Information);
		base.OnFrameworkInitializationCompleted();
	}

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }
}
