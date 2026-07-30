using Ngaq.Core.Shared.Base.Models.Resp;

namespace Ngaq.Core.Shared.Dictionary.Models;

[Doc(@$"大模型詞典 查詢結果。
簡易 通用版、不支持按義項區分/獨立例句字段 等
")]
public interface IRespLlmDict{
	public str Head{get;set;}
	[Doc(@$"模型根據本次輸入文本判定出的語言。
	此為響應專用對象，只保留代碼及母語名，
	供調用方與請求的來源語言配置比對；
	不參與既有詞典結果到 JnWord 的映射。
")]
	public DetectedInputLang DetectedInputLang{get;set;}
	[Doc(@$"可能有多種讀音")]
	public IList<TextedPronunciation> Pronunciations{get;set;}
	[Doc(@$"可能有多種、要圖簡便也可以全放首元素裏")]
	public IList<str> Descrs{get;set;}
}


public class RespLlmDict:IRespLlmDict, IResp{
	public str Head{get;set;} = "";
	/// <summary>
	/// 模型回傳的輸入文本語言判定；缺失時保留空物件，與舊輸出相容。
	/// </summary>
	public DetectedInputLang DetectedInputLang{get;set;} = new();
	public IList<TextedPronunciation> Pronunciations{get;set;} = [];
	public IList<str> Descrs{get;set;} = [];
}

/// <summary>
/// 大模型對本次輸入文本作出的語言判定。
/// <para>它不是使用者選擇的語言配置，故不復用 NormLangDetail；
/// 僅保留模型判定的語言身份所需資料。</para>
/// </summary>
public class DetectedInputLang{
	/// <summary>
	/// 模型判定出的 BCP 47 語言代碼，例如 "es"。
	/// </summary>
	public str Code{get;set;} = "";

	/// <summary>
	/// 模型以該語言自身文字給出的名稱，例如 "Español"。
	/// </summary>
	public str NativeName{get;set;} = "";
}
