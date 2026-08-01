using Ngaq.Core.Shared.Dictionary.Models.Po.NormLang;
using Tsinswreng.CsPage;

namespace Ngaq.Core.Shared.Word.Models.Dto;

[Doc(@$"
{nameof(PoNormLang)} 分頁查詢入參。
")]
public class ReqPageNormLang{
	public IPageQry PageQry{get;set;}
	[Doc("按 Code/NativeName/TranslatedName 的優先級模糊查詢，優先命中 Code，其次 NativeName，最後 TranslatedName。")]
	public str? SearchText{get;set;} = null;
}
