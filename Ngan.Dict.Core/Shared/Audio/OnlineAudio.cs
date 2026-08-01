using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Ngaq.Core.Infra.Errors;
using Tsinswreng.CsErr;

namespace Ngaq.Core.Shared.Audio;

public class OnlineAudio {
	private static readonly HttpClient _http = new HttpClient {
		Timeout = TimeSpan.FromSeconds(30)
	};

	
	/// 下載一次，之後交給 Audio 的工廠無限重讀。
	public async Task<Audio> Get(string url, CT Ct = default) {
		if (string.IsNullOrWhiteSpace(url))
			throw new ArgumentException("Url is empty.");

		using var req = new HttpRequestMessage(HttpMethod.Get, url);
		return await Get(req, Ct);
	}

	/// <summary>
	/// 使用調用方自定義的請求頭下載音頻。
	/// 主要給像 gTTS 這類會根據 User-Agent / Referer 做風控的來源使用。
	/// </summary>
	public async Task<Audio> Get(HttpRequestMessage Req, CT Ct = default) {
		if(Req.RequestUri is null){
			throw new ArgumentException("RequestUri is empty.");
		}

		using var req = Req;
		try{
			// 1. 下載到記憶體
			// Android 上若恢復到 UI 線程時再 Dispose 響應，可能觸發 NetworkOnMainThreadException。
			// 因此這裡明確不捕獲同步上下文，讓後續讀取與釋放都留在後台線程。
			using var resp = await _http
				.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, Ct)
				.ConfigureAwait(false);
			resp.EnsureSuccessStatusCode();

			var url = req.RequestUri.ToString();
			var type = DetectType(resp.Content.Headers.ContentType?.MediaType, url);

			var ms = new MemoryStream();
			await resp.Content.CopyToAsync(ms, Ct).ConfigureAwait(false);
			var bytes = ms.ToArray();          // 真正快取起來

			// 2. 包成工廠：每次給一條全新的 MemoryStream
			Task<Stream> Factory(CancellationToken _)
				=> Task.FromResult<Stream>(new MemoryStream(bytes));

			return new Audio(Factory, type);
		}catch(OperationCanceledException) when(Ct.IsCancellationRequested){
			throw;
		}catch(HttpRequestException ex){
			throw KeysErr.Common.NetWorkErr.ToErr().AddErr(ex).AddDebugArgs(req.RequestUri.ToString());
		}catch(TaskCanceledException ex){
			// HttpClient 超時等也可能走 TaskCanceledException，按網絡錯誤處理。
			throw KeysErr.Common.NetWorkErr.ToErr().AddErr(ex).AddDebugArgs(req.RequestUri.ToString());
		}
	}

	#region 原本的分類邏輯保持不變
	private static EAudioType DetectType(string? mime, string url) {
		if (mime != null) {
			return mime switch {
				"audio/mpeg" or "audio/mp3" => EAudioType.Mp3,
				"audio/wav" or "audio/x-wav" => EAudioType.Wav,
				_ => InferFromExtension(url)
			};
		}
		return InferFromExtension(url);
	}

	private static EAudioType InferFromExtension(string url) {
		var ext = Path.GetExtension(url.AsSpan()).TrimStart('.').ToString().ToLowerInvariant();
		return ext switch {
			"mp3" => EAudioType.Mp3,
			"wav" => EAudioType.Wav,
			_ => throw new NotSupportedException($"Unknown audio type: {url}")
		};
	}
	#endregion
}
