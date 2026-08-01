namespace Ngaq.Core.Shared.Word.Svc;

using Ngaq.Core.Model.Po.Learn_;
using Ngaq.Core.Tools.Io;
using Ngaq.Core.Iter;
using Tsinswreng.CsPage;
using Tsinswreng.CsTools;
using Ngaq.Core.Shared.User.UserCtx;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Core.Tools;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Tsinswreng.CsTempus;

file class DirDoc{
	str Doc =
$$"""
#Sum[

]
#Descr[

]
""";
}

[Obsolete]
public partial interface ISvcWord{
//TODO 加詞後 宜予回饋 如 新ʹ加ʹ詞ʹ數 及 老詞新加之數
	public Task<nil> AddWordsFromFilePath(
		IUserCtx UserCtx
		,Path_Encode Path_Encode
		,CT Ct
	);

	[Doc(@$"
	從生詞表中加入單詞
	遇到({nameof(PoWord.Head)}, {nameof(PoWord.Lang)})相同的 即合併
	會新增{nameof(ELearn.Add)}、
	按 新詞的總共的 新來的 {nameof(KeysProp.description)} 的數量
	決定 新增的{nameof(ELearn.Add)}的數量
	")]
	public Task<nil> AddWordsFromText(
		IUserCtx UserCtx
		,string Text
		,CT Ct
	);

	[Doc(@$"不新增{nameof(ELearn.Add)}")]
	public Task<nil> AddEtMergeWords(
		IUserCtx UserCtx
		,IEnumerable<IJnWord> JnWords
		,CT Ct
	);

	[Doc(@$"非遊標分頁。
	當前 此函數ʹ用途 只有 在背單詞旹 獲取全部單詞、
	故宜棄用此洏另置接口
	")]
	public Task<IPageAsyE<IJnWord>> PageWord(
		IUserCtx UserCtx
		,IPageQry PageQry
		,CT Ct
	);


	[Doc(@$"批量爲單詞插入新的學習記錄、並更新{nameof(PoWord.BizUpdatedAt)}。
	用于背單詞後 儲存學習結果
	")]
	public Task<nil> AddWordId_LearnRecordss(
		IUserCtx UserCtx
		,IEnumerable<WordId_LearnRecords> WordId_LearnRecordss
		,CT Ct
	);

	[Doc($$"""
	從多個獨立的 {{nameof(JnWord)}} json字符串 添加單詞 ???
	#Params(
		[],
		[每個元素都是一個獨立的 {{nameof(JnWord)}} json字符串],
		[],
	)
	""")]
	public Task<nil> AddWordsByJsonLineIter(
		IUserCtx User
		,IAsyncEnumerable<str> JsonLineIter
		,CT Ct
	);

	
	[Doc(@$"軟刪、只設聚合根之{nameof(PoWord.DelAt)}、不改他ʹ資產")]
	public Task<nil> SoftDelJnWordsByIds(
		IUserCtx User
		,IEnumerable<IdWord> Ids
		,CT Ct
	);

	public Task<nil> UpdJnWord(IUserCtx User, IJnWord JnWord, CT Ct);

//test
	public Task<IPage<IJnWord>> PageChangedWordsWithDelWordsAfterTime(
		IUserCtx User
		,IPageQry PageQry
		,UnixMs Tempus
		,CT Ct
	);

	public Task<IPage<IJnWord>> SearchWord(
		IUserCtx User
		,IPageQry PageQry
		,ReqSearchWord Req
		,CT Ct
	);

	[Obsolete]
	public Task<IPage<ITypedObj>> PageSearch(
		IUserCtx User
		,IPageQry PageQry
		,ReqSearchWord Req
		,CT Ct
	);

	public Task<DtoCompressedWords> ZipAllWordsJson(IUserCtx User, ReqPackWords ReqPackWords, CT Ct);

	public Task<nil> SyncCompressedWord(IUserCtx User, DtoCompressedWords Dto, CT Ct);

	public Task<NgaqTextWithBlob> PackAllWordsToTextWithBlobNoStream(IUserCtx User, ReqPackWords Req, CT Ct);

	public Task<nil> SyncFromTextWithBlob(IUserCtx User, NgaqTextWithBlob TextWithBlob, CT Ct);

	[Doc("用于統計")]
	public Task<RespScltWordsOfLearnResultByTimeInterval> ScltAddedWordsByTimeInterval(
		ReqScltWordsOfLearnResultByTimeInterval Req
		,CT Ct
	);

}


