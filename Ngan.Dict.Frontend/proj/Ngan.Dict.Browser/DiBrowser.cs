using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ngan.Dict.Browser.Infra;
using Ngan.Dict.Client.Mock;
using Ngan.Dict.Core.Frontend.ImgBg;
using Ngan.Dict.Core.Frontend.User;
using Ngan.Dict.Core.Infra.Log;
using Ngan.Dict.Core.Infra.Url;
using Ngan.Dict.Core.Shared.Audio;
using Ngan.Dict.Core.Shared.Dictionary.Svc;
using Ngan.Dict.Core.Shared.StudyPlan.Svc;
using Ngan.Dict.Core.Shared.Word.Svc;

namespace Ngan.Dict.Browser;
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
