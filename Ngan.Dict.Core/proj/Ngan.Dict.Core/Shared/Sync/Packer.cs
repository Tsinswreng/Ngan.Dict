using Ngan.Dict.Core.Infra;
using Ngan.Dict.Core.Shared.Base.Models.Po;
using Ngan.Dict.Core.Shared.Word.Models;
using Ngan.Dict.Core.Shared.Word.Models.Po.Kv;
using Ngan.Dict.Core.Tools.Json;
using Tsinswreng.CsErr;
using Tsinswreng.CsTextWithBlob;
using Tsinswreng.CsTools;

namespace Ngan.Dict.Core.Shared.Sync;

public class Packer<T>:IPacker<T>{
	//public ItblToStream<T> ItblToStream{get;set;}
	public GZipLinesUtf8 GZipLinesUtf8{get;set;} = new();
	public Tsinswreng.CsJson.IJsonSerializer JsonS{get;set;} = AppJsonS.Inst;
	public ITextWithStream Pack(
		IAsyncEnumerable<T> Objs, IObjPackInfo PackInfo, CT Ct
	){
		var meta = JsonS.Serialize(PackInfo);
		var lines = Objs.Select(x=>JsonS.Serialize(x));
		var payload = GZipLinesUtf8.ToStream(lines, Ct).GetAwaiter().GetResult();
		return TextWithStream.MkUtf8(meta, payload);
	}
	
	public IAnswer<IAsyncEnumerable<T>> Unpack(
		ITextWithStream TextWithBlob, CT Ct
	){
		var ans = new Answer<IAsyncEnumerable<T>>();
		try{
			// 先解析元信息；此處只保證可解析，不對 PayloadType 作額外校驗。
			_ = JsonS.Deserialize<ObjPackInfo>(TextWithBlob.Text);
			// 再把 gzip 行流轉回對象流，保持懶讀取。
			var lines = GZipLinesUtf8.ToLines(TextWithBlob.Payload, Ct);
			var objs = lines.Select(line=>JsonS.Deserialize<T>(line)!);
			return ans.OkWith(objs);
		}catch(Exception e){
			return ans.AddErr(e);
		}
	}
	
	
}