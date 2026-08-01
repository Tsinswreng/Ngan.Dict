using Ngan.Dict.Core.Frontend.ImgBg;
using Ngan.Dict.Core.Infra;
using Ngan.Dict.Core.Model.Po.Kv;
using Ngan.Dict.Core.Model.Po.Learn_;
using Ngan.Dict.Core.Shared.Audio;
using Ngan.Dict.Core.Shared.Dictionary.Models;
using Ngan.Dict.Core.Shared.Dictionary.Models.Po.NormLang;
using Ngan.Dict.Core.Shared.Dictionary.Svc;
using Ngan.Dict.Core.Shared.StudyPlan.Models;
using Ngan.Dict.Core.Shared.StudyPlan.Models.Po.PreFilter;
using Ngan.Dict.Core.Shared.StudyPlan.Models.Po.StudyPlan;
using Ngan.Dict.Core.Shared.StudyPlan.Models.Po.WeightArg;
using Ngan.Dict.Core.Shared.StudyPlan.Models.Po.WeightCalculator;
using Ngan.Dict.Core.Shared.StudyPlan.Models.PreFilter;
using Ngan.Dict.Core.Shared.StudyPlan.Models.Req;
using Ngan.Dict.Core.Shared.StudyPlan.Svc;
using Ngan.Dict.Core.Shared.User.UserCtx;
using Ngan.Dict.Core.Shared.Word.Models;
using Ngan.Dict.Core.Shared.Word.Models.Dto;
using Ngan.Dict.Core.Shared.Word.Models.Po.Kv;
using Ngan.Dict.Core.Shared.Word.Models.Po.Learn;
using Ngan.Dict.Core.Shared.Word.Models.Po.UserLang;
using Ngan.Dict.Core.Shared.Word.Models.Po.Word;
using Ngan.Dict.Core.Shared.Word.Svc;
using Ngan.Dict.Core.Tools;
using Ngan.Dict.Core.Tools.Io;
using Ngan.Dict.Core.Word.Svc;
using Tsinswreng.CsPage;
using Tsinswreng.CsTempus;
using Tsinswreng.CsTools;

namespace Ngan.Dict.Client.Mock;

public class MockSvcDictionary : ISvcDictionary {
	public Task<string> GetLlmDictSysPromptOrDflt(IDbUserCtx Ctx, CT Ct) {
		return Task.FromResult("");
	}

	public Task<PoNormLang?> GetCurSrcNormLang(IDbUserCtx Ctx, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<PoNormLang?> GetCurTgtNormLang(IDbUserCtx Ctx, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<IRespLlmDict> Lookup(IUserCtx User, IReqLlmDict Req, CT Ct) {
		throw new NotImplementedException();
	}

	public IRespLlmDict ParseRawOutput(string RawOutput) {
		throw new NotImplementedException();
	}

	public Task<PoNormLang?> SetCurSrcNormLang(IDbUserCtx Ctx, PoNormLang Po, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<string> SetLlmDictSysPrompt(IDbUserCtx Ctx, string Prompt, CT Ct) {
		return Task.FromResult(Prompt);
	}

	public Task<PoNormLang?> SetCurTgtNormLang(IDbUserCtx Ctx, PoNormLang Po, CT Ct) {
		throw new NotImplementedException();
	}
}


public class MockSvcWordV2 : ISvcWordV2 {
	public Task<object> OrdAddJnWord(IDbUserCtx Ctx, IAsyncEnumerable<JnWord> Words, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> OrdAddNewLearnRecord(IDbUserCtx Ctx, IAsyncEnumerable<PoWordLearn> PoWordLearnAsyE, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> OrdAddWordLearn(IDbUserCtx Ctx, IAsyncEnumerable<PoWordLearn> WordLearns, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> OrdAddWordProp(IDbUserCtx Ctx, IAsyncEnumerable<PoWordProp> WordProps, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> OrdChangeId(IDbUserCtx Ctx, IAsyncEnumerable<(IdWord Old, IdWord New)> Ids, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> OrdSyncByDto(IDbUserCtx Ctx, IAsyncEnumerable<DtoJnWordSyncResult> Dtos, CT Ct) {
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<DtoJnWordSyncResult> OrdSyncJnWordByBizId(IDbUserCtx Ctx, IAsyncEnumerable<JnWord> JnWords, CT Ct) {
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<DtoJnWordSyncResult> OrdSyncJnWordByBizIdFromStream(IDbUserCtx Ctx, Stream TextWithStream, CT Ct) {
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<RespUpdBizId> OrdUpdHeadLang(IDbUserCtx Ctx, IAsyncEnumerable<PoWord> PoWords, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<IAsyncEnumerable<RespUpdPoWord>> OrdUpdPoWord(IDbUserCtx Ctx, IAsyncEnumerable<PoWord> PoWords, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> OrdUpdWordLearn(IDbUserCtx Ctx, IAsyncEnumerable<PoWordLearn> WordLearns, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> OrdUpdWordProp(IDbUserCtx Ctx, IAsyncEnumerable<PoWordProp> WordProps, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> DelWordLearnInId(IDbUserCtx Ctx, IAsyncEnumerable<IdWordLearn> Ids, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> DelWordPropInId(IDbUserCtx Ctx, IAsyncEnumerable<IdWordProp> Ids, CT Ct) {
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<JnWord> GetAllWordWithDel(IDbUserCtx Ctx, CT Ct) {
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<IJnWordMergeResult> GetWordMergeResult(IDbUserCtx Ctx, IAsyncEnumerable<JnWord> Words, CT Ct) {
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<JnWord> GetWordsToLearn(IDbUserCtx Ctx, CT Ct) {
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<JnWord> GetWordsToLearn(IDbUserCtx Ctx, PreFilter? Prefilter, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> HardDelSoftDeleted(IDbUserCtx Ctx, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<JnWord> LlmDictWordToJnWord(IDbUserCtx Ctx, IReqLlmDict Req, IRespLlmDict LlmDict, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<JnWord> LlmDictWordToJnWordWithLearn(IDbUserCtx Ctx, IReqLlmDict Req, IRespLlmDict LlmDict, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> MergeWord(IDbUserCtx Ctx, IAsyncEnumerable<IJnWordMergeResult> Words, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> MergeWord(IDbUserCtx Ctx, IAsyncEnumerable<JnWord> Words, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> MergeWord_NewDescrAsAdd(IDbUserCtx Ctx, IAsyncEnumerable<JnWord> Words, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<Stream> PackAllWordWithDel(IDbUserCtx Ctx, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> SoftDelJnWordInId(IDbUserCtx Ctx, IAsyncEnumerable<IdWord> Ids, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> SoftDelPoWordInId(IDbUserCtx Ctx, IAsyncEnumerable<IdWord> Ids, CT Ct) {
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<JnWord> UnpackJnWords(Stream TextWithStream, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<IPage<DtoWordSearchHit>> PageSearch(IDbUserCtx Ctx, IPageQry PageQry, ReqSearchWord Req, CT Ct) {
		throw new NotImplementedException();
	}
}

public class MockSvcStudyPlan : ISvcStudyPlan, IStudyPlanGetter {
	public Task<object> OrdAddPreFilter(IDbUserCtx Ctx, IAsyncEnumerable<PoPreFilter> Pos, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> OrdAddStudyPlan(IDbUserCtx Ctx, IAsyncEnumerable<PoStudyPlan> Pos, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> OrdAddWeightArg(IDbUserCtx Ctx, IAsyncEnumerable<PoWeightArg> Pos, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> OrdAddWeightCalculator(IDbUserCtx Ctx, IAsyncEnumerable<PoWeightCalculator> Pos, CT Ct) {
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<PoPreFilter?> OrdGetPreFilterById(IDbUserCtx Ctx, IAsyncEnumerable<IdPreFilter> Ids, CT Ct) {
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<PoWeightArg?> OrdGetWeightArgById(IDbUserCtx Ctx, IAsyncEnumerable<IdWeightArg> Ids, CT Ct) {
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<PoWeightCalculator?> OrdGetWeightCalculatorById(IDbUserCtx Ctx, IAsyncEnumerable<IdWeightCalculator> Ids, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> OrdSoftDelPreFilter(IDbUserCtx Ctx, IAsyncEnumerable<PoPreFilter> Pos, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> OrdSoftDelStudyPlan(IDbUserCtx Ctx, IAsyncEnumerable<PoStudyPlan> Pos, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> OrdSoftDelWeightArg(IDbUserCtx Ctx, IAsyncEnumerable<PoWeightArg> Pos, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> OrdSoftDelWeightCalculator(IDbUserCtx Ctx, IAsyncEnumerable<PoWeightCalculator> Pos, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> OrdUpdPreFilter(IDbUserCtx Ctx, IAsyncEnumerable<PoPreFilter> Pos, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> OrdUpdStudyPlan(IDbUserCtx Ctx, IAsyncEnumerable<PoStudyPlan> Pos, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> OrdUpdWeightArg(IDbUserCtx Ctx, IAsyncEnumerable<PoWeightArg> Pos, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> OrdUpdWeightCalculator(IDbUserCtx Ctx, IAsyncEnumerable<PoWeightCalculator> Pos, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<bool> EnsureCurStudyPlan(IDbUserCtx Ctx, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<BoStudyPlan?> GetCurBoStudyPlan(IDbUserCtx Ctx, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<JnStudyPlan?> GetCurJnStudyPlan(IDbUserCtx Ctx, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<IdStudyPlan?> GetCurStudyPlanId(IDbUserCtx Ctx, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<IWeightCalctr?> GetCurWeightCalctr(IDbUserCtx Ctx, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<BoStudyPlan> GetDfltStudyPlan(IDbUserCtx Ctx, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<BoStudyPlan> GetStudyPlan(IUserCtx User, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<IPageAsyE<PoPreFilter>> PagePreFilter(IDbUserCtx Ctx, ReqPagePreFilter Req, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<IPageAsyE<PoStudyPlan>> PageStudyPlan(IDbUserCtx Ctx, ReqPageStudyPlan Req, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<IPageAsyE<PoWeightArg>> PageWeightArg(IDbUserCtx Ctx, ReqPageWeightArg Req, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<IPageAsyE<PoWeightCalculator>> PageWeightCalculator(IDbUserCtx Ctx, ReqPageWeightCalculator Req, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> RestoreBuiltinStudyPlan(IDbUserCtx Ctx, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> SetCurStudyPlanId(IDbUserCtx Ctx, IdStudyPlan IdStudyPlan, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> SyncPreFilter(IDbUserCtx Ctx, IAsyncEnumerable<PoPreFilter> Pos, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> SyncStudyPlan(IDbUserCtx Ctx, IAsyncEnumerable<PoStudyPlan> Pos, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> SyncWeightArg(IDbUserCtx Ctx, IAsyncEnumerable<PoWeightArg> Pos, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> SyncWeightCalculator(IDbUserCtx Ctx, IAsyncEnumerable<PoWeightCalculator> Pos, CT Ct) {
		throw new NotImplementedException();
	}
}

public class MockSvcNormLang : ISvcNormLang {
	public Task<object> OrdAddNormLang(IDbUserCtx Ctx, IAsyncEnumerable<PoNormLang> Pos, CT Ct) {
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<PoNormLang?> OrdGetNormLangByTypeCode(IDbUserCtx Ctx, IAsyncEnumerable<(ELangIdentType, string)> Type_Code, CT Ct) {
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<string?> OrdGetTranslatedName(IDbUserCtx Ctx, INormLang TargetLang, IAsyncEnumerable<INormLang> NormLangs, CT Ct) {
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<INormLangDetail> OrdGetUiLangs(CT CT) {
		throw new NotImplementedException();
	}

	public Task<object> OrdSoftDelNormLang(IDbUserCtx Ctx, IAsyncEnumerable<PoNormLang> Pos, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> OrdUpdNormLang(IDbUserCtx Ctx, IAsyncEnumerable<PoNormLang> Pos, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> InitBuiltinNormLang(IDbUserCtx Ctx, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<IPageAsyE<PoNormLang>> PageNormLang(IDbUserCtx Ctx, ReqPageNormLang Req, CT Ct) {
		throw new NotImplementedException();
	}
}

public class MockSvcTts : ISvcTts {
	public Task<Audio> GetAudio(string Text, INormLang Lang) {
		throw new NotImplementedException();
	}
}


public class MockAudioPlayer : IAudioPlayer {
	public Task<IPlayState?> Play(Stream S, EAudioType Type, CT Ct) {
		throw new NotImplementedException();
	}
}


public class MockSvcWord : ISvcWord {
	public Task<object> AddEtMergeWords(IUserCtx UserCtx, IEnumerable<IJnWord> JnWords, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> AddWordId_LearnRecordss(IUserCtx UserCtx, IEnumerable<WordId_LearnRecords> WordId_LearnRecordss, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> AddWordsByJsonLineIter(IUserCtx User, IAsyncEnumerable<string> JsonLineIter, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> AddWordsFromFilePath(IUserCtx UserCtx, Path_Encode Path_Encode, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> AddWordsFromText(IUserCtx UserCtx, string Text, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<NgaqTextWithBlob> PackAllWordsToTextWithBlobNoStream(IUserCtx User, ReqPackWords Req, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<IPage<IJnWord>> PageChangedWordsWithDelWordsAfterTime(IUserCtx User, IPageQry PageQry, UnixMs Tempus, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<IPage<ITypedObj>> PageSearch(IUserCtx User, IPageQry PageQry, ReqSearchWord Req, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<IPageAsyE<IJnWord>> PageWord(IUserCtx UserCtx, IPageQry PageQry, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<RespScltWordsOfLearnResultByTimeInterval> ScltAddedWordsByTimeInterval(ReqScltWordsOfLearnResultByTimeInterval Req, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<IPage<IJnWord>> SearchWord(IUserCtx User, IPageQry PageQry, ReqSearchWord Req, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> SoftDelJnWordsByIds(IUserCtx User, IEnumerable<IdWord> Ids, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> SyncCompressedWord(IUserCtx User, DtoCompressedWords Dto, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> SyncFromTextWithBlob(IUserCtx User, NgaqTextWithBlob TextWithBlob, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> UpdJnWord(IUserCtx User, IJnWord JnWord, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<DtoCompressedWords> ZipAllWordsJson(IUserCtx User, ReqPackWords ReqPackWords, CT Ct) {
		throw new NotImplementedException();
	}
}


public class MockImgGetter : IImgGetter {
	public IEnumerable<ITypedObj> GetN(ulong n) {
		throw new NotImplementedException();
	}
}


public class MockSvcUserLang : ISvcUserLang {
	public Task<object> AddAllUnregisteredUserLangs(IDbUserCtx Ctx, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> OrdAddUserLang(IDbUserCtx Ctx, IAsyncEnumerable<PoUserLang> Pos, CT Ct) {
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<PoUserLang?> OrdGetUserLang(IDbUserCtx Ctx, IAsyncEnumerable<string> UniqNames, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<object> OrdUpdUserLang(IDbUserCtx Ctx, IAsyncEnumerable<PoUserLang> Pos, CT Ct) {
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<string> GetUnregisteredUserLangs(IDbUserCtx Ctx, CT Ct) {
		throw new NotImplementedException();
	}

	public Task<IPageAsyE<PoUserLang>> PageUserLang(IDbUserCtx Ctx, ReqPageUserLang Req, CT Ct) {
		throw new NotImplementedException();
	}
}

