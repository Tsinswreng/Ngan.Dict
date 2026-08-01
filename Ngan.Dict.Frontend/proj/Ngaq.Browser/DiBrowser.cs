using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ngaq.Browser.Infra;
using Ngaq.Client.Mock;
using Ngaq.Core.Frontend.ImgBg;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra.Log;
using Ngaq.Core.Infra.Url;
using Ngaq.Core.Shared.Audio;
using Ngaq.Core.Shared.Dictionary.Svc;
using Ngaq.Core.Shared.StudyPlan.Svc;
using Ngaq.Core.Shared.Word.Svc;

namespace Ngaq.Browser;
public static class DiUi{
	public static IServiceCollection SetupBrowser(
		this IServiceCollection z
	){
		z.AddSingleton<I_GetBaseUrl, BaseUrl>();
		z.AddSingleton<ISvcDictionary, MockSvcDictionary>();
		z.AddSingleton<ISvcWordV2, MockSvcWordV2>();
		z.AddSingleton<ISvcWord, MockSvcWord>();
		z.AddSingleton<ISvcStudyPlan, MockSvcStudyPlan>();
		z.AddSingleton<IStudyPlanGetter, MockSvcStudyPlan>();
		z.AddSingleton<ISvcNormLang, MockSvcNormLang>();
		z.AddSingleton<ISvcTts, MockSvcTts>();
		z.AddSingleton<IAudioPlayer, MockAudioPlayer>();
		z.AddSingleton<IImgGetter, MockImgGetter>();
		z.AddSingleton<ISvcUserLang, MockSvcUserLang>();
		z.AddSingleton<IFrontendUserCtxMgr>(FrontendUserCtxMgr.Inst);
		z.AddSingleton<ILogger>(_=>AppLog.Inst);
		return z;
	}
}
