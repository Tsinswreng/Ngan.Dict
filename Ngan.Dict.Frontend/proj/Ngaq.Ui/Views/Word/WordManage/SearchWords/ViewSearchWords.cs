namespace Ngaq.Ui.Views.Word.WordManage.SearchWords;

using Avalonia.Controls;
using Ngaq.Ui.Components.PageBar;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.Ctrls;
using Tsinswreng.Avln.Grid;
using Ctx = VmSearchWords;

/// 用戶詞庫搜索、分頁瀏覽與入口導航頁。
public partial class ViewSearchWords: AppViewBase<Ctx>, I_MkTitleMenu{
	/// 頁面根版面。
	public GridStack Root = new(IsRow: true);
	/// 搜索欄水平版面。
	public GridStack SearchGrid = new(IsRow: false);
	/// 執行當前關鍵字搜索的按鈕。
	public OpBtn? SearchBtn;
	/// 輸入搜索關鍵字的控件。
	public TextBox? SearchInputCtrl;
	/// 開啟自由加詞頁的按鈕。
	public OpBtn? FreeAddBtn;
	/// 顯示搜索命中的虛擬化列表。
	public ItemsControl? WordListCtrl;
	/// 承載搜索結果列表的捲動容器。
	public ScrollViewer? WordListScroll;
	/// 與搜索 ViewModel 共享分頁狀態的子 View。
	public ViewPageBar? PageBarView;

	/// 取得頁面 ViewModel，初始化樣式並建立控件樹。
	public partial ViewSearchWords();
	/// 初始化此頁專屬樣式；目前保留作為後續擴展入口。
	partial void InitStyle();
	/// 建立搜索欄、結果列表及分頁列。
	partial void Render();
	/// 建立虛擬化搜索結果列表與命中卡片模板。
	private partial Control MkWordList();
	/// 依選取命中建立並打開單詞編輯頁。
	private partial void OpenWordEditor(Ngaq.Core.Shared.Word.Models.Dto.DtoWordSearchHit Hit);
	/// 建立並注入頁面共用分頁狀態的分頁子 View。
	private partial Control MkPageBar();
	/// 建立標題列選單中的新增、語言與同步入口。
	public partial Control MkTitleMenu();
	/// 建立導向指定 View 的單一標題選單項。
	private partial MenuItem MkMenuItem(str Title, Func<Control> MkView);
	/// 建立空白草稿並打開自由加詞編輯頁。
	private partial nil OpenFreeAddWordPage();
}
