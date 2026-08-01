using Ngan.Dict.Core.Infra;
using Ngan.Dict.Core.Model.Po.Kv;
using Ngan.Dict.Core.Model.Po.Learn_;
using Ngan.Dict.Core.Shared.Dictionary.Models;
using Ngan.Dict.Core.Shared.StudyPlan.Models.PreFilter;
using Ngan.Dict.Core.Shared.Word.Models;
using Ngan.Dict.Core.Shared.Word.Models.Dto;
using Ngan.Dict.Core.Shared.Word.Models.Po.Kv;
using Ngan.Dict.Core.Shared.Word.Models.Po.Learn;
using Ngan.Dict.Core.Shared.Word.Models.Po.Word;
using Ngan.Dict.Core.Shared.Word.Svc;
using Ngan.Dict.Core.Tools.Json;
using Tsinswreng.CsPage;
using Tsinswreng.CsTools;
using U = Ngan.Dict.Core.Infra.Url.KeysUrl.WordV2;

namespace Ngan.Dict.Client.Domains.Word;

public class HttpWordV2 : ISvcWordV2 {
	/// HTTP 請求調用器。
	protected IHttpCaller HttpCaller{get;set;}

	/// JSON 序列化器。
	protected IJsonSerializer JsonS{get;set;}

	/// 構造函數。
	/// <param name="HttpCaller">HTTP 請求調用器。</param>
	/// <param name="JsonS">JSON 序列化器。</param>
	public HttpWordV2(IHttpCaller HttpCaller, IJsonSerializer JsonS){
		this.HttpCaller = HttpCaller;
		this.JsonS = JsonS;
	}

	public Task<nil> OrdAddJnWord(IDbUserCtx Ctx, IAsyncEnumerable<JnWord> Words, CT Ct) {
		return PostReqNoResp(U.BatAddJnWord, Words, Ct);
	}

	public Task<nil> OrdAddNewLearnRecord(IDbUserCtx Ctx, IAsyncEnumerable<PoWordLearn> PoWordLearnAsyE, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<nil> OrdAddWordLearn(IDbUserCtx Ctx, IAsyncEnumerable<PoWordLearn> WordLearns, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<nil> OrdAddWordProp(IDbUserCtx Ctx, IAsyncEnumerable<PoWordProp> WordProps, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<nil> OrdChangeId(IDbUserCtx Ctx, IAsyncEnumerable<(IdWord Old, IdWord New)> Ids, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<nil> OrdSyncByDto(IDbUserCtx Ctx, IAsyncEnumerable<DtoJnWordSyncResult> Dtos, CT Ct) {
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<DtoJnWordSyncResult> OrdSyncJnWordByBizId(
		IDbUserCtx Ctx, IAsyncEnumerable<JnWord> JnWords, CT Ct
	){
		return PostReqResp<JnWord, DtoJnWordSyncResult>(U.BatSyncJnWordByBizId, JnWords, Ct);
	}

	public IAsyncEnumerable<DtoJnWordSyncResult> OrdSyncJnWordByBizIdFromStream(IDbUserCtx Ctx, Stream TextWithStream, CT Ct) {
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<RespUpdBizId> OrdUpdHeadLang(IDbUserCtx Ctx, IAsyncEnumerable<PoWord> PoWords, CT Ct) {
		return PostReqResp<PoWord, RespUpdBizId>(U.BatUpdHeadLang, PoWords, Ct);
	}

	public Task<IAsyncEnumerable<RespUpdPoWord>> OrdUpdPoWord(IDbUserCtx Ctx, IAsyncEnumerable<PoWord> PoWords, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<nil> OrdUpdWordLearn(IDbUserCtx Ctx, IAsyncEnumerable<PoWordLearn> WordLearns, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<nil> OrdUpdWordProp(IDbUserCtx Ctx, IAsyncEnumerable<PoWordProp> WordProps, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<nil> DelWordLearnInId(IDbUserCtx Ctx, IAsyncEnumerable<IdWordLearn> Ids, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<nil> DelWordPropInId(IDbUserCtx Ctx, IAsyncEnumerable<IdWordProp> Ids, CT Ct) {
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<JnWord> GetAllWordWithDel(IDbUserCtx Ctx, CT Ct) {
		return PostNoReqResp<JnWord>(U.GetAllWordsWithDel, Ct);
	}

	public IAsyncEnumerable<IJnWordMergeResult> GetWordMergeResult(IDbUserCtx Ctx, IAsyncEnumerable<JnWord> Words, CT Ct) {
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<JnWord> GetWordsToLearn(IDbUserCtx Ctx, CT Ct) {
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<JnWord> GetWordsToLearn(IDbUserCtx Ctx, PreFilter? Prefilter, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<JnWord> LlmDictWordToJnWord(IDbUserCtx Ctx, IReqLlmDict Req, IRespLlmDict LlmDict, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<JnWord> LlmDictWordToJnWordWithLearn(IDbUserCtx Ctx, IReqLlmDict Req, IRespLlmDict LlmDict, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<nil> MergeWord(IDbUserCtx Ctx, IAsyncEnumerable<IJnWordMergeResult> Words, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<nil> MergeWord(IDbUserCtx Ctx, IAsyncEnumerable<JnWord> Words, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<nil> MergeWord_NewDescrAsAdd(IDbUserCtx Ctx, IAsyncEnumerable<JnWord> Words, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<Stream> PackAllWordWithDel(IDbUserCtx Ctx, CT Ct) {
		return PostNoReqRawStream(U.Pull, Ct);
	}

	public Task<nil> SoftDelJnWordInId(IDbUserCtx Ctx, IAsyncEnumerable<IdWord> Ids, CT Ct) {
		return PostReqNoResp(U.SoftDelJnWordInId, Ids, Ct);
	}

	public IAsyncEnumerable<JnWord> UnpackJnWords(Stream TextWithStream, CT Ct) {
		throw new NotImplementedException();
	}

	/// 通用模板：gzip 行流請求 + 無返回體。
	/// <typeparam name="TReq">請求元素類型。</typeparam>
	/// <param name="Url">相對路由。</param>
	/// <param name="Req">請求元素流。</param>
	/// <param name="Ct">取消令牌。</param>
	/// <returns>成功返回 NIL。</returns>
	protected async Task<nil> PostReqNoResp<TReq>(
		str Url,
		IAsyncEnumerable<TReq> Req,
		CT Ct
	){
		using var resp = await HttpCaller.SendWithRetry(
			Url,
			Req,
			async (req, ct)=>await MkGzipJsonLineContent(req, ct),
			Ct
		);
		resp.EnsureSuccessStatusCode();
		return NIL;
	}

	/// 通用模板：gzip 行流請求 + gzip 行流返回。
	/// <typeparam name="TReq">請求元素類型。</typeparam>
	/// <typeparam name="TResp">返回元素類型。</typeparam>
	/// <param name="Url">相對路由。</param>
	/// <param name="Req">請求元素流。</param>
	/// <param name="Ct">取消令牌。</param>
	/// <returns>返回元素流。</returns>
	protected async IAsyncEnumerable<TResp> PostReqResp<TReq, TResp>(
		str Url,
		IAsyncEnumerable<TReq> Req,
		[System.Runtime.CompilerServices.EnumeratorCancellation] CT Ct
	){
		using var resp = await HttpCaller.SendWithRetry(
			Url,
			Req,
			async (req, ct)=>await MkGzipJsonLineContent(req, ct),
			Ct
		);
		resp.EnsureSuccessStatusCode();

		using var stream = await resp.Content.ReadAsStreamAsync(Ct);
		await foreach(var line in GZipLinesUtf8.ToLines(stream, Ct)){
			yield return JsonS.Parse<TResp>(line)!;
		}
	}

	/// 通用模板：空請求體 + gzip 行流返回。
	/// <typeparam name="TResp">返回元素類型。</typeparam>
	/// <param name="Url">相對路由。</param>
	/// <param name="Ct">取消令牌。</param>
	/// <returns>返回元素流。</returns>
	protected async IAsyncEnumerable<TResp> PostNoReqResp<TResp>(
		str Url,
		[System.Runtime.CompilerServices.EnumeratorCancellation] CT Ct
	){
		using var resp = await HttpCaller.SendWithRetry(
			Url,
			Array.Empty<u8>(),
			(bytes)=>new ByteArrayContent(bytes),
			Ct
		);
		resp.EnsureSuccessStatusCode();
		using var stream = await resp.Content.ReadAsStreamAsync(Ct);
		await foreach(var line in GZipLinesUtf8.ToLines(stream, Ct)){
			yield return JsonS.Parse<TResp>(line)!;
		}
	}

	/// 通用模板：空請求體 + 原始字節流返回。
	/// <param name="Url">相對路由。</param>
	/// <param name="Ct">取消令牌。</param>
	/// <returns>返回字節流。</returns>
	protected async Task<Stream> PostNoReqRawStream(str Url, CT Ct){
		var resp = await HttpCaller.SendWithRetry(
			Url,
			Array.Empty<u8>(),
			(bytes)=>new ByteArrayContent(bytes),
			Ct
		);
		resp.EnsureSuccessStatusCode();
		var stream = await resp.Content.ReadAsStreamAsync(Ct);
		return new HttpResponseOwnedStream(resp, stream);
	}

	/// 把異步元素流轉成 gzip json 行 HTTP 請求體。
	/// <typeparam name="TReq">請求元素類型。</typeparam>
	/// <param name="Req">請求元素流。</param>
	/// <param name="Ct">取消令牌。</param>
	/// <returns>可直接發送的 HttpContent。</returns>
	protected async Task<HttpContent> MkGzipJsonLineContent<TReq>(
		IAsyncEnumerable<TReq> Req,
		CT Ct
	){
		var lines = Req.Select(item=>JsonS.Stringify(item));
		var body = await GZipLinesUtf8.ToStream(lines, Ct);
		var content = new StreamContent(body);
		content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
		return content;
	}

	public Task<object> SoftDelPoWordInId(IDbUserCtx Ctx, IAsyncEnumerable<IdWord> Ids, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> HardDelSoftDeleted(IDbUserCtx Ctx, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<IPage<DtoWordSearchHit>> PageSearch(IDbUserCtx Ctx, IPageQry PageQry, ReqSearchWord Req, CT Ct) {
		throw new NotImplementedException();
	}

	/// 包裝響應流，確保調用方釋放流時連同 HttpResponseMessage 一併釋放。
	private sealed class HttpResponseOwnedStream:Stream{
		private readonly HttpResponseMessage _resp;
		private readonly Stream _inner;
		private bool _disposed = false;

		public HttpResponseOwnedStream(HttpResponseMessage Resp, Stream Inner){
			_resp = Resp;
			_inner = Inner;
		}

		public override bool CanRead => !_disposed && _inner.CanRead;
		public override bool CanSeek => _inner.CanSeek;
		public override bool CanWrite => _inner.CanWrite;
		public override i64 Length => _inner.Length;
		public override i64 Position{
			get => _inner.Position;
			set => _inner.Position = value;
		}

		public override void Flush(){ _inner.Flush(); }
		public override Task FlushAsync(CT Ct){ return _inner.FlushAsync(Ct); }
		public override i32 Read(byte[] Buffer, i32 Offset, i32 Count){ return _inner.Read(Buffer, Offset, Count); }
		public override i32 Read(Span<byte> Buffer){ return _inner.Read(Buffer); }
		public override Task<i32> ReadAsync(byte[] Buffer, i32 Offset, i32 Count, CT Ct){ return _inner.ReadAsync(Buffer, Offset, Count, Ct); }
		public override ValueTask<i32> ReadAsync(Memory<byte> Buffer, CT Ct = default){ return _inner.ReadAsync(Buffer, Ct); }
		public override i64 Seek(i64 Offset, SeekOrigin Origin){ return _inner.Seek(Offset, Origin); }
		public override void SetLength(i64 Value){ _inner.SetLength(Value); }
		public override void Write(byte[] Buffer, i32 Offset, i32 Count){ _inner.Write(Buffer, Offset, Count); }
		public override void Write(ReadOnlySpan<byte> Buffer){ _inner.Write(Buffer); }
		public override Task WriteAsync(byte[] Buffer, i32 Offset, i32 Count, CT Ct){ return _inner.WriteAsync(Buffer, Offset, Count, Ct); }
		public override ValueTask WriteAsync(ReadOnlyMemory<byte> Buffer, CT Ct = default){ return _inner.WriteAsync(Buffer, Ct); }

		protected override void Dispose(bool Disposing){
			if(_disposed){
				return;
			}
			if(Disposing){
				_inner.Dispose();
				_resp.Dispose();
			}
			_disposed = true;
			base.Dispose(Disposing);
		}

		public override async ValueTask DisposeAsync(){
			if(_disposed){
				return;
			}
			await _inner.DisposeAsync();
			_resp.Dispose();
			_disposed = true;
			await base.DisposeAsync();
		}
	}
}
