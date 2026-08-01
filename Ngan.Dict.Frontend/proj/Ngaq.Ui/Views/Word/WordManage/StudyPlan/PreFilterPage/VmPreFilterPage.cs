namespace Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterPage;

using System;
using System.Collections.ObjectModel;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Shared.StudyPlan.Models.Po.PreFilter;
using Ngaq.Core.Shared.StudyPlan.Models.Req;
using Ngaq.Core.Shared.StudyPlan.Svc;
using Ngaq.Ui.Components.PageBar;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan;
using Ctx = VmPreFilterPage;

/// PreFilter 列表頁 ViewModel。
/// 使用後端接口分頁讀取，不再使用假數據
public partial class VmPreFilterPage: ViewModelBase, IMk<Ctx>{
	void Init(){
		PageBar = VmPageBar.Mk();
		PageBar.PageSize = 10;
		PageBar.FnPrevPage = OnPrevPage;
		PageBar.FnNextPage = OnNextPage;
	}
	protected VmPreFilterPage(){
		Init();
	}

	public static Ctx Mk(){
		return new Ctx();
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmPreFilterPage(){
		#if DEBUG
		{
			var o = new Ctx();
			Samples.Add(o);
		}
		#endif
	}

	ISvcStudyPlan? SvcStudyPlan{get;set;}
	IFrontendUserCtxMgr? UserCtxMgr{get;set;}

	/// 依賴注入構造器。
	public VmPreFilterPage(
		ISvcStudyPlan? SvcStudyPlan
		,IFrontendUserCtxMgr? UserCtxMgr
	):this(){
		this.SvcStudyPlan = SvcStudyPlan;
		this.UserCtxMgr = UserCtxMgr;
		Init();
	}

	public VmPageBar PageBar{get;set;} = null!;

	public str Input{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public ObservableCollection<RowPreFilter> Rows{get;set;} = [];

	public bool IsSelectMode{
		get;
		set{
			if(SetProperty(ref field, value)){
				OnPropertyChanged(nameof(CanCreate));
			}
		}
	} = false;

	/// 在选择模式中也允许新增，方便“边选边创建”。
	public bool CanCreate => true;

	Action<PoPreFilter>? FnOnSelected{get;set;}

	/// 列表行模型。
	public class RowPreFilter{
		public bool IsChecked{get;set;} = false;
		public u64 UiIdx{get;set;}
		public str UiIdxText{get;set;} = "";
		public str Name{get;set;} = "";
		public str Type{get;set;} = "";
		public str ModifiedTime{get;set;} = "";
		public PoPreFilter? Raw{get;set;} = null;
	}

	public async Task<nil> InitSearch(CT Ct){
		PageBar.PageNum = 1;
		return await Search(Ct);
	}

	/// 根據輸入條件查詢後端分頁，並回填 UI 列表。
	public async Task<nil> Search(CT Ct = default){
		if(AnyNull(SvcStudyPlan, UserCtxMgr)){
			return NIL;
		}
		try{
			var pageQry = PageBar.ToPageQry();
			pageQry.WantTotCnt = true;
			var req = new ReqPagePreFilter{
				PageQry = pageQry,
				UniqNameSearch = str.IsNullOrWhiteSpace(Input) ? null : Input.Trim(),
			};

			var page = await SvcStudyPlan.PagePreFilter(UserCtxMgr.GetDbUserCtx(), req, Ct);
			PageBar.FromPageResultInfo(page);

			Rows.Clear();
			var startUiIdx = page.PageIdx * page.PageSize;
			var localIdx = 0UL;
			if(page.DataAsyE is not null){
				await foreach(var po in page.DataAsyE){
					localIdx++;
					var uiIdx = startUiIdx + localIdx;
					Rows.Add(new RowPreFilter{
						UiIdx = uiIdx,
						UiIdxText = uiIdx.ToString(),
						Name = ToolStudyPlanView.FormatUniqName(po.UniqName),
						Type = po.Type.ToString(),
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

	protected async Task<nil> OnPrevPage(VmPageBar pageBar, CT Ct){
		if(pageBar.PageNum <= 1){
			pageBar.PageNum = 1;
			return NIL;
		}
		pageBar.PageNum--;
		return await Search(Ct);
	}

	protected async Task<nil> OnNextPage(VmPageBar pageBar, CT Ct){
		// TotPageCnt == null means backend does not provide total pages.
		// In this case, do not block paging forward by a fake "1 page" limit.
		if(pageBar.TotPageCnt is u64 totalPage && pageBar.PageNum >= totalPage){
			return NIL;
		}
		pageBar.PageNum++;
		return await Search(Ct);
	}

	/// <summary>
	/// 行点击处理：
	/// - 选择模式：回调选中项；
	/// - 普通模式：通知 View 层执行跳转。
	/// </summary>
	public nil OpenDetail(RowPreFilter? row = null){
		if(IsSelectMode){
			if(row?.Raw is not null){
				FnOnSelected?.Invoke(row.Raw);
				return NIL;
			}
			// 選擇模式下點擊「新增」(row==null) 也允許進入新增編輯頁。
			OnOpenDetailRequested?.Invoke(row);
			return NIL;
		}
		OnOpenDetailRequested?.Invoke(row);
		return NIL;
	}

	/// <summary>
	/// 切换为“选择模式”，用于给其他页面选取 PreFilter。
	/// </summary>
	public nil SetSelectMode(Action<PoPreFilter> FnOnSelected){
		IsSelectMode = true;
		this.FnOnSelected = FnOnSelected;
		return NIL;
	}

	/// <summary>
	/// 由 View 监听并执行实际导航（Vm 不直接依赖 View）。
	/// </summary>
	public event Action<RowPreFilter?>? OnOpenDetailRequested;
}
