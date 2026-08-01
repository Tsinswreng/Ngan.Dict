namespace Ngaq.Ui.Views.Word.WordEditV2.WordPropEdit;

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using Ngaq.Core.Model.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views.Word.WordEditV2.WordPropPage;
using Tsinswreng.Avln.Grid;
using Tsinswreng.Avln.Dsl;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;
using Ctx = VmWordPropEdit;
using Ngaq.Ui.Infra.Ctrls;

/// 單行屬性編輯頁。
public partial class ViewWordPropEdit: AppViewBase<Ctx>{
	/// 顯示屬性識別碼的唯讀控件，供檢視目前編輯目標。
	public SelectableTextBlock? IdCtrl;
	/// 選擇屬性鍵類型的控件。
	public ComboBox? KTypeCtrl;
	/// 輸入或選擇字串類型屬性鍵的控件。
	public ComboBox? KStrCtrl;
	/// 輸入整數類型屬性鍵的控件。
	public TextBox? KI64Ctrl;
	/// 選擇屬性值類型的控件。
	public ComboBox? VTypeCtrl;
	/// 輸入字串類型屬性值的控件。
	public TextBox? VStrCtrl;
	/// 輸入整數類型屬性值的控件。
	public TextBox? VI64Ctrl;
	/// 返回屬性列表的保存按鈕。
	public OpBtn? SaveBtn;
	/// 刪除目前屬性資料列的按鈕。
	public OpBtn? DeleteBtn;
	/// 承載全部輸入控件的表單容器，供後置注入的行 ViewModel 綁定。
	public StackPanel? EditorForm;
	/// 記錄已掛接通知的 ViewModel，避免在資料上下文切換時遺留訂閱。
	public Ctx? SubscribedCtx;

	IReadOnlyList<str> KvTypeOptions => [
		I[K.KvTypeStr],
		I[K.KvTypeI64],
	];

	IReadOnlyList<str> PropKeyDisplayOptions => [
		I[K.Descr],
		I[K.Summary],
		I[K.Note],
		I[K.Tag],
		I[K.Source],
		I[K.Alias],
		I[K.Pronunciation],
		I[K.Weight],
		I[K.Learn],
		I[K.Usage],
		I[K.Example],
		I[K.Relation],
		I[K.Ref],
	];

	public partial ViewWordPropEdit();

	/// 依既定版面與綁定狀態建立此頁的控制項樹。
	partial void Render();

	/// 與 LearnEdit 同理，表單直接綁行 Vm，避免嵌套綁定丟失初值。
	partial void OnCtxChanged();

	/// 處理 EditVmPropertyChanged 事件，維持頁面狀態與資料來源同步。
	partial void OnEditVmPropertyChanged(object? Sender, System.ComponentModel.PropertyChangedEventArgs E);

	/// 執行 ApplyRowCtx 所代表的內部協調操作。
	partial void ApplyRowCtx();

	/// 建立 IdSelectableRow 所需的 UI 組件，供頁面組裝時重用。
	private partial Control MkIdSelectableRow(str Label, IBinding Binding, Action<SelectableTextBlock> Init);

	/// 建立 InputRow 所需的 UI 組件，供頁面組裝時重用。
	private partial Control MkInputRow(str Label, IBinding Binding, Action<TextBox> Init);

	/// 建立 ComboRow 所需的 UI 組件，供頁面組裝時重用。
	private partial Control MkComboRow(str Label, IEnumerable<str> Items, IBinding Binding, Action<ComboBox> Init);

	/// 建立 EditableComboRow 所需的 UI 組件，供頁面組裝時重用。
	private partial Control MkEditableComboRow(str Label, IEnumerable<str> Items, IBinding Binding, Action<ComboBox> Init);

	/// 建立 FieldRow 所需的 UI 組件，供頁面組裝時重用。
	private partial Control MkFieldRow(str Label, Control Input);


	/// 將編輯值轉換為儲存或顯示所需的標準格式。
	private partial str ToDisplayPropKey(str RawKey);

	/// 將編輯值轉換為儲存或顯示所需的標準格式。
	private partial str ToStoredPropKey(str DisplayKey);
}
