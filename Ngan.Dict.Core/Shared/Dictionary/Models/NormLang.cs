using Ngaq.Core.Infra.IF;

namespace Ngaq.Core.Shared.Dictionary.Models;

[Doc(@$"Normalized Language")]
public interface INormLang : IAppSerializable{
	public ELangIdentType Type{get;set;}
	public str Code{get;set;}
}


public interface INormLangDetail : INormLang{
	public str NativeName{get;set;}
	public str EnglishName{get;set;}
	
	[Doc("可用于排序。常用的語言權重大")]
	public f64 Weight{get;set;}
}

public interface I_NormLang_TranslatedName{
	[Doc($$"""
	#Example([
	for ja-JP:
		zh-TW: 日語(日本)
		zh-CN: 日语(日本)
		en: Japanese(Japan)
	])
	""")]
	public IDictionary<INormLang, str> NormLang_TranslatedName{get;set;}
}

public class NormLang
	:INormLang
	,IAppSerializable
{
	public ELangIdentType Type{get;set;} = ELangIdentType.Bcp47;
	public str Code{get;set;} = "";
	
}

public class NormLangDetail
	:NormLang
	,INormLangDetail
	,I_NormLang_TranslatedName
	,IAppSerializable
{
	public str NativeName{get;set;} = "";
	public str EnglishName{get;set;} = "";
	public f64 Weight{get;set;} = 0;
	public IDictionary<INormLang, str> NormLang_TranslatedName{get;set;} = new Dictionary<INormLang, str>();
}

