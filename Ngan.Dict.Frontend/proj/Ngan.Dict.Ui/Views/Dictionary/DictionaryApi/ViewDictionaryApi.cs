namespace Ngan.Dict.Ui.Views.Dictionary.DictionaryApi;

using Avalonia.Controls;
using Ngan.Dict.Ui.Infra;
using Tsinswreng.Avln.Grid;
using Tsinswreng.CsCore;

/// 詞典 API 結果頁：把後端返回的 `IList<DictionaryApiWord>` 逐條顯示。
public partial class ViewDictionaryApi : AppViewBase<VmDictionaryApi>{

	/// View 只能有無參構造器。
	public partial ViewDictionaryApi();

	/// 樣式類名常量。
	public partial class Cls{}

	/// 承載所有詞條的列表。
	public ListBox? _Words;

	/// 建立控件樹。
	protected override partial object Build(VmDictionaryApi vm);

	/// 建立單個詞條：詞頭＋音標，再逐個 meaning 列出詞性與釋義。
	protected partial Control MkItem(Ngan.Dict.Core.Shared.Word.Models.DictionaryApi.DictionaryApiWord Word);
}
