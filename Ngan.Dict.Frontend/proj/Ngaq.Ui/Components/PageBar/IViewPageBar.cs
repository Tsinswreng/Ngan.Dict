using Tsinswreng.CsCore;

namespace Ngaq.Ui.Components.PageBar;

public interface IViewPageBar{

	[Doc(@$"點擊上一頁按鈕")]
	public Task<nil> ClickPrev(CT Ct);
	[Doc(@$"點擊下一頁按鈕")]
	public Task<nil> ClickNext(CT Ct);
	public str? CurPage { get; set; }
	public str? TotPage { get; set; }
	public str? PageSize{ get;set; }

}
