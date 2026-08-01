namespace Ngaq.Ui.Views.Word.WordManage.NormLangToUserLang.NormLangToUserLangPage;

using System;
using System.Collections.ObjectModel;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Core.Shared.Word.Models.Po.NormLangToUserLang;
using Ngaq.Core.Shared.Word.Svc;
using Ngaq.Ui.Components.PageBar;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan;

using Ctx = VmNormLangToUserLangPage;

/// 標準語言到用戶語言映射分頁頁 ViewModel。
public partial class VmNormLangToUserLangPage: ViewModelBase, IMk<Ctx>{
	void Init(){
		PageBar = VmPageBar.Mk();
		PageBar.PageSize = 20;
		PageBar.FnPrevPage = OnPrevPage;
		PageBar.FnNextPage = OnNextPage;
	}

	protected VmNormLangToUserLangPage(){
		Init();
	}

	public static Ctx Mk(){
		return new Ctx();
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmNormLangToUserLangPage(){
		#if DEBUG
		{
			var o = new Ctx();
			Samples.Add(o);
		}
		#endif
	}

	ISvcNormLangToUserLang? SvcNormLangToUserLang{get;set;}
	IFrontendUserCtxMgr? UserCtxMgr{get;set;}

	public VmNormLangToUserLangPage(
		ISvcNormLangToUserLang? SvcNormLangToUserLang
		,IFrontendUserCtxMgr? UserCtxMgr
	):this(){
		this.SvcNormLangToUserLang = SvcNormLangToUserLang;
		this.UserCtxMgr = UserCtxMgr;
		Init();
	}

	public VmPageBar PageBar{get;set;} = null!;

	/// 搜索文本，同時匹配 NormLang / UserLang。
	public str Input{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public ObservableCollection<RowNormLangToUserLang> Rows{get;set;} = [];

	public class RowNormLangToUserLang{
		public u64 UiIdx{get;set;}
		public str UiIdxText{get;set;} = "";
		public str NormLangType{get;set;} = "";
		public str NormLang{get;set;} = "";
		public str UserLang{get;set;} = "";
		public str Descr{get;set;} = "";
		public str ModifiedTime{get;set;} = "";
		public PoNormLangToUserLang? Raw{get;set;} = null;
	}

	public async Task<nil> InitSearch(CT Ct){
		PageBar.PageNum = 1;
		return await Search(Ct);
	}

	public async Task<nil> Search(CT Ct = default){
		if(AnyNull(SvcNormLangToUserLang, UserCtxMgr)){
			return NIL;
		}
		try{
			var pageQry = PageBar.ToPageQry();
			pageQry.WantTotCnt = true;
			var req = new ReqPageNormLangToUserLang{
				PageQry = pageQry,
				SearchText = str.IsNullOrWhiteSpace(Input) ? null : Input.Trim(),
			};

			var page = await SvcNormLangToUserLang.PageNormLangToUserLang(UserCtxMgr.GetDbUserCtx(), req, Ct);
			PageBar.FromPageResultInfo(page);

			Rows.Clear();
			var startUiIdx = page.PageIdx * page.PageSize;
			var localIdx = 0UL;
			if(page.DataAsyE is not null){
				await foreach(var po in page.DataAsyE){
					localIdx++;
					var uiIdx = startUiIdx + localIdx;
					Rows.Add(new RowNormLangToUserLang{
						UiIdx = uiIdx,
						UiIdxText = uiIdx.ToString(),
						NormLangType = po.NormLangType.ToString(),
						NormLang = po.NormLang ?? "",
						UserLang = po.UserLang ?? "",
						Descr = po.Descr ?? "",
						ModifiedTime = ToolStudyPlanView.FormatUpdatedDateShort(po.BizUpdatedAt, po.BizCreatedAt),
						Raw = po,
					});
				}
			}
		}catch(Exception e){
			HandleErr(e);
		}
		return NIL;
	}

	protected async Task<nil> OnPrevPage(VmPageBar PageBar, CT Ct){
		if(PageBar.PageNum <= 1){
			PageBar.PageNum = 1;
			return NIL;
		}
		PageBar.PageNum--;
		return await Search(Ct);
	}

	protected async Task<nil> OnNextPage(VmPageBar PageBar, CT Ct){
		if(PageBar.TotPageCnt is u64 TotalPage && PageBar.PageNum >= TotalPage){
			return NIL;
		}
		PageBar.PageNum++;
		return await Search(Ct);
	}

	public nil OpenDetail(RowNormLangToUserLang? Row = null){
		OnOpenDetailRequested?.Invoke(Row);
		return NIL;
	}

	public event Action<RowNormLangToUserLang?>? OnOpenDetailRequested;
}
