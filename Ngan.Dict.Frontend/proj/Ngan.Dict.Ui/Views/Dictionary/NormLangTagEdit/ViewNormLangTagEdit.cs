namespace Ngan.Dict.Ui.Views.Dictionary.NormLangTagEdit;

using Avalonia.Controls;
using Ngan.Dict.Ui.Infra;
using Ngan.Dict.Ui.Infra.Ctrls;
using Ngan.Dict.Ui.Views.Dictionary.NormLangTag;
using Tsinswreng.Avln.Grid;
using Tsinswreng.CsCore;

/// 詞典源語言快捷標籤編輯頁：短文字編輯、上下移、刪除、添加語言與保存。
public partial class ViewNormLangTagEdit : AppViewBase<VmNormLangTagEdit>{

	/// View 只能有無參構造器。
	public partial ViewNormLangTagEdit();

	/// 樣式類名常量。
	public partial class Cls{}

	/// 頁面根版面：上面是標籤列表、下面是操作列。
	public StackGrid Root = new(IsRow: true);
	/// 可編輯快捷標籤列表。
	public ItemsControl? _TagList;
	/// 打開語言選擇頁的按鈕。
	public OpBtn? _AddBtn;
	/// 保存並返回的按鈕。
	public OpBtn? _SaveBtn;

	/// 建立控件樹。
	protected override partial object Build(VmNormLangTagEdit vm);

	/// 建立單個標籤的編輯行。
	protected partial Control MkTagRow(VmNormLangTag Tag);

	/// 建立添加與保存按鈕所在的操作列。
	protected partial Control MkBottomBar(VmNormLangTagEdit vm);

	/// 打開語言選擇頁，選中後追加到集合。
	protected partial void OpenNormLangSelector();

	/// 保存配置並返回。
	protected partial Task<nil> SaveEtBack(CT Ct);
}
