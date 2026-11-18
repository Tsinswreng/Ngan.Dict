namespace Ngan.Dict.Ui.Views.Dictionary.LlmRawOutputEdit;

using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Ngan.Dict.Ui;
using Ngan.Dict.Ui.Infra;
using Ngan.Dict.Ui.Infra.Ctrls;
using Tsinswreng.Avln.Grid;
using Tsinswreng.CsCore;
using CommonK = Ngan.Dict.Ui.Infra.I18n.KeysUiI18nCommon;

/// ViewLlmRawOutputEdit 的實現。
public partial class ViewLlmRawOutputEdit{
	/// View 只能有無參構造器；Vm 在這裏解析。
	public partial ViewLlmRawOutputEdit(){
		vm = App.DiOrMk<VmLlmRawOutputEdit>();
	}

	/// 上方是原始輸出（雙向綁定），下方是確認按鈕。
	protected override partial object Build(VmLlmRawOutputEdit vm){
		var Input = new TextBox()
			.With(t => {
				_RawOutput = t;
				t.AcceptsReturn = true;
				t.TextWrapping = TextWrapping.Wrap;
				t.MinHeight = 200;
			})
			.Text(vm, x => x.RawOutput, BindingMode.TwoWay)
		;
		var Confirm = new OpBtn()
			.With(b => {
				_ConfirmBtn = b;
				b._Button.Background = UiCfg.Inst.MainColor;
				b._Button.HorizontalAlignment = HAlign.Stretch;
				b._Button.VerticalAlignment = VAlign.Stretch;
				b._Button.HorizontalContentAlignment = HAlign.Center;
				b._Button.VerticalContentAlignment = VAlign.Center;
				b._Button.FontWeight = FontWeight.Bold;
				b.BtnContent = I[CommonK.ConfirmChange];
				b.SetExe(Ct => vm.Confirm(Ct));
			})
		;
		return new StackGrid(IsRow: true)
			.Rows("*,Auto")
			.Children(Input, Confirm)
		;
	}
}
