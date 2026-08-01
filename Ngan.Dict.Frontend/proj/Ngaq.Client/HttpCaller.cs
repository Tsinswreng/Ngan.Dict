using System.Text;
using System.Net.Http.Headers;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra.Url;
using Ngaq.Core.Tools.Json;
using Tsinswreng.CsTools;
using Ngaq.Core.Shared.User.Models.Req;
using Ngaq.Core.Shared.User.Models.Resp;
using Ngaq.Core.Infra;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Tools;
using Tsinswreng.CsErr;
namespace Ngaq.Client;

public class HttpCaller:IHttpCaller{
	//IHttpClientFactory

	IJsonSerializer JsonS;
	HttpClient HttpClient;
	I_GetBaseUrl BaseUrlGetter;
	IFrontendUserCtxMgr UserCtxMgr;
	public HttpCaller(
		IJsonSerializer JsonS
		,HttpClient HttpClient
		,I_GetBaseUrl BaseUrlGetter
		,IFrontendUserCtxMgr UserCtxMgr
	){
		this.HttpClient = HttpClient;
		this.JsonS = JsonS;
		this.BaseUrlGetter = BaseUrlGetter;
		this.UserCtxMgr = UserCtxMgr;
	}

	public async Task<HttpResponseMessage> SendWithRetry<TContent>(
		string RelaUrl,
		TContent Content,
		Func<TContent, HttpContent> MkContent,
		CT Ct
	){
		using var dl = DisposableList.Mk();

		var url = ToolPath.SlashTrimEtJoin([BaseUrlGetter.GetBaseUrl(), RelaUrl]);
		HttpResponseMessage resp = null!;

		for (var i = 0; i < 2; i++){
			var userCtx = UserCtxMgr.GetUserCtx();
			var token   = userCtx?.AccessToken;
			var clientId = userCtx?.ClientId;

			var httpContent = MkContent(Content);
			dl.Add(httpContent);
			var reqMsg = new HttpRequestMessage(HttpMethod.Post, url) { Content = httpContent };
			dl.Add(reqMsg);


			reqMsg.Headers.Add("App-Version", AppVer.Inst.Ver+"");
			if (!str.IsNullOrEmpty(token)){
				reqMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			}
			if(!clientId.IsNullOrDefault()){
				reqMsg.Headers.Add("X-Client-Id", clientId+"");
			}

			try{
				resp = await HttpClient.SendAsync(reqMsg, Ct);
			}catch(HttpRequestException ex){
				var e = KeysErr.Common.NetWorkErr.ToErr();
				e.AddErr(ex);
				throw e;
			}
			if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized){
				dl.Add(resp);
				var refresh = await RefreshBothToken(Ct);
				// TODO: 处理 refresh 失败
				continue;
			}
			break;
		}

		return resp;
	}

	/// 發送 POST 請求（最多重試一次）；每次嘗試都由異步工廠重新創建 HttpContent。
	/// 這可避免不可 seek 的流在 401 重試時被重放為空內容。
	/// <typeparam name="TContent">內容工廠所需的狀態類型。</typeparam>
	/// <param name="RelaUrl">相對 URL。</param>
	/// <param name="Content">內容工廠狀態。</param>
	/// <param name="MkContentAsy">每次嘗試都會調用一次，用於創建全新 HttpContent。</param>
	/// <param name="Ct">取消令牌。</param>
	/// <returns>最終響應（401 之外的狀態不在此方法內拋錯）。</returns>
	public async Task<HttpResponseMessage> SendWithRetry<TContent>(
		string RelaUrl,
		TContent Content,
		Func<TContent, CT, Task<HttpContent>> MkContentAsy,
		CT Ct
	){
		using var dl = DisposableList.Mk();

		var url = ToolPath.SlashTrimEtJoin([BaseUrlGetter.GetBaseUrl(), RelaUrl]);
		HttpResponseMessage resp = null!;

		for(var i = 0; i < 2; i++){
			var userCtx = UserCtxMgr.GetUserCtx();
			var token = userCtx?.AccessToken;
			var clientId = userCtx?.ClientId;

			// 每輪都新建 content，確保流式上傳可重試而不必整包進內存。
			var httpContent = await MkContentAsy(Content, Ct);
			dl.Add(httpContent);
			var reqMsg = new HttpRequestMessage(HttpMethod.Post, url){ Content = httpContent };
			dl.Add(reqMsg);

			reqMsg.Headers.Add("App-Version", AppVer.Inst.Ver+"");
			if(!str.IsNullOrEmpty(token)){
				reqMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			}
			if(!clientId.IsNullOrDefault()){
				reqMsg.Headers.Add("X-Client-Id", clientId+"");
			}

			try{
				resp = await HttpClient.SendAsync(reqMsg, Ct);
			}catch(HttpRequestException ex){
				var e = KeysErr.Common.NetWorkErr.ToErr();
				e.AddErr(ex);
				throw e;
			}

			if(resp.StatusCode == System.Net.HttpStatusCode.Unauthorized){
				dl.Add(resp);
				_ = await RefreshBothToken(Ct);
				continue;
			}
			break;
		}

		return resp;
	}


	/// 發送 POST 請求並把回應反序列化成 TResp。
	/// 若 TResp 是 nil 可直接當作「不需回應」處理。

	public async Task<TResp?> Post<TReq, TResp>(
		str RelaUrl
		,TReq Req,
		CT Ct
	) {
		var json = JsonS.Stringify(Req);

		using var resp = await SendWithRetry(
			RelaUrl,
			json,
			j => new StringContent(j, Encoding.UTF8, "application/json"),
			Ct
		);

		var body = await resp.Content.ReadAsStringAsync(Ct);
		if(str.IsNullOrEmpty(body)){
			resp.EnsureSuccessStatusCode();
			return default;
		}

		IWebAns<TResp>? Ans = null!;
		try{
			Ans = WebAns.Deserialize<TResp>(body);
		}catch(Exception e){
			throw new Exception(
				$"Json反序列化失敗。Url={RelaUrl}, Body={body}",
				e
			);
		}

		if(Ans.Errors is not null && Ans.Errors.Count > 0){
			throw AppErr.FromViews(Ans.Errors);
		}
		resp.EnsureSuccessStatusCode();
		return Ans.Data;
	}


	/// 發送 POST 請求並把回應反序列化成 TResp。

	public async Task<TResp?> PostByteStream<TReq, TResp>(
		str RelaUrl,u8[] Req,CT Ct
	) {
		using var resp = await SendWithRetry(
			RelaUrl,
			Req,
			bytes => new ByteArrayContent(bytes)
			{
				Headers = { ContentType = new MediaTypeHeaderValue("application/octet-stream") }
			},
			Ct);

		// 可視需求打開以下兩行，保證非 2xx 直接拋
		resp.EnsureSuccessStatusCode();

		var body = await resp.Content.ReadAsStringAsync(Ct);
		if(str.IsNullOrEmpty(body)){
			return default;
		}
		try{
			var R = JsonS.Parse<TResp>(body); // 若回應是空字串則會拋例外
			return R;
		}catch(Exception){
			throw;//再次引发捕获到的异常会更改堆栈信息CA2200
			//System.Console.WriteLine("Json反序列化失敗: "+e);//t
		}
	}

	public async Task<IAnswer<nil>> RefreshBothToken(CT Ct){
		var R = new Answer<nil>();
		var User = UserCtxMgr.GetUserCtx();
		if(str.IsNullOrEmpty(User.RefreshToken)){
			return R.AddErrStr("str.IsNullOrEmpty(User.RefreshToken)");
		}
		var Url = ToolPath.SlashTrimEtJoin([BaseUrlGetter.GetBaseUrl(), KeysUrl.OpenUser.TokenRefresh]);
		var Req = new ReqRefreshTheToken{
			RefreshToken = User.RefreshToken
		};
		var Json = JsonS.Stringify(Req);
		using var content = new StringContent(
			Json, Encoding.UTF8, "application/json"
		);
		HttpResponseMessage resp;
		try{
			resp = await HttpClient.PostAsync(Url, content, Ct);
		}catch(HttpRequestException ex){
			var e = KeysErr.Common.NetWorkErr.ToErr();
			e.AddErr(ex);
			throw e;
		}

		var body = await resp.Content.ReadAsStringAsync(Ct);
		//var R2 = JsonS.Parse<RespRefreshBothToken>(body);
		var Ans = WebAns.Deserialize<RespRefreshBothToken>(body);
		var R2 = Ans.DataOrThrow();
		resp.EnsureSuccessStatusCode();

		User.RefreshToken = R2.RefreshToken;
		User.AccessToken = R2.AccessToken;
		User.AccessTokenExpireAt = R2.AccessTokenExpireAt;
		User.RefreshTokenExpireAt = R2.RefreshTokenExpireAt;

		UserCtxMgr.SetUserCtx(User);


		return R.OkWith(NIL);
	}


}
