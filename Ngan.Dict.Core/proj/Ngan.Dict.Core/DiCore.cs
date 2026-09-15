using Microsoft.Extensions.DependencyInjection;
using Ngan.Dict.Core.Shared.User.UserCtx;
using Ngan.Dict.Core.Infra;
using Ngan.Dict.Core.Infra.Cfg;
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

		// 公共庫新接口：直接複用 AppJsonCtx.Inst.Options 作默認 options——
		// 它已含源生成 resolver(AppJsonCtx.Default)、強類型轉換器、enum 字符串轉換器、
		// 不轉義編碼與 PascalCase 命名策略,與舊 JSON.Stringify 行為完全一致。
		z.AddSingleton<Tsinswreng.CsJson.IJsonSerializer>(_ => new Tsinswreng.CsJson.JsonSerializer(AppJsonCtx.Inst.Options));

		//browser 不支持
		return z;
	}
}