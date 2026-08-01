namespace Ngaq.Ui.Views.Word.WordManage.StudyPlan.WeightArgPage;

using System;
using System.Collections.ObjectModel;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightArg;
using Ngaq.Core.Shared.StudyPlan.Models.Req;
using Ngaq.Core.Shared.StudyPlan.Svc;
using Ngaq.Ui.Components.PageBar;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan;

using Ctx = VmWeightArgPage;

/// WeightArg 列表頁 ViewModel。
/// 使用後端接口分頁讀取，不使用假數據。
public partial class VmWeightArgPage: ViewModelBase, IMk<Ctx>{
	void Init(){
		PageBar = VmPageBar.Mk();
		PageBar.PageSize = 10;
		PageBar.FnPrevPage = OnPrevPage;
		PageBar.FnNextPage = OnNextPage;
	}

	protected VmWeightArgPage(){
		Init();
	}

	public static Ctx Mk(){
		return new Ctx();
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmWeightArgPage(){
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
	public VmWeightArgPage(
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

	public ObservableCollection<RowWeightArg> Rows{get;set;} = [];

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

	Action<PoWeightArg>? FnOnSelected{get;set;}

	/// 列表行模型。
	public class RowWeightArg{
		public bool IsChecked{get;set;} = false;
		public u64 UiIdx{get;set;}
		public str UiIdxText{get;set;} = "";
		public str Name{get;set;} = "";
		public str ModifiedTime{get;set;} = "";
		public PoWeightArg? Raw{get;set;} = null;
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
			var req = new ReqPageWeightArg{
				PageQry = pageQry,
				UniqNameSearch = str.IsNullOrWhiteSpace(Input) ? null : Input.Trim(),
			};

			var page = await SvcStudyPlan.PageWeightArg(UserCtxMgr.GetDbUserCtx(), req, Ct);
			PageBar.FromPageResultInfo(page);

			Rows.Clear();
			var startUiIdx = page.PageIdx * page.PageSize;
			var localIdx = 0UL;
			if(page.DataAsyE is not null){
				await foreach(var po in page.DataAsyE){
					localIdx++;
					var uiIdx = startUiIdx + localIdx;
					Rows.Add(new RowWeightArg{
						UiIdx = uiIdx,
						UiIdxText = uiIdx.ToString(),
						Name = ToolStudyPlanView.FormatUniqName(po.UniqName),
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
	public nil OpenDetail(RowWeightArg? row = null){
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
	/// 切换为“选择模式”，用于给其他页面选取 WeightArg。
	/// </summary>
	public nil SetSelectMode(Action<PoWeightArg> FnOnSelected){
		IsSelectMode = true;
		this.FnOnSelected = FnOnSelected;
		return NIL;
	}

	/// <summary>
	/// 由 View 监听并执行实际导航（Vm 不直接依赖 View）。
	/// </summary>
	public event Action<RowWeightArg?>? OnOpenDetailRequested;
}
