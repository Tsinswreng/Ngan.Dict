using Ngan.Dict.Core.Frontend.Kv;
using Ngan.Dict.Core.Infra;
using Ngan.Dict.Core.Infra.Errors;
using Ngan.Dict.Core.Model.Po.Kv;
using Ngan.Dict.Core.Model.Po.Learn_;
using Ngan.Dict.Core.Shared.Dictionary.Models;
using Ngan.Dict.Core.Shared.StudyPlan.Models.PreFilter;
using Ngan.Dict.Core.Shared.Sync;
using Ngan.Dict.Core.Shared.Word.Models;
using Ngan.Dict.Core.Shared.Word.Models.Dto;
using Ngan.Dict.Core.Shared.Word.Models.Learn_;
using Ngan.Dict.Core.Shared.Word.Models.Po.Kv;
using Ngan.Dict.Core.Shared.Word.Models.Po.Learn;
using Ngan.Dict.Core.Shared.Word.Models.Po.Word;
using Ngan.Dict.Core.Word.Models.Po.Word;
using Tsinswreng.CsCtx;
using Tsinswreng.CsPage;
using Tsinswreng.CsSql;
using Tsinswreng.CsTextWithBlob;

namespace Ngan.Dict.Core.Shared.Word.Svc;

[Doc(@$"
Word Service Version 2. for {nameof(JnWord)}
the old version is {nameof(ISvcWord)}, for it uses the pattern of FnXxx and returns inner function
and did not support well for batch operations.

一切涉及修改{nameof(JnWord)}的操作都要修改其 {nameof(PoWord.BizUpdatedAt)}
```cs
RepoWord.AsAppRepo().BatBizTouch()
```
")]
public partial interface ISvcWordV2{

	[Doc(@$"
	獲取全部待學習單詞。
	會被當前學習方案定義的預篩選器篩選。
	如果未定義學習方案 則返回用戶詞庫的所有單詞
	#See[{nameof(PreFilter)}]
	#See[{nameof(KeysClientKv.CurStudyPlanId)}]
	")]
	public IAsyncEnumerable<JnWord> GetWordsToLearn(
		IDbUserCtx Ctx, CT Ct
	);
	
	// public IAsyncEnumerable<JnWord> GetWordsToLearn(
	// 	IFnCtx Ctx, CT Ct
	// );

	[Doc(@$"
	獲取全部待學習單詞。
	會被當前學習方案定義的預篩選器篩選。
	如果未定義篩選器 則返回用戶詞庫的所有單詞
	#See[{nameof(KeysClientKv.CurStudyPlanId)}]
	")]
	public IAsyncEnumerable<JnWord> GetWordsToLearn(
		IDbUserCtx Ctx, PreFilter? Prefilter, CT Ct
	);
	
	[Doc(@$"批量爲單詞插入新的學習記錄、並更新{nameof(PoWord.BizUpdatedAt)}。
	用于背單詞後 儲存學習結果
	")]
	public Task<nil> OrdAddNewLearnRecord(
		IDbUserCtx Ctx
		,IAsyncEnumerable<PoWordLearn> PoWordLearnAsyE, CT Ct
	);
	

	
	[Doc(@$"先把每個元素的Owner改成和Ctx中的用戶一致。
	然後用{nameof(IRepo<,>.OrdAddAgg)}。
	#Throw[{nameof(KeysErr.Common.DataIllegalOrConflict)}][
		添加失敗時即拋。不做合併等處理
	]
	")]
	public Task<nil> OrdAddJnWord(
		IDbUserCtx Ctx,
		IAsyncEnumerable<JnWord> Words, CT Ct
	);
	
	[Doc(@$"軟刪 整ʹ單詞 含附屬資產亦需被標爲軟刪。
	用戶刪除單詞旹 當用 {nameof(SoftDelPoWordInId)} 洏非斯函數、
	緣: 當 先刪一部分資產後刪單詞 後又欲復(撤銷刪除)旹、無法辨。
	")]
	public Task<nil> SoftDelJnWordInId(
		IDbUserCtx Ctx,
		IAsyncEnumerable<IdWord> Ids, CT Ct
	);
	
	[Doc(@$"只軟刪聚合根、不刪資產。
	用戶刪除單詞旹 當用 斯函數 洏非{nameof(SoftDelPoWordInId)}。
	")]
	public Task<nil> SoftDelPoWordInId(
		IDbUserCtx Ctx,
		IAsyncEnumerable<IdWord> Ids, CT Ct
	);
	
	[Doc(@$"在{nameof(PoWord)}, {nameof(PoWordProp)}, {nameof(PoWordLearn)}三個表中
	把已被軟刪除的實體真正從數據庫中刪除。
	只刪當前用戶的數據。
	可能導致孤兒資產
	")]
	public Task<nil> HardDelSoftDeleted(IDbUserCtx Ctx, CT Ct);
	
	[Doc(@$"大模型詞典 轉 用戶單詞。
	{nameof(PoWord.Lang)} : {nameof(ISvcNormLangToUserLang.GetUserLangByNormLang)}。
	不額外添加 {nameof(JnWord.Learns)}。
	#Throw[{nameof(KeysErr.Word.NormLangToUserLangIsNotMapped)}][
		用戶未配置映射
	]
	")]
	public Task<JnWord> LlmDictWordToJnWord(
		IDbUserCtx Ctx
		,IReqLlmDict Req
		,IRespLlmDict LlmDict, CT Ct
	);
	
	[Doc(@$"在{nameof(LlmDictWordToJnWord)}基礎上、
	爲 {nameof(JnWord.Learns)}添加一項 {nameof(ELearn.Add)}。
	時間設爲當前時間。
	")]
	public Task<JnWord> LlmDictWordToJnWordWithLearn(
		IDbUserCtx Ctx
		,IReqLlmDict Req
		,IRespLlmDict LlmDict, CT Ct
	);
	
	
	public Task<nil> OrdAddWordProp(
		IDbUserCtx Ctx, IAsyncEnumerable<PoWordProp> WordProps, CT Ct
	);
	
	public Task<nil> OrdUpdWordProp(
		IDbUserCtx Ctx, IAsyncEnumerable<PoWordProp> WordProps, CT Ct
	);
	
	public Task<nil> OrdAddWordLearn(
		IDbUserCtx Ctx, IAsyncEnumerable<PoWordLearn> WordLearns, CT Ct
	);
	
	public Task<nil> OrdUpdWordLearn(
		IDbUserCtx Ctx, IAsyncEnumerable<PoWordLearn> WordLearns, CT Ct
	);
	
	public Task<nil> DelWordPropInId(
		IDbUserCtx Ctx, IAsyncEnumerable<IdWordProp> Ids, CT Ct
	);
	
	
	public Task<nil> DelWordLearnInId(
		IDbUserCtx Ctx, IAsyncEnumerable<IdWordLearn> Ids, CT Ct
	);
	
	[Doc(@$"
	先用傳入的{nameof(PoWord.Id)} 去庫裏查單詞、設查得的單詞爲 Old
	
	若Old和傳入的{nameof(PoWord.Id)},{nameof(PoWord.Head)},{nameof(PoWord.Lang)}都相同、
	則直接更新其他不同字段、{nameof(RespUpdPoWord.HasUpdatedBizId)}=false
	
	若 Old和New 的 ({nameof(PoWord.Head)},{nameof(PoWord.Lang)})不同、
	就先調{nameof(OrdUpdHeadLang)}、再以返回的Id爲基準、更新其他字段
	#Rtn[更新結果Dto。其{nameof(RespUpdPoWord.FinalId)}一定有值]
	")]
	public Task<IAsyncEnumerable<RespUpdPoWord>> OrdUpdPoWord(
		IDbUserCtx Ctx, IAsyncEnumerable<PoWord> PoWords, CT Ct
	);
	
	[Doc(@$"更新單詞Id。
	此函數一般用于同步旹 {nameof(EDiffByBizIdResultForSync.IdNotEqual)} 按Id更小者爲準。
	(函數實現規則是把{nameof(Ids)}的Old改成New、上面只是說用途、不代表此函數的實現規則是按Id更小)
	")]
	public Task<nil> OrdChangeId(
		IDbUserCtx Ctx, IAsyncEnumerable<(IdWord Old, IdWord New)> Ids, CT Ct
	);
	
	
	[Doc(@$"
	取單詞合併結果。 不實際寫入數據庫。
	#See[{nameof(ISvcWordInMem.Merge)}]
	")]
	public IAsyncEnumerable<IJnWordMergeResult> GetWordMergeResult(
		IDbUserCtx Ctx, IAsyncEnumerable<JnWord> Words, CT Ct
	);
	
	[Doc(@$"
	合併單詞。會實際寫入數據庫
	#See[{nameof(ISvcWordInMem.Merge)}]
	")]
	public Task<nil> MergeWord(
		IDbUserCtx Ctx, IAsyncEnumerable<IJnWordMergeResult> Words, CT Ct
	);
	
	[Doc(@$"合併單詞。會實際寫入數據庫。
	#See[{nameof(GetWordMergeResult)}]
	")]
	public Task<nil> MergeWord(
		IDbUserCtx Ctx, IAsyncEnumerable<JnWord> Words, CT Ct
	);
	
	
	[Doc(@$"合併單詞。會實際寫入數據庫。
	
	其中、Remote比Local多出來的 {nameof(KeysProp.description)}
	會被當作 {nameof(PoWordLearn)}、事件爲 {nameof(ELearn.Add)}、
	{nameof(PoWordLearn.BizCreatedAt)} 與原 Prop 一致。
	
	用于從生詞表文本添加生詞
	
	#See[{nameof(GetWordMergeResult)}]
	")]
	public Task<nil> MergeWord_NewDescrAsAdd(
		IDbUserCtx Ctx, IAsyncEnumerable<JnWord> Words, CT Ct
	);


	
	[Doc(@$"
	BizId即({nameof(PoWord.Head)},{nameof(PoWord.Lang)})。
	
	對入參中每項(Arg)、先用Arg.Id 查得數據庫中己存之實體 WordOfId;
	若WordOfId不存在則拋{nameof(KeysErr.Word.WordOfId__NotFound)}
	若WordOfId存在但Owner不同則拋{nameof(KeysErr.Common.PermissionDenied)};
	
	若WordOfId存在且WordOfId的BizId與Arg的相同則不管({nameof(EUpdBizIdResult.BizIdAlreadyEqual)});
	
	若WordOfId存在且WordOfId的BizId與Arg不同
	[
		先用WordOfId.BizId查庫得到 WordOfHeadLang ;
		if WordOfHeadLang 已被軟刪除 先取消他的軟刪除狀態;
		if WordOfHeadLang is null [ 即 作爲更改目標的BizId不存在,更改不衝突({nameof(EUpdBizIdResult.DataOfBizIdNotExist)})
			直接把 WordOfId 的 BizId 改成 Arg的 BizId ;
			改 業務更新時間;
		]else[ 即 WordOfHeadLang 不爲空 作爲更改目標的BizId已存在,更改衝突({nameof(EUpdBizIdResult.BizIdNotEqual)})
			把 WordOfId 設成已軟刪除;
			把WordOfId的資產({nameof(JnWord.Props)},{nameof(JnWord.Learns)})
			算成 WordOfHeadLang 的資產(改{nameof(I_WordId.WordId)}外鍵、爲移動洏非複製);改完之後WordOfId不再有任何資產
			更改有變動的實體的{nameof(PoWord.BizUpdatedAt)}。(資產實體不需要改更新時間、因爲 只是外鍵變了 內容沒變)
			設 返值Dto的{nameof(RespUpdBizId.FinalId)}爲 WordOfHeadLang.Id
		]
	]
	#Rtn[同位置的每個元素與入參一一對應、
	返回的Id都是最終基準Id
	]
	")]
	public IAsyncEnumerable<RespUpdBizId> OrdUpdHeadLang(IDbUserCtx Ctx, IAsyncEnumerable<PoWord> PoWords, CT Ct);

	[Doc(@$"
		把{nameof(JnWords)}合入數據庫。
		BizId指業務層面之唯一標識、而非Id字段
		
		對每個元素:
		先去數據庫中 按({nameof(PoWord.Owner)},{nameof(PoWord.Head)},{nameof(PoWord.Lang)})
		查到舊詞Local(可能爲null);
		
		然後調用{nameof(ISvcWordInMem.SyncJnWord)}。
		然後調用{nameof(OrdSyncByDto)}。
		#Rtn[亦返{nameof(DtoJnWordSyncResult)} 便于審計等]
	")]
	public IAsyncEnumerable<DtoJnWordSyncResult> OrdSyncJnWordByBizId(
		IDbUserCtx Ctx, IAsyncEnumerable<JnWord> JnWords, CT Ct
	);
	
	[Doc(@$"
	#See[{nameof(OrdSyncJnWordByBizId)}]
	#See[{nameof(UnpackJnWords)}]
	")]
	public IAsyncEnumerable<DtoJnWordSyncResult> OrdSyncJnWordByBizIdFromStream(
		IDbUserCtx Ctx, Stream TextWithStream, CT Ct
	);
	
	[Doc(@$"調用{nameof(ISvcWordSync)}裏面的API來實現。
	
	")]
	public Task<nil> OrdSyncByDto(
		IDbUserCtx Ctx,
		IAsyncEnumerable<DtoJnWordSyncResult> Dtos, CT Ct
	);
	
	public IAsyncEnumerable<JnWord> GetAllWordWithDel(
		IDbUserCtx Ctx, CT Ct
	);
	
	[Doc(@$"
	#See[{nameof(IPacker<>)}]
	#Rtn[{nameof(ITextWithStream)}, {nameof(ExtnTextWithStream.ToStream)}]
	")]
	public Task<Stream> PackAllWordWithDel(
		IDbUserCtx Ctx, CT Ct
	);
	
	[Doc(@$"
	#Params([{nameof(ITextWithStream)}])
	")]
	public IAsyncEnumerable<JnWord> UnpackJnWords(
		Stream TextWithStream, CT Ct
	);

	[Doc($$"""
	分頁搜索用戶詞庫。
	返回強類型命中結果 {{nameof(DtoWordSearchHit)}}，
	避免把“命中的是單詞本身還是某條資產”這類信息丟失。

	搜索規則：
	- 優先級固定爲: Id匹配 > Head精準 > Head模糊。
	- 若 {{nameof(ReqSearchWord.RawStr)}} 可解析爲
		{{nameof(IdWord)}} / {{nameof(IdWordProp)}} / {{nameof(IdWordLearn)}}，
		則按 Id匹配 處理。
	- 若未按 Id 命中，則再按 {{nameof(PoWord.Head)}} 精準匹配處理。
	- 若未按 {{nameof(PoWord.Head)}} 精準命中，則最後再做 {{nameof(PoWord.Head)}} 模糊匹配。
	- 只返回未軟刪除的單詞聚合，避免已刪數據回流到搜索結果。

	返回約定：
	- {{nameof(DtoWordSearchHit.JnWord)}} 始終有值，表示命中結果所屬的完整單詞聚合。
	- {{nameof(DtoWordSearchHit.HitKind)}} 用于指示命中來源。
	- 當命中屬性或學習記錄時，應在
		{{nameof(DtoWordSearchHit.WordProp)}} 或 {{nameof(DtoWordSearchHit.WordLearn)}}
		中保留精確命中的資產。
	
	其中{{nameof(DtoWordSearchHit.JnWord)}}包含 已被軟刪者、
	但每個{{nameof(DtoWordSearchHit.JnWord)}}中的資產 不包含 已被軟刪者。
	""")]
	public Task<IPage<DtoWordSearchHit>> PageSearch(
		IDbUserCtx Ctx,
		IPageQry PageQry,
		ReqSearchWord Req,
		CT Ct
	);

	// -------- IList 批核心形狀(新範式)--------
	// 每寫方法兩個形狀:流式入口(IAsyncEnumerable、對外)= EnsureTxn 整流 + SqlFlow.Batches/BatchesInOnly 逐批調 IList 批核心;
	// IList 批核心(同名重載)= 函數只管一批,內部 EnsureTxn 可嵌套(有外層只加入、無外層自開自提交),單獨調用/測試亦安全。
	// 純讀(GetWordMergeResult)不開事務;出參形狀同步物化:純寫 Task<nil>、需回讀者 Task<IList<T>>。

	[Doc(@$"IList 版 `{nameof(OrdAddNewLearnRecord)}`:一批一次寫(整批一個事務)。
	#See[{nameof(ISvcWordV2.OrdAddNewLearnRecord)}]")]
	public Task<nil> OrdAddNewLearnRecord(
		IDbUserCtx Ctx
		,IList<PoWordLearn> WordLearns, CT Ct
	);

	[Doc(@$"IList 版 `{nameof(OrdAddJnWord)}`:一批一次寫(整批一個事務,先歸屬歸一化後整批 OrdAddAgg)。
	#See[{nameof(ISvcWordV2.OrdAddJnWord)}]")]
	public Task<nil> OrdAddJnWord(
		IDbUserCtx Ctx,
		IList<JnWord> Words, CT Ct
	);

	[Doc(@$"IList 版 `{nameof(SoftDelJnWordInId)}`:一批一次軟刪聚合(根+資產,整批一個事務)。
	#See[{nameof(ISvcWordV2.SoftDelJnWordInId)}]")]
	public Task<nil> SoftDelJnWordInId(
		IDbUserCtx Ctx,
		IList<IdWord> Ids, CT Ct
	);

	[Doc(@$"IList 版 `{nameof(SoftDelPoWordInId)}`:一批一次軟刪聚合根(整批一個事務)。
	#See[{nameof(ISvcWordV2.SoftDelPoWordInId)}]")]
	public Task<nil> SoftDelPoWordInId(
		IDbUserCtx Ctx,
		IList<IdWord> Ids, CT Ct
	);

	[Doc(@$"IList 版 `{nameof(OrdAddWordProp)}`:一批一次插入(整批一個事務)。
	#See[{nameof(ISvcWordV2.OrdAddWordProp)}]")]
	public Task<nil> OrdAddWordProp(
		IDbUserCtx Ctx, IList<PoWordProp> WordProps, CT Ct
	);

	[Doc(@$"IList 版 `{nameof(OrdUpdWordProp)}`:一批一次更新(整批一個事務)。
	#See[{nameof(ISvcWordV2.OrdUpdWordProp)}]")]
	public Task<nil> OrdUpdWordProp(
		IDbUserCtx Ctx, IList<PoWordProp> WordProps, CT Ct
	);

	[Doc(@$"IList 版 `{nameof(OrdAddWordLearn)}`:一批一次插入(整批一個事務)。
	#See[{nameof(ISvcWordV2.OrdAddWordLearn)}]")]
	public Task<nil> OrdAddWordLearn(
		IDbUserCtx Ctx, IList<PoWordLearn> WordLearns, CT Ct
	);

	[Doc(@$"IList 版 `{nameof(OrdUpdWordLearn)}`:一批一次更新(整批一個事務)。
	#See[{nameof(ISvcWordV2.OrdUpdWordLearn)}]")]
	public Task<nil> OrdUpdWordLearn(
		IDbUserCtx Ctx, IList<PoWordLearn> WordLearns, CT Ct
	);

	[Doc(@$"IList 版 `{nameof(DelWordPropInId)}`:一批一次軟刪(整批一個事務)。
	#See[{nameof(ISvcWordV2.DelWordPropInId)}]")]
	public Task<nil> DelWordPropInId(
		IDbUserCtx Ctx, IList<IdWordProp> Ids, CT Ct
	);

	[Doc(@$"IList 版 `{nameof(DelWordLearnInId)}`:一批一次軟刪(整批一個事務)。
	#See[{nameof(ISvcWordV2.DelWordLearnInId)}]")]
	public Task<nil> DelWordLearnInId(
		IDbUserCtx Ctx, IList<IdWordLearn> Ids, CT Ct
	);

	[Doc(@$"IList 版 `{nameof(OrdUpdPoWord)}`:一批一次處理(整批一個事務)。
	出參 IList 與入參位置一一對應、{nameof(RespUpdPoWord.FinalId)} 一定有值。
	#See[{nameof(ISvcWordV2.OrdUpdPoWord)}]")]
	public Task<IList<RespUpdPoWord>> OrdUpdPoWord(
		IDbUserCtx Ctx, IList<PoWord> PoWords, CT Ct
	);

	[Doc(@$"IList 版 `{nameof(OrdChangeId)}`:一批一次改主鍵(整批一個事務,含資產外鍵遷移)。
	#See[{nameof(ISvcWordV2.OrdChangeId)}]")]
	public Task<nil> OrdChangeId(
		IDbUserCtx Ctx, IList<(IdWord Old, IdWord New)> Ids, CT Ct
	);

	[Doc(@$"IList 版 `{nameof(GetWordMergeResult)}`:純讀、不落庫、無事務,一批一次計算。
	出參 IList 與入參位置一一對應。
	#See[{nameof(ISvcWordV2.GetWordMergeResult)}]")]
	public Task<IList<IJnWordMergeResult>> GetWordMergeResult(
		IDbUserCtx Ctx, IList<JnWord> Words, CT Ct
	);

	[Doc(@$"IList 版 `{nameof(MergeWord)}`:一批一次合併入庫(整批一個事務)。
	#See[{nameof(ISvcWordV2.MergeWord)}]")]
	public Task<nil> MergeWord(
		IDbUserCtx Ctx, IList<IJnWordMergeResult> Words, CT Ct
	);

	[Doc(@$"IList 版 `{nameof(MergeWord)}`(入參 JnWord):一批一次合併入庫(整批一個事務)。
	#See[{nameof(ISvcWordV2.MergeWord)}]")]
	public Task<nil> MergeWord(
		IDbUserCtx Ctx, IList<JnWord> Words, CT Ct
	);

	[Doc(@$"IList 版 `{nameof(MergeWord_NewDescrAsAdd)}`:一批一次合併入庫(整批一個事務)。
	#See[{nameof(ISvcWordV2.MergeWord_NewDescrAsAdd)}]")]
	public Task<nil> MergeWord_NewDescrAsAdd(
		IDbUserCtx Ctx, IList<JnWord> Words, CT Ct
	);

	[Doc(@$"IList 版 `{nameof(OrdUpdHeadLang)}`:一批一次處理(整批一個事務)。
	出參 IList 與入參位置一一對應、返回的 Id 都是最終基準 Id。
	#See[{nameof(ISvcWordV2.OrdUpdHeadLang)}]")]
	public Task<IList<RespUpdBizId>> OrdUpdHeadLang(
		IDbUserCtx Ctx, IList<PoWord> PoWords, CT Ct
	);

	[Doc(@$"IList 版 `{nameof(OrdSyncJnWordByBizId)}`:一批一次同步(整批一個事務)。
	出參 IList 與入參位置一一對應、便於審計。
	#See[{nameof(ISvcWordV2.OrdSyncJnWordByBizId)}]")]
	public Task<IList<DtoJnWordSyncResult>> OrdSyncJnWordByBizId(
		IDbUserCtx Ctx, IList<JnWord> JnWords, CT Ct
	);

	[Doc(@$"IList 版 `{nameof(OrdSyncByDto)}`:一批一次分桶派發(整批一個事務)。
	#See[{nameof(ISvcWordV2.OrdSyncByDto)}]
	#See[{nameof(ISvcWordSync)}]")]
	public Task<nil> OrdSyncByDto(
		IDbUserCtx Ctx,
		IList<DtoJnWordSyncResult> Dtos, CT Ct
	);

}

[Doc(@$"此接口下函數不作爲公共API。
只是把同步單詞時 按 遇到的不同分類 分成不同函數處理、方便測試
")]
public partial interface ISvcWordSync{
	[Doc(@$"{nameof(EDiffByBizIdResultForSync.NoChange)}")]
	public Task<nil> OrdSync_NoChange(
		IDbUserCtx Ctx,
		IAsyncEnumerable<DtoJnWordSyncResult> Dtos, CT Ct
	);
	
	[Doc(@$"{nameof(EDiffByBizIdResultForSync.RemoteIsOlder)}")]
	public Task<nil> OrdSync_RemoteIsOlder(
		IDbUserCtx Ctx,
		IAsyncEnumerable<DtoJnWordSyncResult> Dtos, CT Ct
	);
	
	[Doc(@$"{nameof(EDiffByBizIdResultForSync.LocalNotExist)}")]
	public Task<nil> OrdSync_LocalNotExist(
		IDbUserCtx Ctx,
		IAsyncEnumerable<DtoJnWordSyncResult> Dtos, CT Ct
	);
	
	[Doc(@$"{nameof(EDiffByBizIdResultForSync.IdNotEqual)}")]
	public Task<nil> OrdSync_IdNotEqual(
		IDbUserCtx Ctx,
		IAsyncEnumerable<DtoJnWordSyncResult> Dtos, CT Ct
	);

	// -------- IList 批核心形狀(新範式)--------
	// 各分支直接吃已物化的批、不再內部切批;事務邊界由調用方(OrdSyncByDto IList 版)聲明,分支只加入不另開。

	[Doc(@$"{nameof(EDiffByBizIdResultForSync.NoChange)}:兩端一致、無任何寫。")]
	public Task<nil> OrdSync_NoChange(
		IDbUserCtx Ctx, IList<DtoJnWordSyncResult> Dtos, CT Ct
	);

	[Doc(@$"{nameof(EDiffByBizIdResultForSync.RemoteIsOlder)}:Local 較新、保持不動。")]
	public Task<nil> OrdSync_RemoteIsOlder(
		IDbUserCtx Ctx, IList<DtoJnWordSyncResult> Dtos, CT Ct
	);

	[Doc(@$"{nameof(EDiffByBizIdResultForSync.LocalNotExist)}:本用戶詞庫無此 BizId、直接整聚合插入。")]
	public Task<nil> OrdSync_LocalNotExist(
		IDbUserCtx Ctx, IList<DtoJnWordSyncResult> Dtos, CT Ct
	);

	[Doc(@$"{nameof(EDiffByBizIdResultForSync.IdNotEqual)}:BizId 同而 Id 不同,以 Id 更小者為基準收斂(遷主鍵+資產外鍵)。")]
	public Task<nil> OrdSync_IdNotEqual(
		IDbUserCtx Ctx, IList<DtoJnWordSyncResult> Dtos, CT Ct
	);

}
