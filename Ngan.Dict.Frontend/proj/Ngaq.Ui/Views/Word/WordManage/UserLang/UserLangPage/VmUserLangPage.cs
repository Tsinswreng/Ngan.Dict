namespace Ngaq.Ui.Views.Word.WordManage.UserLang.UserLangPage;

using System;
using System.Collections.ObjectModel;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Core.Shared.Word.Models.Po.UserLang;
using Ngaq.Core.Shared.Word.Svc;
using Ngaq.Ui.Components.PageBar;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan;

using Ctx = VmUserLangPage;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

/// UserLang 列表頁 ViewModel。
/// 負責分頁查詢、行模型映射與打開詳情事件派發。
public partial class VmUserLangPage: ViewModelBase, IMk<Ctx>{
	/// 初始化分頁器並綁定翻頁行爲。
	void Init(){
		PageBar = VmPageBar.Mk();
		PageBar.PageSize = 10;
		PageBar.FnPrevPage = OnPrevPage;
		PageBar.FnNextPage = OnNextPage;
	}

	/// 無參構造器供 `IMk<T>` 和設計期使用。
	protected VmUserLangPage(){
		Init();
	}

	/// 建立 Vm 實例。
	public static Ctx Mk(){
		return new Ctx();
	}

	/// 設計期示例數據。
	public static ObservableCollection<Ctx> Samples = [];
	static VmUserLangPage(){
		#if DEBUG
		{
			var o = new Ctx();
			Samples.Add(o);
		}
		#endif
	}

	/// 後端 UserLang 服務。
	ISvcUserLang? SvcUserLang{get;set;}
	/// 前端當前用戶上下文管理器。
	IFrontendUserCtxMgr? UserCtxMgr{get;set;}

	/// 依賴注入構造器。
	public VmUserLangPage(
		ISvcUserLang? SvcUserLang
		,IFrontendUserCtxMgr? UserCtxMgr
	):this(){
		this.SvcUserLang = SvcUserLang;
		this.UserCtxMgr = UserCtxMgr;
		Init();
	}

	/// 分頁條狀態。
	public VmPageBar PageBar{get;set;} = null!;

	/// 搜索輸入文本（按 UniqName 模糊查詢）。
	public str Input{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	/// 列表行集合（TreeDataGrid 綁定源）。
	public ObservableCollection<RowUserLang> Rows{get;set;} = [];

	/// 是否處於選擇模式（供其他頁面復用本分頁視圖挑選 UserLang）。
	public bool IsSelectMode{
		get;
		set{
			if(SetProperty(ref field, value)){
				OnPropertyChanged(nameof(ShowManageActions));
			}
		}
	} = false;

	/// 管理操作（新增/自動補全）是否顯示。
	public bool ShowManageActions => !IsSelectMode;

	Action<PoUserLang>? FnOnSelected{get;set;}

	/// 列表行模型。
	public class RowUserLang{
		public u64 UiIdx{get;set;}
		public str UiIdxText{get;set;} = "";
		public str Name{get;set;} = "";
		public str RelLangType{get;set;} = "";
		public str RelLang{get;set;} = "";
		public str ModifiedTime{get;set;} = "";
		public PoUserLang? Raw{get;set;} = null;
	}

	/// 重置到首頁並執行查詢。
	public async Task<nil> InitSearch(CT Ct){
		PageBar.PageNum = 1;
		return await Search(Ct);
	}

	/// 調用後端分頁接口，刷新當前表格行。
	public async Task<nil> Search(CT Ct = default){
		if(AnyNull(SvcUserLang, UserCtxMgr)){
			return NIL;
		}
		try{
			var pageQry = PageBar.ToPageQry();
			pageQry.WantTotCnt = true;
			var req = new ReqPageUserLang{
				PageQry = pageQry,
				UniqNameSearch = str.IsNullOrWhiteSpace(Input) ? null : Input.Trim(),
			};

			var page = await SvcUserLang.PageUserLang(UserCtxMgr.GetDbUserCtx(), req, Ct);
			PageBar.FromPageResultInfo(page);

			Rows.Clear();
			var startUiIdx = page.PageIdx * page.PageSize;
			var localIdx = 0UL;
			if(page.DataAsyE is not null){
				await foreach(var po in page.DataAsyE){
					localIdx++;
					var uiIdx = startUiIdx + localIdx;
					Rows.Add(new RowUserLang{
						UiIdx = uiIdx,
						UiIdxText = uiIdx.ToString(),
						Name = ToolStudyPlanView.FormatUniqName(po.UniqName),
						RelLangType = po.RelLangType.ToString(),
						RelLang = str.IsNullOrWhiteSpace(po.RelLang) ? "-" : po.RelLang,
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

	/// 上一頁。
	protected async Task<nil> OnPrevPage(VmPageBar PageBar, CT Ct){
		if(PageBar.PageNum <= 1){
			PageBar.PageNum = 1;
			return NIL;
		}
		PageBar.PageNum--;
		return await Search(Ct);
	}

	/// 下一頁。
	protected async Task<nil> OnNextPage(VmPageBar PageBar, CT Ct){
		if(PageBar.TotPageCnt is u64 TotalPage && PageBar.PageNum >= TotalPage){
			return NIL;
		}
		PageBar.PageNum++;
		return await Search(Ct);
	}

	/// 請求打開詳情頁（row=null 表示新增）。
	public nil OpenDetail(RowUserLang? Row = null){
		if(IsSelectMode){
			if(Row?.Raw is not null){
				FnOnSelected?.Invoke(Row.Raw);
			}
			return NIL;
		}
		OnOpenDetailRequested?.Invoke(Row);
		return NIL;
	}

	/// 設為選擇模式，點行後回調選中項而不再進入詳情頁。
	public nil SetSelectMode(Action<PoUserLang> FnOnSelected){
		IsSelectMode = true;
		this.FnOnSelected = FnOnSelected;
		return NIL;
	}

	/// 將 `PoWord.Lang` 中尚未註冊的語言補全到 `PoUserLang`，並刷新當前列表。
	public async Task<nil> AddAllUnregisteredUserLangs(CT Ct = default){
		if(AnyNull(SvcUserLang, UserCtxMgr)){
			return NIL;
		}
		try{
			await SvcUserLang.AddAllUnregisteredUserLangs(UserCtxMgr.GetDbUserCtx(), Ct);
			ShowToast(I18n[K.AddedAllUnregisteredUserLangs]);
			return await InitSearch(Ct);
		}catch(Exception e){
			HandleErr(e);
		}
		return NIL;
	}

	/// 由 View 層監聽，收到後在 View 中實例化詳情頁並導航。
	public event Action<RowUserLang?>? OnOpenDetailRequested;
}


