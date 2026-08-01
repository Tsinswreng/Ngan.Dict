namespace Ngaq.Ui.Components.PageBar;
using System.Collections.ObjectModel;
using Ngaq.Ui.Infra;
using Tsinswreng.CsCore;
using Tsinswreng.CsPage;
using Ctx = VmPageBar;
[Doc(@$"have page size and page number,
may not suitable for cursor pagination.
")]
public partial class VmPageBar: ViewModelBase, IMk<Ctx>{
	//蔿從構造函數依賴注入、故以靜態工廠代無參構造器
	protected VmPageBar(){}
	public static Ctx Mk(){
		return new Ctx();
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmPageBar(){
		#if DEBUG
		{
			var o = new Ctx();
			Samples.Add(o);
		}
		#endif
	}

	public void FromPageResultInfo(IPageResultInfo PageResultInfo){
		var z = this; var p = PageResultInfo;
		z.PageNum = p.PageIdx+1;
		z.PageSize = p.PageSize;
		if(!p.HasTotCnt){
			z.TotCnt = null;
			z.TotPageCnt = null;
			return;
		}
		z.TotCnt = p.TotCnt;
		if(z.PageSize == 0){
			z.TotPageCnt = null;
			return;
		}
		var totCnt = z.TotCnt ?? 0;
		z.TotPageCnt = totCnt/z.PageSize + (u64)(totCnt%z.PageSize!=0 ? 1 : 0);
	}

	public IPageQry ToPageQry(){
		var z = this;
		var R = new PageQry();
		R.PageIdx = z.PageNum-1;
		R.PageSize = z.PageSize;
		return R;
	}

	public Func<VmPageBar, CT, Task<nil>>? FnPrevPage{get;set;}
	public Func<VmPageBar, CT, Task<nil>>? FnNextPage{get;set;}



	[Doc(@$"Page number shown in GUI.
	ususally starts from 1,
	unlike {nameof(IPageQry.PageIdx)} which starts from 0.
	")]
	public u64 PageNum{
		get;
		set{SetProperty(ref field, value);}
	}=1;

	public str PageNumStr{
		get=>PageNum+"";
		set{
			if(u64.TryParse(value, out var v)){
				PageNum = v;
			}
		}
	}

	public u64 PageSize{
		get;
		set{SetProperty(ref field, value);}
	}=10;

	public str PageSizeStr{
		get=>PageSize+"";
		set{
			if(u64.TryParse(value, out var v)){
				PageSize = v;
			}
		}
	}

	[Doc(@$"Total count of items (not count of pages).
	backend may not provide this information,
	in this case, set to null, and not show it in GUI.
	")]
	public u64? TotCnt{
		get;
		set{SetProperty(ref field, value);}
	}=null;

	[Doc(@$"#See{nameof(TotCnt)}")]
	public u64? TotPageCnt{
		get;
		set{SetProperty(ref field, value);}
	}=null;

	public str TotPageCntStr{
		get{
			return TotPageCnt+"";
		}
		set{
			if(u64.TryParse(value, out var v)){
				TotPageCnt = v;
			}
		}
	}
}
