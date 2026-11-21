using Ngan.Dict.Core.Shared.Base.Models.Resp;

namespace Ngan.Dict.Core.Shared.Dictionary.Models;

[Doc(@$"大模型詞典 查詢結果。
同一份結果同時承載「查詞」與「翻譯」兩種形態：
+ 查詞：`Head`／`Pronunciations`／`Descrs`；
+ 翻譯：`SourceText`／`Translation`／`Segments`／`Notes`。
兩者用 `Kind` 區分；老輸出沒有這些欄位時一律落默認值，仍可解析。
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
	[Doc(@$"已結構化的義項：詞性、釋義、例句。
	新版提示詞只要求這個欄位；`Descrs` 僅用於兼容舊提示詞與舊快取。
")]
	public IList<DictSense> Senses{get;set;}
	[Doc(@$"舊版純文本釋義。僅作兼容保留，新輸出不應再使用。")]
	public IList<str> Descrs{get;set;}

	/// 本次回答的類型，取值對齊 `EQueryKind` 的名字（`Word`／`Sentence`／`Paragraph`）。
	/// 模型沒給時為空串，調用方應以請求端的判定為準。
	public str Kind{get;set;}

	/// 翻譯路徑的原文。與 `Head` 分開：`Head` 可能被模型正規化過，原文必須保持原樣。
	public str SourceText{get;set;}

	/// 翻譯路徑的整段譯文。
	public str Translation{get;set;}

	/// 逐句對照；長段落時用來做分段顯示。
	public IList<TranslationSegment> Segments{get;set;}

	/// 術語／譯法說明；可為空。
	public IList<TranslationNote> Notes{get;set;}
}


public class RespLlmDict:IRespLlmDict, IResp{
	public str Head{get;set;} = "";
	/// <summary>
	/// 模型回傳的輸入文本語言判定；缺失時保留空物件，與舊輸出相容。
	/// </summary>
	public DetectedInputLang DetectedInputLang{get;set;} = new();
	public IList<TextedPronunciation> Pronunciations{get;set;} = [];
	public IList<DictSense> Senses{get;set;} = [];
	public IList<str> Descrs{get;set;} = [];
	public str Kind{get;set;} = "";
	public str SourceText{get;set;} = "";
	public str Translation{get;set;} = "";
	public IList<TranslationSegment> Segments{get;set;} = [];
	public IList<TranslationNote> Notes{get;set;} = [];
}

/// 逐句對照的一行：原文句與其譯文。
public class TranslationSegment{
	/// 原文句（模型不應改寫）。
	public str Src{get;set;} = "";
	/// 該句的譯文。
	public str Dst{get;set;} = "";
}

/// 翻譯附註：術語、專名或譯法取捨。
public class TranslationNote{
	/// 被說明的詞或術語。
	public str Term{get;set;} = "";
	/// 說明文字（目標語言）。
	public str Note{get;set;} = "";
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

/// 一個義項：詞性 ＋ 釋義 ＋ 例句。
/// *為何要拆開*：純文本釋義只能整段顯示；拆成欄位之後，界面才能把詞性做成標籤、
/// 例句獨立縮排、單條複製，也才談得上排版。
public class DictSense{
	/// 詞性，例如 "感嘆詞"、"n."。沒有詞性時為空串。
	public str Pos{get;set;} = "";

	/// 該義項的釋義（目標語言）。可多行，但一般不建議。
	public str Meaning{get;set;} = "";

	/// 該義項的例句；可為空。
	public IList<SenseExample> Examples{get;set;} = [];
}

/// 例句：原文（來源語言）＋ 譯文（目標語言）。
public class SenseExample{
	/// 例句原文。
	public str Src{get;set;} = "";

	/// 例句譯文。
	public str Dst{get;set;} = "";
}
