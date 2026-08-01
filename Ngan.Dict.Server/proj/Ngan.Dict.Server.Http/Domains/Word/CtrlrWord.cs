namespace Ngan.Dict.Server.Http.Domains.Word;

using Ngan.Dict.Core.Infra.Errors;
using Ngan.Dict.Core.Shared.Word.Models.Dto;
using Ngan.Dict.Core.Shared.Word.Svc;
using Ngan.Dict.Core.Tools;
using Ngan.Dict.Server.Http.Infra;
using Tsinswreng.CsTextWithBlob;
using U = Ngan.Dict.Core.Infra.Url.KeysUrl.Word;

public class CtrlrWord(
	ISvcWord SvcWord
) : ICtrlr{
	public nil InitRouter(
		RouteGroupBuilder R
	){
		R.MapPost(U.Push, ReceiveFull);
		R.MapPost(U.Pull, SendFull);

		return NIL;
	}

	public async Task<IResult> ReceiveFull(
		HttpContext Ctx, CT Ct
	){
		using var ms = new MemoryStream();
		await Ctx.Request.Body.CopyToAsync(ms, Ct);
		var Body = ms.ToArray();
		var textWithBlob = NgaqTextWithBlob.Parse(Body);
		await SvcWord.SyncFromTextWithBlob(Ctx.ToUserCtx(), textWithBlob, Ct);
		//return Results.Ok();
		return this.Ok();
	}

	public async Task<IResult> SendFull(
		ReqPackWords Req
		,HttpContext Ctx, CT Ct
	){
		var textWithBlob = await SvcWord.PackAllWordsToTextWithBlobNoStream(
			Ctx.ToUserCtx(), Req, Ct
		);
		var bytes = textWithBlob.ToByteArr();
		return Results.Bytes(bytes);
	}
}
