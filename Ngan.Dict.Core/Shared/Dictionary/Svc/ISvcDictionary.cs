using Ngaq.Core.Frontend.Kv;
using Ngaq.Core.Infra;
using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.Dictionary.Models.Po.NormLang;
using Ngaq.Core.Shared.Kv;
using Ngaq.Core.Shared.User.UserCtx;

namespace Ngaq.Core.Shared.Dictionary.Svc;


[Doc("Llm Dictionary")]
public interface ISvcDictionary{
	public Task<IRespLlmDict> Lookup(IUserCtx User, IReqLlmDict Req, CT Ct);
	/// 直接用原始 LLM 響應文本重新解析詞典結構（不再次調用 LLM）。
	public IRespLlmDict ParseRawOutput(str RawOutput);
	
	// [Doc(@$"#See[{nameof(KeysClientKv.Dictionary.RecentUsedNormLangs)}]")]
	// public Task<IList<NormLang>> GetRecentUsedNormLangs(
	// 	IDbUserCtx Ctx, CT Ct
	// );
	
	[Doc(@$"
		詞典當前設定的源語言
		#See[{nameof(KeysKv.Dictionary.CurSrcLang)}]
	")]
	public Task<PoNormLang?> GetCurSrcNormLang(IDbUserCtx Ctx, CT Ct);
	
	[Doc(@$"
		詞典當前設定的源語言
		#See[{nameof(KeysKv.Dictionary.CurSrcLang)}]
	")]
	public Task<PoNormLang?> SetCurSrcNormLang(
		IDbUserCtx Ctx, PoNormLang Po, CT Ct
	);
	
	[Doc(@$"
		詞典當前設定的目標語言
		#See[{nameof(KeysKv.Dictionary.CurTgtLang)}]
	")]
	public Task<PoNormLang?> GetCurTgtNormLang(IDbUserCtx Ctx, CT Ct);
	
	[Doc(@$"
		詞典當前設定的目標語言
		#See[{nameof(KeysKv.Dictionary.CurTgtLang)}]
	")]
	public Task<PoNormLang?> SetCurTgtNormLang(
		IDbUserCtx Ctx, PoNormLang Po, CT Ct
	);
	

	[Doc(@$"獲取詞典系統提示語，若未設定則返回默認提示語。")]
	public Task<str> GetLlmDictSysPromptOrDflt(IDbUserCtx Ctx, CT Ct);
	
	[Doc(@$"設置詞典系統提示語。")]
	public Task<str> SetLlmDictSysPrompt(IDbUserCtx Ctx, str Prompt, CT Ct);
	
}
