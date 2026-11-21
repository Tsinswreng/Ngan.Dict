namespace Ngan.Dict.Ui.Views.Dictionary;

using Avalonia.Controls;
using Avalonia.Media;
using Ngan.Dict.Core.Shared.Word.Models;
using Ngan.Dict.Core.Shared.Word.Models.Dto;
using Ngan.Dict.Ui.Infra;
using Ngan.Dict.Ui.Infra.Ctrls;
using Ngan.Dict.Ui.Views.Dictionary.NormLangTag;
using Ngan.Dict.Ui.Views.Dictionary.SimpleWord;
using Tsinswreng.Avln.Grid;
using Tsinswreng.CsCore;

/// 詞典查詢頁：語言選擇、查詞、結果展示與詞庫入口。
/// 版面由上而下四列：源語言快捷標籤列、語言列、搜索列、結果區。
public partial class ViewDictionary
	: AppViewBase<VmDictionary>
	, I_MkTitleMenu
{
	/// View 只能有無參構造器；Vm 在構造器裏解析。
	public partial ViewDictionary();

	/// 樣式類名常量。
	public partial class Cls{}

	/// 頁面根版面：四列縱向堆疊。
	public StackGrid Root = new(IsRow: true);
	/// 輸入待查詞語的文本框。
	public TextBox? _SearchTextBox;
	/// 執行或取消查詞的操作按鈕。
	public Button? _SearchBtn;
	/// 快速保存當前查詞結果的操作按鈕。
	public Button? _SaveToWordBtn;
	/// 打開詞典頁附加操作選單的按鈕。
	public Button? _MenuBtn;
	/// 打開快捷標籤編輯頁的按鈕。
	public OpBtn? _SrcLangTagEditBtn;
	/// 橫向承載源語言快捷標籤的容器。
	public StackPanel _SrcLangTagList = new(){
		Orientation = Avalonia.Layout.Orientation.Horizontal,
	};
	/// 顯示源語言快捷標籤的橫向滾動容器。
	public ScrollViewer? _SrcLangTagScroll;
	/// 承載使用提示與查詞結果的結果區。
	public Grid? _ResultArea;
	/// 搜索列容器（診斷用）。
	public DockPanel? _SearchBar;
	/// 簡明查詞結果子視圖。
	public ViewSimpleWord? _ResultView;

	/// 建立控件樹。
	protected override partial object Build(VmDictionary vm);

	/// 控件樹建好後的初始化：掛事件、載入語言與快捷標籤。
	protected override partial void OnAfterInitialized();

/// 建立語言列：源語言、互換、目標語言。
	protected partial Control MkLangBar(VmDictionary vm);
	/// 建立搜索列：查詞、輸入框、快速保存、選單。
	protected partial Control MkSearchBar(VmDictionary vm);
	/// 建立源語言快捷欄。
	protected partial Control MkSrcLangTagBar(VmDictionary vm);
	/// 建立單個源語言快捷 Tag。
	protected partial Control MkSrcLangTag(VmDictionary vm, VmNormLangTag Tag);
	/// 按 ViewModel 當前集合重建快捷 Tag 列。
	protected partial void RebuildSrcLangTags();
	/// 打開快捷標籤編輯頁。
	protected partial void OpenSrcLangTagEditor();
	/// 建立共用樣式的語言選擇按鈕。
	protected partial Button MkLangButton();
	/// 建立交換源語言與目標語言的按鈕。
	protected partial Button MkLangSwapButton();
	/// 建立搜索列上的圖標按鈕。
	protected partial Button MkBarBtn(Control Icon, Action? OnClick, IBrush? Background = null);

	/// 初始化工具列按鈕的外觀與命令。
	protected partial nil InitToolbarBtn(OpBtn Btn, Control Content, Func<CT, Task<nil>>? Exe = null, IBrush? Background = null);
	/// 未查詞時顯示的使用說明。
	protected partial str GuideText();
	/// 有結果時快速保存，否則進入自由加詞頁。
	protected partial Task<nil> QuickSaveOrFreeAdd(CT Ct);
	/// 經單詞編輯頁保存；沒有結果則進自由加詞頁。
	protected partial Task<nil> OpenSaveViaWordEditOrFreeAdd(CT Ct);
	/// 標記快速保存按鈕為已保存。
	protected partial nil MarkQuickSaveBtnAsSaved();
	/// 恢復快速保存按鈕的默認樣式。
	protected partial nil ResetQuickSaveBtnToDefaultStyle();
	/// 跟隨 Vm 狀態更新按鈕外觀。
	protected partial void OnVmPropertyChanged(object? Sender, System.ComponentModel.PropertyChangedEventArgs E);
	/// 建立結果區。
	protected partial Control MkResultArea(VmDictionary vm);
	/// 打開源語言／目標語言選擇頁。
	protected partial void OpenNormLangSelector(bool IsSrc);
	/// 建立標題選單。
	public partial Control MkTitleMenu();
/// 建立帶標題與點擊動作的選單項目。
	protected partial MenuItem MkMenuItem(str Header, Action OnClick);
	/// 在錨點附近打開標題選單。
	protected partial void OpenTitleMenuNear(Control? Anchor);
	/// 用指定單詞草稿打開合併模式的單詞編輯頁。
	protected partial nil OpenWordEditPage(JnWord JnWord);
	/// 打開預填源語言的空白加詞頁。
	protected partial nil OpenFreeAddWordPage();
	/// 可選填入搜索文本後觸發查詞。
	public partial void ClickLookupBtn(str? SearchText = null);
}
