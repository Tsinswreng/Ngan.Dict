namespace Ngaq.Ui.Views.Word.WordCard;

using Ngaq.Core.Shared.Word.Models;

/// 單詞卡片右鍵/長按菜單的業務動作入口。由頁面 ViewModel 實現。
public interface IWordCardMenuAction{
	/// 菜單朗讀動作。僅返回業務結果；UI 提示與導航由 View 層處理。
	public Task<DtoWordCardPronounceResult> PronounceWord(IJnWord? JnWord, CT Ct);
}
