using Microsoft.Extensions.DependencyInjection;
using Ngan.Dict.Core.Shared.User.UserCtx;
using Ngan.Dict.Core.Infra.Cfg;
using Ngan.Dict.Core.Tools.Json;
using Ngan.Dict.Core.Word.Svc;
using Tsinswreng.CsCfg;
using Ngan.Dict.Core.Shared.Word;
using Microsoft.Extensions.Logging;
using Ngan.Dict.Core.Shared.Word.WeightAlgo;
namespace Ngan.Dict.Core;
public static class DiCore{
	public static IServiceCollection SetupCore(this IServiceCollection z){
		z.AddSingleton<ICfgAccessor>(AppCfg.Inst);
		z.AddTransient<MgrLearn, MgrLearn>();
		z.AddScoped<IWeightCalctr, SvcWeight>();
		z.AddTransient<IJsonSerializer, AppJsonSerializer>();

		//browser 不支持
		return z;
	}
}
