namespace Ngaq.Android.Domains.Clipboard;

using global::Android.App;
using global::Android.Content;
using Ngaq.Core.Frontend.Clipboard;
using System.Threading;
using System.Threading.Tasks;

/// Android 原生剪貼板服務：
/// 使用 ClipboardManager 讀取主剪貼板文本，供通知查詞與熱鍵查詞共用。
public class AndroidSvcClipboard : ISvcClipboard{
	public Task<string?> GetText(CancellationToken ct){
		ct.ThrowIfCancellationRequested();

		var context = Application.Context;
		var manager = context.GetSystemService(Context.ClipboardService) as ClipboardManager;
		var clip = manager?.PrimaryClip;
		if(clip is null || clip.ItemCount <= 0){
			return Task.FromResult<string?>(null);
		}

		var item = clip.GetItemAt(0);
		var text = item?.CoerceToText(context)?.ToString();
		return Task.FromResult<string?>(text);
	}
}
