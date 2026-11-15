using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ngan.Dict.Core.Infra.Log;
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
		// 1: 日誌門面。注入場合拿到的就是全局那一個 AppLog.Inst，
		//    與靜態場合共用同一條出口（各宿主在入口處指定內層 logger）。
		z.AddSingleton<ILogger>(_ => AppLog.Inst);

		// 2: 配置：同一個實例註冊成四種寬度，注入方只拿自己需要的權限。
		z.AddSingleton(AppCfg.Inst.GetType(), AppCfg.Inst);
		z.AddSingleton<LayeredCfgAccessor>(AppCfg.Inst);
		z.AddSingleton<ICfgReader>(AppCfg.Inst);
		z.AddSingleton<ICfgAccessor>(AppCfg.Inst);
		z.AddSingleton<ILayeredCfgAccessor>(AppCfg.Inst);
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