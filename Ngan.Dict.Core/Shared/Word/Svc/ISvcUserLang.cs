
using Ngaq.Core.Infra;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Core.Shared.Word.Models.Po.UserLang;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Tsinswreng.CsPage;

namespace Ngaq.Core.Shared.Word.Svc;

[Doc(@$"{nameof(PoUserLang)}
所有涉及修改{nameof(PoUserLang)}的 都要更新 {nameof(PoUserLang.BizUpdatedAt)}。
")]
public interface ISvcUserLang{
	public Task<IPageAsyE<PoUserLang>> PageUserLang(
		IDbUserCtx Ctx, ReqPageUserLang Req, CT Ct
	);

	public IAsyncEnumerable<PoUserLang?> OrdGetUserLang(
		IDbUserCtx Ctx, IAsyncEnumerable<str> UniqNames, CT Ct
	);
	
	public Task<nil> OrdUpdUserLang(
		IDbUserCtx Ctx, IAsyncEnumerable<PoUserLang> Pos, CT Ct
	);
	
	public Task<nil> OrdAddUserLang(
		IDbUserCtx Ctx, IAsyncEnumerable<PoUserLang> Pos, CT Ct
	);
	
	
	[Doc(@$"獲取{nameof(PoWord)}中存在 但 {nameof(PoUserLang)}中不存在的語言。
	{nameof(PoWord.Lang)} 對應 {nameof(PoUserLang.UniqName)}
	")]
	public IAsyncEnumerable<str> GetUnregisteredUserLangs(
		IDbUserCtx Ctx, CT Ct
	);
	
	[Doc(@$"#See[{nameof(GetUnregisteredUserLangs)}]")]
	public Task<nil> AddAllUnregisteredUserLangs(IDbUserCtx Ctx, CT Ct);
	
}
