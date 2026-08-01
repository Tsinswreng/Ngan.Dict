namespace Ngaq.Core.Infra.Errors;

using K = Tsinswreng.CsErr.IErrNode;
using static Tsinswreng.CsErr.ErrNode;
using Ngaq.Core.Shared.Word.Models.Po.NormLangToUserLang;

[Doc(@$"異常項
內部類按領域劃分
標識符命名 約定 __ 潙參數佔位符
- {nameof(MkB)} 業務異常
")]
public static class KeysErr{
	public static class Common{
		public static K _R = Mk(null, [nameof(Common)]);
		public static K ArgErr = MkB(_R, [nameof(ArgErr)]);
		public static K NetWorkErr = MkB(_R, [nameof(NetWorkErr)]);
		public static K UnknownErr = MkB(_R, [nameof(UnknownErr)]);
		public static K AddFailed = MkB(_R, [nameof(AddFailed)]);
		public static K PermissionDenied = MkB(_R, [nameof(PermissionDenied)]);
		[Doc(@$"數據不合法或衝突")]
		public static K DataIllegalOrConflict = MkB(_R, [nameof(PermissionDenied)]);
	}
	public class Audio{
		public static K _R = Mk(null, [nameof(Audio)]);
		[Doc(@$"已取得音頻數據 但 播放失敗")]
		public static K AudioPlayFailed = MkB(_R, [nameof(AudioPlayFailed)]);
	}
	public class User{
		public static K _R = Mk(null, [nameof(User)]);
		[Doc(@$"慎用、防止用戶枚舉漏洞")]
		public static K UserNotExist = MkB(_R, [nameof(UserNotExist)]);
		[Doc(@$"慎用、防止用戶枚舉漏洞")]
		public static K UserAlreadyExist = MkB(_R, [nameof(UserAlreadyExist)]);
		public static K PasswordNotMatch = MkB(_R, [nameof(PasswordNotMatch)]);
		public static K InvalidToken = MkB(_R, [nameof(InvalidToken)]);
		public static K TokenExpired = MkB(_R, [nameof(TokenExpired)]);
		public static K AuthenticationFailed = MkB(_R, [nameof(AuthenticationFailed)]);
	}
	public class Word{
		public static K _R = Mk(null, [nameof(Word)]);
		public static K WordOfId__NotFound = MkB(_R, [nameof(WordOfId__NotFound)]);
		public static K LoadWordListFailed = MkB(_R, [nameof(LoadWordListFailed)]);
		public static K SaveWordListFailed = MkB(_R, [nameof(SaveWordListFailed)]);
		public static K LoadWordCalcWeightFailed = MkB(_R, [nameof(LoadWordCalcWeightFailed)]);
		public static K StartLearnWithWeightCalcFailed = MkB(_R, [nameof(StartLearnWithWeightCalcFailed)]);
		public static K WeightCalcGetStudyPlanFailed = MkB(_R, [nameof(WeightCalcGetStudyPlanFailed)]);
		public static K WeightCalcInvalidAlgorithm = MkB(_R, [nameof(WeightCalcInvalidAlgorithm)]);
		public static K WeightCalcRunFailed = MkB(_R, [nameof(WeightCalcRunFailed)]);
		public static K WeightCalcResultHandleFailed = MkB(_R, [nameof(WeightCalcResultHandleFailed)]);
		public static K WeightCalcResultStreamNull = MkB(_R, [nameof(WeightCalcResultStreamNull)]);
		public static K WeightCalcResultWordIdInvalid__ = MkB(_R, [nameof(WeightCalcResultWordIdInvalid__)]);
		public static K JsWeightCalcCodeEmpty = MkB(_R, [nameof(JsWeightCalcCodeEmpty)]);
		public static K JsWeightCalcExecFailed = MkB(_R, [nameof(JsWeightCalcExecFailed)]);
		public static K JsWeightCalcReturnedEmpty = MkB(_R, [nameof(JsWeightCalcReturnedEmpty)]);
		public static K JsWeightCalcReturnedInvalidJson = MkB(_R, [nameof(JsWeightCalcReturnedInvalidJson)]);
		public static K BuiltinWeightCalcArgParseFailed = MkB(_R, [nameof(BuiltinWeightCalcArgParseFailed)]);
		public static K BuiltinWeightCalcExecFailed = MkB(_R, [nameof(BuiltinWeightCalcExecFailed)]);
		public static K __NotBelongToLang__ = MkB(_R, [nameof(__NotBelongToLang__)]);
		public static K __And__IsNotSameUserWord = MkB(_R, [nameof(__And__IsNotSameUserWord)]);
		public static K BackgroundImageServiceFailedToInit = MkB(_R, [nameof(BackgroundImageServiceFailedToInit)]);
		[Doc(@$"{nameof(PoNormLangToUserLang)}未設置。")]
		public static K NormLangToUserLangIsNotMapped = MkB(_R, [nameof(NormLangToUserLangIsNotMapped)]);
		
		[Doc(@$"單詞同步失敗。
		參數填入 WordId字符串。
		")]
		public static K Word__And__SyncFailed = MkB(_R, [nameof(Word__And__SyncFailed)]);
	}
	public class StudyPlan{
		public static K _R = Mk(null, [nameof(StudyPlan)]);
		[Doc(@$"佔位符: [1]->褈複的UniqName")]
		[Obsolete(@$"實現複雜。直接用 {nameof(Common.DataIllegalOrConflict)}")]
		public static K UniqNameDuplicated__ = MkB(_R, [nameof(UniqNameDuplicated__)]);
		
		[Obsolete(@$"直接用 {nameof(Common.DataIllegalOrConflict)}")]
		public static K AddFailedDataMayConflict = MkB(_R, [nameof(AddFailedDataMayConflict)]);
		[Obsolete(@$"直接用 {nameof(Common.DataIllegalOrConflict)}")]
		public static K UpdateFailedDataMayConflict = MkB(_R, [nameof(UpdateFailedDataMayConflict)]);
	}

	/// 字典服務相關異常
	public class Dictionary{
		public static K _R = Mk(null, [nameof(Dictionary)]);
		/// LLM 字典 API 未配置（URL 或 Key 缺失）
		public static K LlmApiNotConfigured = MkB(_R, [nameof(LlmApiNotConfigured)]);
		/// LLM API 返回空響應
		public static K LlmApiEmptyResponse = MkB(_R, [nameof(LlmApiEmptyResponse)]);
		/// LLM API 響應結構無效
		public static K LlmApiInvalidResponseStructure = MkB(_R, [nameof(LlmApiInvalidResponseStructure)]);
		/// LLM API 返回空內容
		public static K LlmApiEmptyContent = MkB(_R, [nameof(LlmApiEmptyContent)]);
		/// 解析 LLM 響應失敗
		public static K LlmResponseParseFailed = MkB(_R, [nameof(LlmResponseParseFailed)]);
	}
	public class Sync{
		public static K _R = Mk(null, [nameof(Sync)]);
		public static K BatchGetByIdCountMismatch = MkB(_R, [nameof(BatchGetByIdCountMismatch)]);
		public static K SyncShouldUseSameId = MkB(_R, [nameof(SyncShouldUseSameId)]);
	}
}
