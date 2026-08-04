namespace Ngan.Dict.Core.Shared.Dictionary.Models;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Ngan.Dict.Core.Shared.Base.Models.Req;
using Tsinswreng.CsTempus;

public class DtoOnNewSeg{
	public str? NewSeg{get;set;}
}

public class DtoOnDone{
	
}



/// 词典查询请求接口
public interface IReqLlmDict:IReq{
	/// 唯一请求ID，用于追踪/缓存
	public str Id { get; set; }
	public UnixMs UnixMs { get; set; }
	/// 核心查询内容
	public Query Query { get; set; }
	/// 语言配置
	public OptLang OptLang { get; set; }

	/// 用户偏好配置
	public Preferences? Preferences { get; set; }
}

public interface IReqLlmDictEvt{
	/// 當收到新的文本片段時觸發
	/// 返ʹ值: 返0
	public Func<DtoOnNewSeg, CT, i32>? OnNewSeg{get;set;}
	/// llm響應終旹觸發
	/// 返ʹ值: 返0
	public Func<DtoOnDone, CT, i32>? OnDone{get;set;}

}

/// 词典查询请求，支持多模式查询
public class ReqLlmDict:IReqLlmDict{
	/// 唯一请求ID，用于追踪/缓存
	public str Id { get; set; } = "";
	public UnixMs UnixMs { get; set; }

	/// 核心查询内容
	public Query Query { get; set; } = new Query();

	/// 语言配置
	public OptLang OptLang { get; set; } = new OptLang();

	/// 用户偏好配置
	public Preferences? Preferences { get; set; } = null;

	/// 扩展元数据（用于AI个性化）暫不需
	//public UserContext UserContext { get; set; } = new UserContext();
}

public class ReqLlmDictEvt:ReqLlmDict, IReqLlmDictEvt{
	
	/// 當收到新的文本片段時觸發
	/// 返ʹ值: 返0
	
	public Func<DtoOnNewSeg, CT, i32>? OnNewSeg{get;set;}

	
	/// llm響應終旹觸發
	/// 返ʹ值: 返0
	
	public Func<DtoOnDone, CT, i32>? OnDone{get;set;}

}

/// 核心查询内容
public class Query {
	[Doc(@$"查詢文本。
	可能是詞或整句。
	")]
	public str InputText { get; set; } = "";

	/// 可选：上下文句子，用于消歧
	public str? ContextSentence { get; set; }

}

/// 语言配置（核心动态配置）
public class OptLang {
	/// 源语言配置
	public NormLangDetail SrcLang { get; set; } = new NormLangDetail();

	/// 目标语言配置
	public IList<NormLangDetail> TgtLangs { get; set; } = [];
}


/// 用户偏好配置
public class Preferences {
	//public DetailLevel DetailLevel { get; set; } = DetailLevel.Comprehensive;
	public bool TryIncludeExamples { get; set; } = true;
	public bool TryIncludeSynonyms { get; set; } = true;
	public bool TryIncludeAntonyms { get; set; } = false;
	//詞源
	public bool TryIncludeEtymology { get; set; } = true;
	public int MaxExamples { get; set; } = 3;
}

// /// 扩展元数据（用于AI个性化）
// public class UserContext {
// 	/// 用户语言水平，影响释义复杂度
// 	public ProficiencyLevel ProficiencyLevel { get; set; } = ProficiencyLevel.Intermediate;

// 	/// 可选：关联查询历史（用于连贯释义）暫不實現
// 	//public IList<str> PreviousQueries { get; set; } = new List<str>();
// }



#if false
class Sample{
	const str s =
"""
Id: "1cwMqe3zEnhxG28qZoRVX"
UnixMs: 1740000000000
Query:
  InputText: "acquiesce"
  ContextSentence: "He chose to acquiesce to the decision"
OptLang:
  SrcLang:
    Iso639_1: "en"
    Variety: "us"
    Script: null
  TgtLangs:
    - Iso639_1: "zh"
      Variety: "tw"
      Script: "hant"
    - Iso639_1: "ja"
      Variety: "jp"
      Script: ""
""";
}


#endif
