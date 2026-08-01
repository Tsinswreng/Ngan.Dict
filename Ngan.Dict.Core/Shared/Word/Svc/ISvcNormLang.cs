using Ngaq.Core.Frontend.Kv;
using Ngaq.Core.Infra;
using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.Dictionary.Models.Po.NormLang;
using Ngaq.Core.Shared.Kv;
using Ngaq.Core.Shared.Word.Models.Dto;
using Tsinswreng.CsPage;

namespace Ngaq.Core.Shared.Word.Svc;

[Doc(@$"{nameof(PoNormLang)}
所有涉及修改{nameof(PoNormLang)}的 都要更新 {nameof(PoNormLang.BizUpdatedAt)}。
")]
public interface ISvcNormLang{
	[Doc("根據標準語言標識與類型查詢單個語言。")]
	public IAsyncEnumerable<PoNormLang?> OrdGetNormLangByTypeCode(
		IDbUserCtx Ctx,
		IAsyncEnumerable<(ELangIdentType, str)> Type_Code,
		CT Ct
	);

	public Task<nil> OrdAddNormLang(
		IDbUserCtx Ctx,
		IAsyncEnumerable<PoNormLang> Pos,
		CT Ct
	);

	public Task<nil> OrdUpdNormLang(
		IDbUserCtx Ctx,
		IAsyncEnumerable<PoNormLang> Pos,
		CT Ct
	);

	public Task<nil> OrdSoftDelNormLang(
		IDbUserCtx Ctx,
		IAsyncEnumerable<PoNormLang> Pos,
		CT Ct
	);
	
	[Doc(@$"按 以下優先級 模糊查詢:
	- {nameof(PoNormLang.Code)}
	- {nameof(PoNormLang.NativeName)}
	- TranslatedName

	按 {nameof(PoNormLang.Weight)} 降序
	
	")]
	public Task<IPageAsyE<PoNormLang>> PageNormLang(
		IDbUserCtx Ctx,
		ReqPageNormLang Req,
		CT Ct
	);
	
	
	[Doc(@$"初始化內置語言。
	#See[{nameof(InitNormLang.GetNormLangList)}]。
	如果數據庫中已有衝突的 則跳過。
	")]
	public Task<nil> InitBuiltinNormLang(IDbUserCtx Ctx, CT Ct);
	
	[Doc(@$"批量查询标准语言的翻译名称。
	#Params([],[將翻譯爲何種語言], [待被翻譯的諸語言],[])
	#Rtn[
	返值 元素 須與 {nameof(NormLangs)} 一一對應。 查不到者 對應位置即null
	]
	#Examples([
	fn(ctx, zh-TW, [en, en-US, ja])
	-> ['英語', '英語(美國)', '日語']
	])
	實 返 格式 未必 與 例 全一致。 TranslatedName僅用于界面顯示。
	")]
	public IAsyncEnumerable<str?> OrdGetTranslatedName(
		IDbUserCtx Ctx, 
		INormLang TargetLang,
		IAsyncEnumerable<INormLang> NormLangs, CT Ct
	);
	
	
	[Doc(@$"取 UI 支持的語言。
	放10種最常用的就夠 除中文 分 簡體繁體外 其他語言不用細分變體
	")]
	public IAsyncEnumerable<INormLangDetail> OrdGetUiLangs(
		CT CT
	);
	
}
