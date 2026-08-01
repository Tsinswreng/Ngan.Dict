namespace Ngaq.Ui.Views.Dictionary.LlmRawOutputEdit;

using Avalonia.Controls;
using Avalonia.Media;
using Ngaq.Ui;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.Ctrls;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsI18n;
using CommonK = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;
using Ctx = VmLlmRawOutputEdit;

/// LLM 原始輸出查看/編輯頁。
public partial class ViewLlmRawOutputEdit: AppViewBase<Ctx>{


	public ViewLlmRawOutputEdit(){
		Ctx = App.DiOrMk<Ctx>();
		Style();
		Render();
	}

	public partial class Cls{}

	protected nil Style(){
		return NIL;
	}

	GridStack Root = new(IsRow: true);
	protected nil Render(){
		this.Content = Root.Grid;
		Root.SetRowDefs([
			new(1, GUT.Star),
			new(1, GUT.Auto),
		]);
		Root.A(new TextBox(), o=>{
			o.CBind<Ctx>(o.PropText, x=>x.RawOutput);
			o.AcceptsReturn = true;
			o.TextWrapping = TextWrapping.Wrap;
			o.MinHeight = 200;
		})
		.A(new OpBtn(), o=>{
			o._Button.Background = UiCfg.Inst.MainColor;
			o._Button.HorizontalAlignment = HAlign.Stretch;
			o._Button.VerticalAlignment = VAlign.Stretch;
			o._Button.HorizontalContentAlignment = HAlign.Center;
			o._Button.VerticalContentAlignment = VAlign.Center;
			o._Button.FontWeight = FontWeight.Bold;
			o.BtnContent = I[CommonK.ConfirmChange];
			o.SetExe(Ct=>Ctx?.Confirm(Ct));
		});
		return NIL;
	}
}
