namespace Ngaq.Ui.Views.Dictionary.SimpleWord;

using Avalonia.Controls;
using Ngaq.Ui.Infra;
using Tsinswreng.Avln.Grid;
using Ctx = VmSimpleWord;

/// 顯示詞頭、讀音列表與釋義的簡明查詞結果視圖。
public partial class ViewSimpleWord: AppViewBase<Ctx>{
	/// 建立簡明查詞結果視圖及控件樹。
	public partial ViewSimpleWord();

	/// 簡明查詞結果樣式類名集合；目前保留作後續擴展入口。
	public partial class Cls{}

	/// 初始化此視圖專屬樣式；目前沒有額外樣式。
	protected partial nil Style();

	/// 按詞頭、讀音與釋義順序排列內容的根版面。
	GridStack Root = new(IsRow: true);

	/// 建立允許選取且自動換行的文本控件。
	private partial SelectableTextBlock Txt();
	/// 建立詞頭、讀音列表與釋義控件樹。
	protected partial nil Render();
	/// 建立每行含播放按鈕與讀音文本的讀音列表。
	private partial Control PronunciationList();
}
