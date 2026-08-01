namespace Ngaq.Ui.Views.Dictionary.LlmRawOutputEdit;

using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.I18n;
using CommonK = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

using Ctx = VmLlmRawOutputEdit;

/// LLM 原始輸出編輯頁 ViewModel。
public partial class VmLlmRawOutputEdit: ViewModelBase, IMk<Ctx>{
	protected VmLlmRawOutputEdit(){}
	public static Ctx Mk(){
		return new Ctx();
	}

	/// 可編輯的原始輸出文本。
	public str RawOutput{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	/// 外部注入的確認回調：接收編輯後文本並觸發重新解析。
	Func<str, CT, Task<nil>>? OnConfirm;

	public nil SetRawOutput(str Text){
		RawOutput = Text ?? "";
		return NIL;
	}

	public nil SetOnConfirm(Func<str, CT, Task<nil>>? Fn){
		OnConfirm = Fn;
		return NIL;
	}

	/// 點擊「確認更改」後執行：先回調解析，再返回上一頁。
	public async Task<nil> Confirm(CT Ct){
		if(OnConfirm is null){
			return NIL;
		}
		await OnConfirm(RawOutput, Ct);
		ViewNavi?.Back();
		return NIL;
	}
}
