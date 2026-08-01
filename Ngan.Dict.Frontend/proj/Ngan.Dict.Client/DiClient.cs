namespace Ngan.Dict.Client;

using Microsoft.Extensions.DependencyInjection;
using Ngan.Dict.Client.Svc;
using Ngan.Dict.Client.Word.Svc;
using Ngan.Dict.Core.Shared.User.Svc;


public static class DiClient{
	public static IServiceCollection SetupClient(this IServiceCollection z){
		z.AddScoped<ISvcUser, ClientUser>();
		z.AddSingleton<IHttpCaller, HttpCaller>();
		z.AddScoped<HttpClient>();
		z.AddScoped<ClientWordSync>();
		z.AddScoped<ClientWordSyncV2>();
#if false //TODO 移至DiBrowser
		z.AddScoped<ISvcWord, ClientWord>();
		z.AddScoped<IUserCtxMgr, UserCtxMgr>();
		z.AddSingleton<IImgGetter>((s)=>null!);
#endif
		return z;
	}
}
