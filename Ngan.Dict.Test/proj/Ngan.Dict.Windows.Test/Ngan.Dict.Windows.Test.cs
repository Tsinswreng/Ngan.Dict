#if false
測試方法:
+ 直接在Ngan.Dict.Windows.Test中執行 dotnet run (非AOT編譯)
+ 執行以下腳本 在AOT還境下測試
```bash
dotnet publish -c Release -r win-x64
./bin/Release/net10.0/win-x64/publish/Ngan.Dict.Windows.Test.exe
```
#endif

using Microsoft.Extensions.DependencyInjection;
using Ngan.Dict.Core;
using Ngan.Dict.Backend;
using Ngan.Dict.Backend.Di;
using Tsinswreng.CsTreeTest;

namespace Ngan.Dict.Windows.Test;

internal class Program{
	public static IServiceCollection SvcColct = new ServiceCollection();
	public static IServiceProvider SvcProvdr => Ngan.Dict.Core.Di.SvcProvider;
	public static async Task Main(string[] args){
		SvcColct
			.SetupCore()
			.SetupLocal()
			.SetupLocalFrontend()
		;

		var mgr = WindowsTestMgr.Inst;
		_ = mgr.InitSvc(SvcColct, sc => {
			var sp = sc.BuildServiceProvider();
			Ngan.Dict.Core.Di.SvcProvider = sp;
			return sp;
		});

		AppIniter.Inst.Sp = SvcProvdr;
		_ = AppIniter.Inst.Init(default).Result;
		ITestExecutor executor = new TreeTestExecutor();
		await executor.RunEtPrint(mgr.TestNode);
	}
}
