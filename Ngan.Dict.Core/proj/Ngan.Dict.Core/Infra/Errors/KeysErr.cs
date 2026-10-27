namespace Ngan.Dict.Core.Infra.Errors;

using K = Tsinswreng.CsErr.IErrNode;
using static Tsinswreng.CsErr.ErrNode;
using Ngan.Dict.Core.Shared.Word.Models.Po.NormLangToUserLang;

[Doc(@$"異常項
內部類按領域劃分
標識符命名 約定 __ 潙參數佔位符
- {nameof(Mk)} 業務異常（不帶標籤 ＝ 公開可見的業務異常，這是默認）
")]
public static class KeysErr{
	public static class Common{
		public static K _R = Mk(null, [nameof(Common)]);
		public static K ArgErr = Mk(_R, [nameof(ArgErr)]);
		public static K NetWorkErr = Mk(_R, [nameof(NetWorkErr)]);
		public static K UnknownErr = Mk(_R, [nameof(UnknownErr)]);
		public static K AddFailed = Mk(_R, [nameof(AddFailed)]);
		public static K PermissionDenied = Mk(_R, [nameof(PermissionDenied)]);
		[Doc(@$"數據不合法或衝突")]
		public static K DataIllegalOrConflict = Mk(_R, [nameof(PermissionDenied)]);
	}
	public class Audio{
		public static K _R = Mk(null, [nameof(Audio)]);
		[Doc(@$"已取得音頻數據 但 播放失敗")]
		public static K AudioPlayFailed = Mk(_R, [nameof(AudioPlayFailed)]);
	}
	public class User{
		public static K _R = Mk(null, [nameof(User)]);
		[Doc(@$"慎用、防止用戶枚舉漏洞")]
		public static K UserNotExist = Mk(_R, [nameof(UserNotExist)]);
		[Doc(@$"慎用、防止用戶枚舉漏洞")]
		public static K UserAlreadyExist = Mk(_R, [nameof(UserAlreadyExist)]);
		public static K PasswordNotMatch = Mk(_R, [nameof(PasswordNotMatch)]);
		public static K InvalidToken = Mk(_R, [nameof(InvalidToken)]);
		public static K TokenExpired = Mk(_R, [nameof(TokenExpired)]);
		public static K AuthenticationFailed = Mk(_R, [nameof(AuthenticationFailed)]);
	}
	public class Word{
		public static K _R = Mk(null, [nameof(Word)]);
		public static K WordOfId__NotFound = Mk(_R, [nameof(WordOfId__NotFound)]);
		public static K LoadWordListFailed = Mk(_R, [nameof(LoadWordListFailed)]);
		public static K SaveWordListFailed = Mk(_R, [nameof(SaveWordListFailed)]);
		public static K LoadWordCalcWeightFailed = Mk(_R, [nameof(LoadWordCalcWeightFailed)]);
		public static K StartLearnWithWeightCalcFailed = Mk(_R, [nameof(StartLearnWithWeightCalcFailed)]);
		public static K WeightCalcGetStudyPlanFailed = Mk(_R, [nameof(WeightCalcGetStudyPlanFailed)]);
		public static K WeightCalcInvalidAlgorithm = Mk(_R, [nameof(WeightCalcInvalidAlgorithm)]);
		public static K WeightCalcRunFailed = Mk(_R, [nameof(WeightCalcRunFailed)]);
		public static K WeightCalcResultHandleFailed = Mk(_R, [nameof(WeightCalcResultHandleFailed)]);
		public static K WeightCalcResultStreamNull = Mk(_R, [nameof(WeightCalcResultStreamNull)]);
		public static K WeightCalcResultWordIdInvalid__ = Mk(_R, [nameof(WeightCalcResultWordIdInvalid__)]);
		public static K JsWeightCalcCodeEmpty = Mk(_R, [nameof(JsWeightCalcCodeEmpty)]);
		public static K JsWeightCalcExecFailed = Mk(_R, [nameof(JsWeightCalcExecFailed)]);
		public static K JsWeightCalcReturnedEmpty = Mk(_R, [nameof(JsWeightCalcReturnedEmpty)]);
		public static K JsWeightCalcReturnedInvalidJson = Mk(_R, [nameof(JsWeightCalcReturnedInvalidJson)]);
		public static K BuiltinWeightCalcArgParseFailed = Mk(_R, [nameof(BuiltinWeightCalcArgParseFailed)]);
		public static K BuiltinWeightCalcExecFailed = Mk(_R, [nameof(BuiltinWeightCalcExecFailed)]);
		public static K __NotBelongToLang__ = Mk(_R, [nameof(__NotBelongToLang__)]);
		public static K __And__IsNotSameUserWord = Mk(_R, [nameof(__And__IsNotSameUserWord)]);
		public static K BackgroundImageServiceFailedToInit = Mk(_R, [nameof(BackgroundImageServiceFailedToInit)]);
		[Doc(@$"{nameof(PoNormLangToUserLang)}未設置。")]
		public static K NormLangToUserLangIsNotMapped = Mk(_R, [nameof(NormLangToUserLangIsNotMapped)]);
		
		[Doc(@$"單詞同步失敗。
		參數填入 WordId字符串。
		")]
		public static K Word__And__SyncFailed = Mk(_R, [nameof(Word__And__SyncFailed)]);
	}
	public class StudyPlan{
		public static K _R = Mk(null, [nameof(StudyPlan)]);
		[Doc(@$"佔位符: [1]->褈複的UniqName")]
		[Obsolete(@$"實現複雜。直接用 {nameof(Common.DataIllegalOrConflict)}")]
		public static K UniqNameDuplicated__ = Mk(_R, [nameof(UniqNameDuplicated__)]);
		
		[Obsolete(@$"直接用 {nameof(Common.DataIllegalOrConflict)}")]
		public static K AddFailedDataMayConflict = Mk(_R, [nameof(AddFailedDataMayConflict)]);
		[Obsolete(@$"直接用 {nameof(Common.DataIllegalOrConflict)}")]
		public static K UpdateFailedDataMayConflict = Mk(_R, [nameof(UpdateFailedDataMayConflict)]);
	}

	/// 字典服務相關異常
	public class Dictionary{
		public static K _R = Mk(null, [nameof(Dictionary)]);
		/// LLM 字典 API 未配置（URL 或 Key 缺失）
		public static K LlmApiNotConfigured = Mk(_R, [nameof(LlmApiNotConfigured)]);
		/// LLM API 返回空響應
		public static K LlmApiEmptyResponse = Mk(_R, [nameof(LlmApiEmptyResponse)]);
		/// LLM API 響應結構無效
		public static K LlmApiInvalidResponseStructure = Mk(_R, [nameof(LlmApiInvalidResponseStructure)]);
		/// LLM API 返回空內容
		public static K LlmApiEmptyContent = Mk(_R, [nameof(LlmApiEmptyContent)]);
		/// 解析 LLM 響應失敗
		public static K LlmResponseParseFailed = Mk(_R, [nameof(LlmResponseParseFailed)]);
	}
	public class Sync{
		public static K _R = Mk(null, [nameof(Sync)]);
		public static K BatchGetByIdCountMismatch = Mk(_R, [nameof(BatchGetByIdCountMismatch)]);
		public static K SyncShouldUseSameId = Mk(_R, [nameof(SyncShouldUseSameId)]);
	}
}

