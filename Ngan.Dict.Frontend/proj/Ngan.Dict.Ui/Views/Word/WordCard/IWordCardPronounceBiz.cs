namespace Ngan.Dict.Ui.Views.Word.WordCard;

using Ngan.Dict.Core.Infra;
using Ngan.Dict.Core.Shared.Word.Models;

/// 單詞卡片「朗讀」業務能力。
/// 僅處理業務判斷與服務調用，不觸及 View 層。
public interface IWordCardPronounceBiz{
	public Task<DtoWordCardPronounceResult> PronounceWord(
		IDbUserCtx DbUserCtx,
		IJnWord? JnWord,
		CT Ct
	);
}

