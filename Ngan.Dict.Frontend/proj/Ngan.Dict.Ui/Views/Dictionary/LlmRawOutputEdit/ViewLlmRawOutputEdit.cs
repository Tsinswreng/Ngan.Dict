namespace Ngan.Dict.Ui.Views.Dictionary.LlmRawOutputEdit;

using Avalonia.Controls;
using Ngan.Dict.Ui.Infra;
using Ngan.Dict.Ui.Infra.Ctrls;
using Tsinswreng.Avln.Grid;
using Tsinswreng.CsCore;

/// LLM 原始輸出查看／編輯頁：上方可編輯原始輸出，下方確認按鈕。
public partial class ViewLlmRawOutputEdit : AppViewBase<VmLlmRawOutputEdit>{

	/// View 只能有無參構造器。
	public partial ViewLlmRawOutputEdit();

	/// 樣式類名常量。
	public partial class Cls{}

	/// 原始輸出編輯框。
	public TextBox? _RawOutput;

	/// 確認並重新解析的按鈕。
	public OpBtn? _ConfirmBtn;

	/// 建立控件樹。
	protected override partial object Build(VmLlmRawOutputEdit vm);
}
