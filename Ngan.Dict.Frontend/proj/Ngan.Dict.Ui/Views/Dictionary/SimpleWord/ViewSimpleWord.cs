namespace Ngan.Dict.Ui.Views.Dictionary.SimpleWord;

using Avalonia.Controls;
using Ngan.Dict.Core.Shared.Dictionary.Models;
using Ngan.Dict.Ui.Infra;
using Ngan.Dict.Ui.Infra.Ctrls;
using Tsinswreng.CsCore;

/// 顯示詞頭、讀音列表與釋義的簡明查詞結果視圖。
/// 佈局：內容縱向排列，右上角疊一個「複製釋義」按鈕；右側留出按鈕寬度，避免遮住首行。
public partial class ViewSimpleWord : AppViewBase<VmSimpleWord>{

	/// View 只能有無參構造器。
	public partial ViewSimpleWord();

	/// 樣式類名常量。
	public partial class Cls{}

	/// 詞頭。
	public SelectableTextBlock? _Head;
	/// 讀音列表（每行：播放按鈕＋讀音文本）。
	public ItemsControl? _Pronunciations;
	/// 釋義全文。
	public SelectableTextBlock? _Description;
	/// 把本次釋義複製到剪貼板。
	public OpBtn? _CopyDescriptionBtn;

	/// 建立控件樹。
	protected override partial object Build(VmSimpleWord vm);

	/// 建立允許選取且自動換行的文本控件。
	protected partial SelectableTextBlock Txt();

	/// 建立讀音列表。
	protected partial Control MkPronunciations(VmSimpleWord vm);

	/// 建立單行讀音：播放按鈕＋讀音文本。
	protected partial Control MkPronunciationRow(VmSimpleWord vm, Pronunciation P);
}
