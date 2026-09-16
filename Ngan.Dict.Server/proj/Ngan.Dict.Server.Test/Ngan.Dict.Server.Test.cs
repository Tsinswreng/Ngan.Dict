using Microsoft.Extensions.DependencyInjection;
using Ngan.Dict.Core;
using Ngan.Dict.Core.Infra.Url;
using Ngan.Dict.Backend.Di;
using Ngan.Dict.Server;
using Ngan.Dict.Server.Http;
using Ngan.Dict.Server.Infra.Cfg;
using Ngan.Dict.Core.Infra.Log;
using Microsoft.Extensions.Logging;
using Tsinswreng.CsTreeTest;

namespace Ngan.Dict.Server.Test;

internal class Program{
	public static IServiceCollection SvcColct = new ServiceCollection();
	public static IServiceProvider SvcProvdr => Ngan.Dict.Core.Di.SvcProvider;

	public static async Task Main(string[] args){
		AppLog.UseConsoleLogger(nameof(Ngan.Dict.Server.Test), LogLevel.Information);
		BaseDirMgr.Inst._BaseDir = Directory.GetCurrentDirectory();

		var serverCfgPath = TestCfg.GetServerCfgPath(args);
		ServerCfg.Inst.LoadFromArgs([serverCfgPath]);

		SvcColct
			.AddLogging()
			.SetupCore()
			.SetupServer()
			.SetupCommonBackend()
		;

		var mgr = ServerTestMgr.Inst;
		_ = mgr.InitSvc(SvcColct, sc => {
			var sp = sc.BuildServiceProvider();
			Ngan.Dict.Core.Di.SvcProvider = sp;
			return sp;
		});

		await ServerTestBootstrap.EnsureServerDbReady(SvcProvdr, default);

		ITestExecutor executor = new TreeTestExecutor();
		await executor.RunEtPrint(
			mgr.TestNode
			// ,Opt: new OptTestExecutor{
			// 	IsParallel = false,
			// 	MaxDegreeOfParallelism = 1,
			// }
		);
		throw new Exception("Test AOT Exeption Output");
	}
}
