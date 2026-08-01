namespace Ngaq.Ui.Views.Dictionary.NormLangTagEdit;

using Avalonia.Controls;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.Ctrls;
using Ngaq.Ui.Views.Dictionary.NormLangTag;
using Tsinswreng.Avln.Grid;
using Ctx = VmNormLangTagEdit;

/// 詞典源語言快捷標籤專用編輯頁。
/// 提供短文字編輯、上下移、刪除、添加語言與保存操作。
public partial class ViewNormLangTagEdit: AppViewBase<Ctx>{
	/// 建立編輯頁並初始化控件樹。
	public partial ViewNormLangTagEdit();

	/// 頁面根版面。
	public GridStack Root = new(IsRow: true);
	/// 顯示可編輯快捷標籤列的列表控件。
	public ItemsControl? TagList;
	/// 打開標準語言選擇頁的按鈕。
	public OpBtn? AddBtn;
	/// 保存配置並返回上一頁的按鈕。
	public OpBtn? SaveBtn;

	/// 建立快捷標籤列表與底部操作列。
	private partial void Render();
	/// 建立單個快捷標籤的編輯行。
	private partial Control MkTagRow(VmNormLangTag Tag);
	/// 建立添加與保存按鈕所在的操作列。
	private partial Control MkBottomBar();
	/// 打開標準語言選擇頁，選中後追加到編輯集合。
	private partial void OpenNormLangSelector();
	/// 保存配置；成功後返回詞典頁。
	private partial Task<nil> SaveEtBack(CT Ct);
}
