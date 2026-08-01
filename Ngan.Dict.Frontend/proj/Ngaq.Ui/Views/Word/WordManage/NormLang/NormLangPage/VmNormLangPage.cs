namespace Ngaq.Ui.Views.Word.WordManage.NormLang.NormLangPage;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ngaq.Core.Infra.Cfg;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.Dictionary.Models.Po.NormLang;
using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Core.Shared.Word.Svc;
using Ngaq.Ui.Components.PageBar;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan;
using Tsinswreng.CsTools;

using Ctx = VmNormLangPage;using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;
using Tsinswreng.CsCfg;

public partial class VmNormLangPage: ViewModelBase, IMk<Ctx>{
	void Init(){
		PageBar = VmPageBar.Mk();
		PageBar.PageSize = 20;
		PageBar.FnPrevPage = OnPrevPage;
		PageBar.FnNextPage = OnNextPage;
	}

	protected VmNormLangPage(){
		Init();
	}

	public static Ctx Mk(){
		return new Ctx();
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmNormLangPage(){
		#if DEBUG
		{
			var o = new Ctx();
			Samples.Add(o);
		}
		#endif
	}

	ISvcNormLang? SvcNormLang{get;set;}
	IFrontendUserCtxMgr? UserCtxMgr{get;set;}

	public VmNormLangPage(
		ISvcNormLang? SvcNormLang
		,IFrontendUserCtxMgr? UserCtxMgr
	):this(){
		this.SvcNormLang = SvcNormLang;
		this.UserCtxMgr = UserCtxMgr;
		Init();
	}

	public VmPageBar PageBar{get;set;} = null!;

	public str Input{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public ObservableCollection<RowNormLang> Rows{get;set;} = [];

	public bool CanCreate => true;

	public bool IsSelectMode{
		get;
		set{SetProperty(ref field, value);}
	} = false;

	Action<PoNormLang>? FnOnSelected{get;set;}

	public class RowNormLang{
		public u64 UiIdx{get;set;}
		public str UiIdxText{get;set;} = "";
		public str Type{get;set;} = "";
		public str Code{get;set;} = "";
		public str NativeName{get;set;} = "";
		public str TranslatedName{get;set;} = "";
		public str ModifiedTime{get;set;} = "";
		public PoNormLang? Raw{get;set;} = null;
	}

	public async Task<nil> InitSearch(CT Ct){
		PageBar.PageNum = 1;
		return await Search(Ct);
	}

	public async Task<nil> Search(CT Ct = default){
		if(AnyNull(SvcNormLang, UserCtxMgr)){
			return NIL;
		}
		try{
			var pageQry = PageBar.ToPageQry();
			pageQry.WantTotCnt = true;
			var req = new ReqPageNormLang{
				PageQry = pageQry,
				SearchText = str.IsNullOrWhiteSpace(Input) ? null : Input.Trim(),
			};

			var page = await SvcNormLang.PageNormLang(UserCtxMgr.GetDbUserCtx(), req, Ct);
			PageBar.FromPageResultInfo(page);

			Rows.Clear();
			var startUiIdx = page.PageIdx * page.PageSize;
			var localIdx = 0UL;
			var Pos = await CollectRows(page.DataAsyE, Ct);
			var TranslatedNames = await GetTranslatedNames(Pos, Ct);
			for(var i = 0; i < Pos.Count; i++){
				var po = Pos[i];
				localIdx++;
				var uiIdx = startUiIdx + localIdx;
				Rows.Add(new RowNormLang{
					UiIdx = uiIdx,
					UiIdxText = uiIdx.ToString(),
					Type = po.Type.ToString(),
					Code = po.Code ?? "",
					NativeName = po.NativeName ?? "",
					TranslatedName = i < TranslatedNames.Count ? (TranslatedNames[i] ?? "") : "",
					ModifiedTime = ToolStudyPlanView.FormatUpdatedDateShort(po.BizUpdatedAt, po.BizCreatedAt),
					Raw = po,
				});
			}
		}catch(Exception e){
			HandleErr(e);
		}
		return NIL;
	}

	/// 收集當前頁結果。單頁數量有限，便於後續按序填充翻譯名稱。
	private static async Task<List<PoNormLang>> CollectRows(
		IAsyncEnumerable<PoNormLang>? Rows,
		CT Ct
	){
		List<PoNormLang> Ans = [];
		if(Rows is null){
			return Ans;
		}
		await foreach(var Row in Rows.WithCancellation(Ct)){
			Ans.Add(Row);
		}
		return Ans;
	}

	/// 按頁面語言批量獲取翻譯名稱，返回順序與 Pos 一一對應。
	private async Task<List<str?>> GetTranslatedNames(IList<PoNormLang> Pos, CT Ct){
		List<str?> Ans = [];
		if(AnyNull(SvcNormLang, UserCtxMgr) || Pos.Count == 0){
			return Ans;
		}
		var UiLang = KeysClientCfg.Lang.GetFrom(AppCfg.Inst) ?? "en";
		var TargetLang = new NormLang{
			Type = ELangIdentType.Bcp47,
			Code = UiLang,
		};
		var Names = SvcNormLang.OrdGetTranslatedName(
			UserCtxMgr.GetDbUserCtx(),
			TargetLang,
			ToolAsyE.ToAsyE(Pos.Select(x=>(INormLang)new NormLang{
				Type = x.Type,
				Code = x.Code,
			})),
			Ct
		);
		await foreach(var Name in Names.WithCancellation(Ct)){
			Ans.Add(Name);
		}
		return Ans;
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
		if(PageBar.TotPageCnt is u64 TotalPage && TotalPage > 0 && PageBar.PageNum >= TotalPage){
			return NIL;
		}
		PageBar.PageNum++;
		return await Search(Ct);
	}

	public nil OpenDetail(RowNormLang? Row = null){
		if(IsSelectMode){
			if(Row?.Raw is not null){
				FnOnSelected?.Invoke(Row.Raw);
				return NIL;
			}
			// 選擇模式下仍允許從「新增」按鈕進入詳情頁創建語言。
			if(Row is null){
				OnOpenDetailRequested?.Invoke(null);
			}
			return NIL;
		}
		OnOpenDetailRequested?.Invoke(Row);
		return NIL;
	}

	public nil SetSelectMode(Action<PoNormLang> FnOnSelected){
		IsSelectMode = true;
		this.FnOnSelected = FnOnSelected;
		return NIL;
	}

	public async Task<nil> InitBuiltinNormLang(CT Ct = default){
		if(AnyNull(SvcNormLang, UserCtxMgr)){
			return NIL;
		}
		try{
			await SvcNormLang.InitBuiltinNormLang(UserCtxMgr.GetDbUserCtx(), Ct);
			ShowToast(I18n[K.InitializedBuiltinNormLang]);
			return await InitSearch(Ct);
		}catch(Exception e){
			HandleErr(e);
		}
		return NIL;
	}

	public event Action<RowNormLang?>? OnOpenDetailRequested;
}

