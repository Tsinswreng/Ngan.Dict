namespace Ngan.Dict.Backend.Di;

using Microsoft.Extensions.DependencyInjection;
using Ngan.Dict.Core.Frontend.ImgBg;
using Ngan.Dict.Core.Frontend.User;
using Ngan.Dict.Core.Frontend.User.Svc;
using Ngan.Dict.Core.Infra.Url;
using Ngan.Dict.Core.Shared.Audio;
using Ngan.Dict.Core.Shared.Dictionary.Svc;
using Ngan.Dict.Core.Shared.Encryption.Svc;
using Ngan.Dict.Core.Shared.User.UserCtx;
using Ngan.Dict.Backend.Domains.Dictionary.Svc;
using Ngan.Dict.Backend.Frontend.User.Svc;
using Ngan.Dict.Backend.ImplFrontend;
using Ngan.Dict.Backend.Infra;
using Tsinswreng.CsCore;
using System.IO.Abstractions;

[Doc("僅限本地(客戶端)後端使用、勿用于Server")]
public static class DiLocalFrontend{
	public static IServiceCollection SetupLocalFrontend(this IServiceCollection z){
		//z.AddScoped<ISvcSecretKv, SvcSecretKv>();
		z.AddScoped<ISvcTokenStorage, SvcTokenStorage>();
		z.AddSingleton<IFrontendUserCtxMgr>(FrontendUserCtxMgr.Inst);
		z.AddSingleton<IUserCtx>(FrontendUserCtxMgr.Inst.GetUserCtx());
		z.AddSingleton<I_GetBaseUrl, BaseUrl>();
		
		z.AddSingleton<OnlineAudio>();
		z.AddScoped<ISvcTts, Gtts>();
		z.AddScoped<IImgGetter, SvcImg>();
		z.AddSingleton<IFileSystem, FileSystem>();

		return z;
	}
}
