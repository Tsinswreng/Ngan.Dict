namespace Ngaq.Ui.Views.Word.WordEditV2.WordLearnEdit;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using Ngaq.Ui.Components.TempusBox;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views.Word.WordEditV2.WordLearnPage;
using Tsinswreng.Avln.Grid;
using Tsinswreng.Avln.Dsl;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsTempus;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

/// 單行學習記錄編輯頁。
public partial class ViewWordLearnEdit: AppViewBase{
	public VmWordLearnEdit? Ctx{
		get{return DataContext as VmWordLearnEdit;}
		set{DataContext = value;}
	}

	StackPanel? EditorForm;
	VmWordLearnEdit? SubscribedCtx;

	IReadOnlyList<str> LearnResultOptions => [
		I[K.Learn_Add],
		I[K.Learn_Rmb],
		I[K.Learn_Fgt],
	];

	public partial ViewWordLearnEdit();

	/// 依既定版面與綁定狀態建立此頁的控制項樹。
	partial void Render();

	/// 編輯表單直接綁到行 Vm，自避開 `Ctx.Row.Xxx` 這種嵌套綁定在後置注入時失效的問題。
	partial void OnCtxChanged();

	/// 處理 EditVmPropertyChanged 事件，維持頁面狀態與資料來源同步。
	partial void OnEditVmPropertyChanged(object? Sender, System.ComponentModel.PropertyChangedEventArgs E);

	/// 執行 ApplyRowCtx 所代表的內部協調操作。
	partial void ApplyRowCtx();

	/// 建立 IdSelectableRow 所需的 UI 組件，供頁面組裝時重用。
	private partial Control MkIdSelectableRow(str Label, IBinding Binding);

	/// 建立 ComboRow 所需的 UI 組件，供頁面組裝時重用。
	private partial Control MkComboRow(str Label, IEnumerable<str> Items, IBinding Binding);

	/// 建立 TempusRow 所需的 UI 組件，供頁面組裝時重用。
	private partial Control MkTempusRow(str Label, IBinding Binding);

	/// 建立 FieldRow 所需的 UI 組件，供頁面組裝時重用。
	private partial Control MkFieldRow(str Label, Control Input);

}
