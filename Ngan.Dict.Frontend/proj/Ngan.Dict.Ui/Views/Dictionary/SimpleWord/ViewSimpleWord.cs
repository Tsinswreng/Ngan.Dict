namespace Ngan.Dict.Ui.Views.Dictionary.SimpleWord;

using Avalonia.Controls;
using Ngan.Dict.Core.Shared.Dictionary.Models;
using Ngan.Dict.Ui.Infra;
using Ngan.Dict.Ui.Infra.Ctrls;
using Tsinswreng.CsCore;

/// 查詢結果視圖：同一塊位置承載「詞典卡」與「翻譯卡」。
/// 詞典卡：詞頭＋讀音同排，其下逐義項（詞性／釋義／例句）；
/// 翻譯卡：原文＋譯文＋逐句對照＋備註。
/// 兩張卡由 `VmSimpleWord.Kind` 互斥顯示。
/// 版式原則見 `ViewSimpleWord.Impl.cs` 的檔頭說明：只用三級字色與三種字號，間距取自 `UiCfg`。
public partial class ViewSimpleWord : AppViewBase<VmSimpleWord>{

	/// View 只能有無參構造器。
	public partial ViewSimpleWord();

	/// 樣式類名常量。
	public partial class Cls{}

	/// 詞頭。
	public SelectableTextBlock? _Head;
	/// 讀音列表（每項：播放按鈕＋讀音文本）。
	public ItemsControl? _Pronunciations;
	/// 結構化義項列表。
	public ItemsControl? _Senses;

	/// 翻譯模式的原文。
	public SelectableTextBlock? _SourceText;
	/// 翻譯模式的譯文。
	public SelectableTextBlock? _Translation;
	/// 翻譯模式的逐句對照。
	public ItemsControl? _Segments;
	/// 翻譯模式的術語備註。
	public ItemsControl? _Notes;

	/// 建立控件樹。
	protected override partial object Build(VmSimpleWord vm);

	/// 建立允許選取且自動換行的文本控件。
	protected partial SelectableTextBlock Txt();

	/// 建立詞典卡。
	protected partial Control MkDictCard(VmSimpleWord vm);

	/// 建立詞頭列：詞頭與讀音同一行。
	protected partial Control MkHeadRow(VmSimpleWord vm);

	/// 建立讀音列表。
	protected partial Control MkPronunciations(VmSimpleWord vm);

	/// 建立單行讀音：播放按鈕＋讀音文本。
	protected partial Control MkPronunciationRow(VmSimpleWord vm, Pronunciation P);

	/// 建立義項列表。
	protected partial Control MkSenses(VmSimpleWord vm);

	/// 建立單個義項：詞性／釋義／例句。
	protected partial Control MkSenseBlock(DictSense Sense);

	/// 建立詞性標籤。
	protected partial TextBlock MkPosLabel(str Pos);

	/// 建立某義項底下的例句區。
	protected partial Control MkExamples(DictSense Sense);

	/// 建立翻譯卡。
	protected partial Control MkTranslateCard(VmSimpleWord vm);

	/// 建立逐句對照列表。
	protected partial Control MkSegments(VmSimpleWord vm);

	/// 建立術語備註列表。
	protected partial Control MkNotes(VmSimpleWord vm);
}
