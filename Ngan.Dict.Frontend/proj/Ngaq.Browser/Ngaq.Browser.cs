using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using Microsoft.Extensions.DependencyInjection;
using Ngaq.Core;
using Ngaq.Core.Infra.Log;
using Ngaq.Ui;
using Ngaq.Client;
namespace Ngaq.Browser;
internal sealed partial class Program {
	private static async Task Main(string[] args){
		var I = 0;
		try{
			AppLog.UseLogger(new BrowserLogger());
			var svc = new ServiceCollection();
			svc
				.SetupCore()
				.SetupUi()
				.SetupBrowser()
				.SetupBrowser()
				//.SetUpLocal()//TODO 改成按需API調用
				.SetupClient()
			;
			var servicesProvider = svc.BuildServiceProvider();
			//從此後即報錯
			await BuildAvaloniaApp()
			.AfterSetup(e=>{
				App.SetSvcProvider(servicesProvider);
			})
			.WithInterFont()
			.StartBrowserAppAsync("out");//avalonia窗口 ʃʰ掛載ʹhtml元素
			return;
			}
		catch (System.Exception e){
			System.Console.WriteLine(e);
		}
		return;
	}

	public static AppBuilder BuildAvaloniaApp()
		=> AppBuilder.Configure<App>();
}
