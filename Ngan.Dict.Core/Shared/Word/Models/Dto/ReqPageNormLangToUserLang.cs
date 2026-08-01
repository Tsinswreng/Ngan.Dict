using Ngaq.Core.Shared.Dictionary.Models.Po.NormLang;
using Ngaq.Core.Shared.Word.Models.Po.NormLangToUserLang;
using Tsinswreng.CsPage;

namespace Ngaq.Core.Shared.Word.Models.Dto;

[Doc(@$"
{nameof(PoNormLangToUserLang)} 分頁查詢入參。
")]
public class ReqPageNormLangToUserLang{
	public IPageQry PageQry { get; set; }
	[Doc(@$"
	同時按以下字段查詢。是取並集的關係 而不是交集。
	#See[{nameof(PoNormLangToUserLang.UserLang)}]
	#See[{nameof(PoNormLangToUserLang.NormLang)}]
	")]
	public str? SearchText { get; set; } = null;
	
}
