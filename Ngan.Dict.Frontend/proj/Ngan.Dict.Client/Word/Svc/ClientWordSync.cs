namespace Ngan.Dict.Client.Word.Svc;

using System.Text;
using Ngan.Dict.Core.Frontend.User;
using Ngan.Dict.Core.Infra.Url;
using Ngan.Dict.Core.Shared.Word.Models.Dto;
using Ngan.Dict.Core.Shared.Word.Svc;
using Ngan.Dict.Core.Tools;
using Ngan.Dict.Core.Tools.Json;

public class ClientWordSync{
	IHttpCaller HttpCaller;
	IFrontendUserCtxMgr UserCtxMgr;
	ISvcWord SvcWord;
	IJsonSerializer JsonS;
	public ClientWordSync(
		IHttpCaller HttpCaller
		,IFrontendUserCtxMgr UserCtxMgr
		,ISvcWord SvcUser
		,IJsonSerializer JsonS
	){
		this.HttpCaller = HttpCaller;
		this.UserCtxMgr = UserCtxMgr;
		this.SvcWord = SvcUser;
		this.JsonS = JsonS;
	}
	public async Task<nil> AllWordsToServerNonStream(CT Ct){
		var User = UserCtxMgr.GetUserCtx();
		var Req = new ReqPackWords{
			Type = EWordsPack.LineSepJnWordJsonGZip
		};
		var textWithBlob = await SvcWord.PackAllWordsToTextWithBlobNoStream(User, Req, Ct);

		await HttpCaller.PostByteStream<DtoCompressedWords, nil>(
			KeysUrl.Word.Push
			,textWithBlob.ToByteArr(), Ct
		);

		//await HttpCaller.PostBlob<nil>();
		return NIL;
	}

	public async Task<nil> SaveAllWordsFromServerNonStream(CT Ct){
		var User = UserCtxMgr.GetUserCtx();
		var Req = new ReqPackWords{
			Type = EWordsPack.LineSepJnWordJsonGZip
		};
		using var resp = await HttpCaller.SendWithRetry(
			KeysUrl.Word.Pull
			,JsonS.Stringify(Req)
			,(json)=>new StringContent(
				json
				,Encoding.UTF8
				,"application/json"
			)
			,Ct
		);
		var bytes = await resp.Content.ReadAsByteArrayAsync(Ct);//t
		var textWithBlob = NgaqTextWithBlob.Parse(bytes);
		await SvcWord.SyncFromTextWithBlob(User, textWithBlob, Ct);
		return NIL;
	}
}
