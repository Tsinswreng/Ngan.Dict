namespace Ngan.Dict.Server.Http;

using Ngan.Dict.Server;
using Ngan.Dict.Server.Infra.Cfg;
using Ngan.Dict.Core;
using Tsinswreng.CsCfg;
using CfgItems = Ngan.Dict.Server.Infra.Cfg.KeysServerCfg;
using Ngan.Dict.Backend.Di;



/// 蔿入口之依賴注入

public static class DiHttpServer{
	public static IServiceCollection SetupHttpServer(
		this IServiceCollection z
		,ICfgAccessor Cfg
	){
		z.SetupCore()
		.SetupServer()
		.SetupWeb();
		return z;
	}
}
