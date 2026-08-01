namespace Ngaq.Client;

using Microsoft.Extensions.DependencyInjection;
using Ngaq.Client.Svc;
using Ngaq.Client.Word.Svc;
using Ngaq.Core.Shared.User.Svc;


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
