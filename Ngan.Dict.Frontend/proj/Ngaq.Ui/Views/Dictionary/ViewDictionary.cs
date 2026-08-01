namespace Ngaq.Ui.Views.Dictionary;

using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Media;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.Ctrls;
using Ngaq.Ui.Views.Dictionary.NormLangTag;
using Tsinswreng.Avln.Grid;
using Ctx = VmDictionary;

/// 詞典查詢頁，負責語言選擇、查詞操作、結果展示與詞庫入口導航。
public partial class ViewDictionary
	: AppViewBase<Ctx>
	, I_MkTitleMenu
{
	/// 建立詞典頁並接通 ViewModel 事件與控件樹。
	public partial ViewDictionary();

	/// 詞典頁樣式類名集合；目前保留作後續擴展入口。
	public partial class Cls{}

	/// 初始化此頁專屬樣式；目前沒有額外樣式。
	protected partial nil Style();

	/// 頁面根垂直版面。
	public GridStack Root = new(IsRow: true);
	/// 輸入待查詞語的文本框。
	public TextBox SearchTextBox = new();
	/// 執行或取消查詞的操作按鈕。
	public OpBtn SearchBtn = new();
	/// 快速保存當前查詞結果的操作按鈕。
	public OpBtn SaveToWordBtn = new();
	/// 打開詞典頁附加操作選單的按鈕。
	public OpBtn MenuBtn = new();
	/// 承載源語言、互換與目標語言按鈕的水平版面。
	public GridStack? _LangGrid;
	/// 承載源語言快捷 Tag 滾動區與固定編輯按鈕的水平版面。
	public GridStack? _SrcLangTagBar;
	/// 顯示源語言快捷 Tag 的橫向滾動容器。
	public ScrollViewer? SrcLangTagScroll;
	/// 承載動態數量源語言快捷 Tag 的橫向流式版面。
	public StackPanel SrcLangTagList = new(){
		Orientation = Avalonia.Layout.Orientation.Horizontal,
	};
	/// 打開快捷標籤專用編輯頁的按鈕。
	public OpBtn SrcLangTagEditBtn = new();
	/// 承載搜索、輸入、保存與選單按鈕的水平版面。
	GridStack? _searchGrid;
	/// 在使用提示與查詞結果之間切換的結果容器。
	Grid? _resultArea;

	/// 建立源語言快捷欄、語言列、搜索工具列與結果區。
	protected partial nil Render();
	[Doc(@$"建立可橫向滾動的源語言快捷 Tag 與固定編輯按鈕。
	_Note/Features/2026_0719_163811/DictLangSwitchBar.typ
	")]
	private partial Control MkSrcLangTagBar();
	/// 建立單個源語言快捷 Tag，並接通直接切換源語言的事件。
	private partial Control MkSrcLangTag(VmNormLangTag Tag);
	/// 按 ViewModel 當前集合重建橫向快捷 Tag 控件列。
	private partial void RebuildSrcLangTags();
	/// 打開快捷標籤專用編輯頁。
	private partial void OpenSrcLangTagEditor();
	/// 建立共用樣式的語言選擇按鈕。
	private partial Button MkLangButton();
	/// 建立交換源語言與目標語言的按鈕。
	private partial Button MkLangSwapButton();
	/// 初始化搜索、保存與選單共用的操作按鈕外觀及命令。
	private partial nil InitToolbarBtn(
		OpBtn Btn,
		Control Content,
		Func<CT, Task<nil>>? Exe = null,
		IBrush? Background = null
	);
	/// 取得未查詞時顯示的使用說明。
	private partial str GuideText();
	/// 有完整查詞結果時快速保存，否則進入自由加詞頁。
	private partial Task<nil> QuickSaveOrFreeAdd(CT Ct);
	/// 經單詞編輯頁保存當前結果；沒有結果時進入自由加詞頁。
	private partial Task<nil> OpenSaveViaWordEditOrFreeAdd(CT Ct);
	/// 將快速保存按鈕標記為已保存狀態。
	private partial nil MarkQuickSaveBtnAsSaved();
	/// 清除快速保存按鈕的局部顏色，使其恢復主題默認樣式。
	private partial nil ResetQuickSaveBtnToDefaultStyle();
	/// 跟隨 ViewModel 的快速保存狀態更新按鈕外觀。
	private partial void OnCtxPropertyChanged(object? Sender, PropertyChangedEventArgs E);
	/// 建立使用提示與簡明查詞結果共用的結果區。
	private partial Control MkResultArea();
	/// 打開源語言或目標語言選擇頁。
	private partial void OpenNormLangSelector(bool IsSrc);
	/// 建立詞典頁標題選單。
	public partial Control MkTitleMenu();
	/// 在指定錨點附近打開標題選單。
	private partial void OpenTitleMenuNear(Control? Anchor);
	/// 用指定單詞草稿打開合併模式的單詞編輯頁。
	private partial nil OpenWordEditPage(JnWord JnWord);
	/// 打開預填當前源語言的空白單詞編輯頁。
	private partial nil OpenFreeAddWordPage();
	/// 可選地填入搜索文本，然後觸發查詞按鈕。
	public partial void ClickLookupBtn(str? SearchText = null);
}
