using Microsoft.Extensions.DependencyInjection;
using Ngaq.Core.Shared.User.UserCtx;
using Ngaq.Core.Infra.Cfg;
using Ngaq.Core.Tools.Json;
using Ngaq.Core.Word.Svc;
using Tsinswreng.CsCfg;
using Ngaq.Core.Shared.Word;
using Microsoft.Extensions.Logging;
using Ngaq.Core.Shared.Word.WeightAlgo;
namespace Ngaq.Core;
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
