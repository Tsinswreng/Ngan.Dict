using Ngan.Dict.Core.Infra;
using Ngan.Dict.Core.Shared.Dictionary.Models;
using Ngan.Dict.Core.Shared.Word.Models.Dto;
using Ngan.Dict.Core.Shared.Word.Models.Po.NormLangToUserLang;
using Tsinswreng.CsPage;

namespace Ngan.Dict.Core.Shared.Word.Svc;

[Doc($"{nameof(PoNormLangToUserLang)} 服務介面。")]
public interface ISvcNormLangToUserLang{
	[Doc("根據標準語言類型與標識值，查詢單個用戶自定義語言。")]
	public Task<str?> GetUserLangByNormLang(
		IDbUserCtx Ctx,
		ELangIdentType NormLangType,
		str NormLang,
		CT Ct
	);
	
	public Task<nil> BatAddNormLangToUserLang(
		IDbUserCtx Ctx,
		IAsyncEnumerable<PoNormLangToUserLang> Pos,
		CT Ct
	);
	
	public Task<nil> BatUpdNormLangToUserLang(
		IDbUserCtx Ctx,
		IAsyncEnumerable<PoNormLangToUserLang> Pos,
		CT Ct
	);
	
	public Task<nil> BatSoftDelNormLangToUserLang(
		IDbUserCtx Ctx,
		IAsyncEnumerable<PoNormLangToUserLang> Pos,
		CT Ct
	);
	
	public Task<IPageAsyE<PoNormLangToUserLang>> PageNormLangToUserLang(
		IDbUserCtx Ctx,
		ReqPageNormLangToUserLang Req,
		CT Ct
	);
}
