using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Service.Parser.Model;
using Ngaq.Core.Word.Parser;

namespace Ngaq.Core.Service.Parser;


/// 單詞表文本解析結果轉Bo_Word

public  partial class ParseResultMapper(){
	protected static ParseResultMapper? _Inst = null;
	public static ParseResultMapper Inst => _Inst??= new ParseResultMapper();



/// TODO
/// 若key無命名空間 則須潙內置鍵 即Const_PropKey中ʃ有
/// 若有命名空間則檢格式 取末個「:」作分隔 如Tsinswreng:MyCustomPropKey。不合此格式則錯

/// <param name="key"></param>
/// <returns></returns>
	public nil CheckPropKey(str key){

		return NIL;
	}

	public PoWordProp PropToKv(I_Prop prop){
		var po_kv = new PoWordProp();
		po_kv.SetStrStr(prop.Key.Text.Trim(), prop.Value.Text.Trim());
		return po_kv;
	}

	public IList<JnWord> Map(
		WordListTxtMetadata Metadata
		,IList<I_DateBlock> DateBlocks
	){
		if(Metadata.belong is null){
			throw new Exception("Metadata.Belong is null");//TODO i18n
		}
		var Ans = new List<JnWord>();
		foreach(var dateBlock in DateBlocks){
			var dateStr = dateBlock.Date.Text.Trim();
			var Date = DateTimeOffset.Parse(dateStr);
			var UnixMs = Date.ToUnixTimeMilliseconds();
			foreach(var wordBlock in dateBlock.Words){
				var ua = new JnWord();
				ua.BizCreatedAt = UnixMs;
				ua.Word.Lang = Metadata.belong;
				var trimedHead = wordBlock.Head?.Text?.Trim();
				if(trimedHead == null || str.IsNullOrEmpty(trimedHead)){
					continue;
				}
				foreach(var prop in wordBlock.Props){//除description以外的屬性
					var PoKv = PropToKv(prop);
					PoKv.BizCreatedAt = UnixMs;
					ua.Props.Add(PoKv);
				}

				ua.Word.Head = trimedHead;

				var bodyStrList = new List<str>();
				foreach(var seg in wordBlock.Body){
					bodyStrList.Add(seg.Text.Trim());
				}
				var bodyStr = string.Join("\n", bodyStrList);
				var kv_descr = new PoWordProp();
				kv_descr.BizCreatedAt = UnixMs;
				kv_descr.SetStrToken(
					null, KeysProp.Inst.description, bodyStr.Trim()
				);
				ua.Props.Add(kv_descr);
				foreach (var prop in dateBlock.Props){
					var po_kv = PropToKv(prop);
					po_kv.BizCreatedAt = UnixMs;
					ua.Props.Add(po_kv);
				}
				ua.EnsureForeignId();
				Ans.Add(ua);
			}//~foreach(var wordBlock in dateBlock.Words)
		}//~foreach(var dateBlock in DateBlocks)
		return Ans;
	}
}
