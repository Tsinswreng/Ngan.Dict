namespace Ngaq.Ui.Views.Word.WordManage.SearchWords.SearchedWordCard;

using Avalonia.Controls;
using Tsinswreng.Avln.Grid;
using Ngaq.Ui.Infra;
using Ctx = VmSearchedWordCard;

/// 搜索結果中的精簡單詞卡片。
public partial class ViewSearchedWordCard: AppViewBase<Ctx>{
	/// 卡片根版面。
	public GridStack Root = new(IsRow: true);
	/// 顯示語言標記的文字控件。
	public TextBlock? LangCtrl;
	/// 顯示單詞詞頭與刪除狀態的文字控件。
	public TextBlock? HeadCtrl;
	/// 顯示學習統計的資訊區。
	public Control? InfoCtrl;
	/// 建立卡片 ViewModel、樣式與控件樹。
	public partial ViewSearchedWordCard();
	/// 初始化詞卡文字陰影等共用視覺樣式。
	partial void InitStyle();
	/// 建立語言列、命中資訊與詞頭展示區。
	partial void Render();
	/// 建立學習結果、最近學習時間及權重的資訊格。
	private partial Control MkInfoGrid();
}
