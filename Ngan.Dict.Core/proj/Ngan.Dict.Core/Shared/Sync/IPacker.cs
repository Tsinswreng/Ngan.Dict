using Ngan.Dict.Core.Infra;
using Ngan.Dict.Core.Shared.Base.Models.Po;
using Ngan.Dict.Core.Shared.Word.Models;
using Ngan.Dict.Core.Shared.Word.Models.Po.Kv;
using Ngan.Dict.Core.Tools.Json;
using Tsinswreng.CsErr;
using Tsinswreng.CsTextWithBlob;
using Tsinswreng.CsTools;

namespace Ngan.Dict.Core.Shared.Sync;


public interface IPacker<T>{
	
	[Doc(@$"{nameof(ITextWithStream)}格式、
	{nameof(ITextWithStream.Payload)}默認爲 GZip壓縮後之Jsonl(ndjson)。
	#See[{nameof(ItblToStream<>)}]
	#See[{nameof(GZipLinesUtf8)}]
	")]
	public ITextWithStream Pack(
		IAsyncEnumerable<T> Objs, IObjPackInfo PackInfo, CT Ct
	);
	
	[Doc(@$"
	{nameof(ITextWithStream)}格式 默認 GZip壓縮後之Jsonl(ndjson)。
	#See[{nameof(ItblToStream<>)}]
	#See[{nameof(GZipLinesUtf8)}]
	")]
	public IAnswer<IAsyncEnumerable<T>> Unpack(
		ITextWithStream TextWithBlob, CT Ct
	);
	
	
}
