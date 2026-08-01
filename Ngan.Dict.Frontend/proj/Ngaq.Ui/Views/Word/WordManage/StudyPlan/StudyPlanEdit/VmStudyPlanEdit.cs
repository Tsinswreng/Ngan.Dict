namespace Ngaq.Ui.Views.Word.WordManage.StudyPlan.StudyPlanEdit;

using System;
using System.Collections.ObjectModel;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Shared.StudyPlan.Models;
using Ngaq.Core.Shared.StudyPlan.Models.Po.PreFilter;
using Ngaq.Core.Shared.StudyPlan.Models.Po.StudyPlan;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightArg;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightCalculator;
using Ngaq.Core.Shared.StudyPlan.Svc;
using Ngaq.Core.Tools;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Tools;
using Tsinswreng.CsTools;
using Ctx = VmStudyPlanEdit;using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

/// StudyPlan 編輯頁 ViewModel。
/// 支持新增、修改與刪除。
public partial class VmStudyPlanEdit: ViewModelBase, IMk<Ctx>{
	protected VmStudyPlanEdit(){}

	public static Ctx Mk(){
		return new Ctx();
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmStudyPlanEdit(){
		#if DEBUG
		{
			var o = new Ctx();
			Samples.Add(o);
		}
		#endif
	}

	ISvcStudyPlan? SvcStudyPlan{get;set;}
	IFrontendUserCtxMgr? UserCtxMgr{get;set;}
	i64 RefRefreshVer{get;set;} = 0;

	public VmStudyPlanEdit(
		ISvcStudyPlan? SvcStudyPlan
		,IFrontendUserCtxMgr? UserCtxMgr
	){
		this.SvcStudyPlan = SvcStudyPlan;
		this.UserCtxMgr = UserCtxMgr;
	}

	public bool IsCreateMode{
		get;
		set{SetProperty(ref field, value);}
	} = false;

	public str LastError{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public bool HasError => !str.IsNullOrWhiteSpace(LastError);

	public str PoIdText{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public str PoUniqName{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public str PoDescr{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public str PreFilterIdText{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public str PreFilterUniqNameText{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public str WeightCalculatorIdText{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public str WeightCalculatorUniqNameText{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public str WeightArgIdText{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public str WeightArgUniqNameText{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public PoStudyPlan PoStudyPlan{
		get;
		set{SetProperty(ref field, value);}
	} = new();

	public BoStudyPlan BoStudyPlan{
		get;
		set{SetProperty(ref field, value);}
	} = new();

	public nil SetCreateMode(bool IsCreate){
		IsCreateMode = IsCreate;
		return NIL;
	}

	public nil FromPoStudyPlan(PoStudyPlan? PoStudyPlan){
		this.PoStudyPlan = ClonePoStudyPlan(PoStudyPlan);
		BoStudyPlan = new BoStudyPlan{
			PoStudyPlan = ClonePoStudyPlan(PoStudyPlan),
		};
		IsCreateMode = PoStudyPlan is null || PoStudyPlan.Id == IdStudyPlan.Zero;
		SyncFromBo();
		_ = RefreshRefUniqNames();
		return NIL;
	}

	public nil FromBoStudyPlan(BoStudyPlan? BoStudyPlan){
		this.BoStudyPlan = CloneBoStudyPlan(BoStudyPlan);
		this.PoStudyPlan = ClonePoStudyPlan(this.BoStudyPlan.PoStudyPlan);
		IsCreateMode = this.PoStudyPlan.Id == IdStudyPlan.Zero;
		SyncFromBo();
		return NIL;
	}

	public async Task<nil> Save(CT Ct = default){
		if(AnyNull(SvcStudyPlan, UserCtxMgr)){
			return NIL;
		}
		try{
			var po = BuildPoFromFields();
			var dbCtx = UserCtxMgr.GetDbUserCtx();
			if(IsCreateMode){
				await SvcStudyPlan.OrdAddStudyPlan(dbCtx, ToolAsyE.ToAsyE([po]), Ct);
			}else{
				await SvcStudyPlan.OrdUpdStudyPlan(dbCtx, ToolAsyE.ToAsyE([po]), Ct);
			}
			PoStudyPlan = po;
			BoStudyPlan.PoStudyPlan = ClonePoStudyPlan(po);
			SyncFromBo();
			IsCreateMode = false;
			LastError = "";
			OnPropertyChanged(nameof(HasError));
			ShowToast(I18n[K.Saved]);
		}catch(Exception e){
			LastError = e.Message;
			OnPropertyChanged(nameof(HasError));
			HandleErr(e);
		}
		return NIL;
	}

	public async Task<nil> Delete(CT Ct = default){
		if(AnyNull(SvcStudyPlan, UserCtxMgr)){
			return NIL;
		}
		try{
			var po = BuildPoFromFields();
			if(!IsCreateMode && po.Id != IdStudyPlan.Zero){
				await SvcStudyPlan.OrdSoftDelStudyPlan(UserCtxMgr.GetDbUserCtx(), ToolAsyE.ToAsyE([po]), Ct);
			}
			PoStudyPlan = new PoStudyPlan();
			BoStudyPlan = new BoStudyPlan();
			IsCreateMode = true;
			SyncFromBo();
			LastError = "";
			OnPropertyChanged(nameof(HasError));
			ShowToast(I18n[K.Deleted]);
		}catch(Exception e){
			LastError = e.Message;
			OnPropertyChanged(nameof(HasError));
			HandleErr(e);
		}
		return NIL;
	}

	void SyncFromBo(){
		var po = PoStudyPlan ?? new PoStudyPlan();
		PoIdText = po.Id.ToString();
		PoUniqName = po.UniqName ?? "";
		PoDescr = po.Descr ?? "";
		PreFilterIdText = po.PreFilterId.ToString();
		PreFilterUniqNameText = BoStudyPlan.PoPreFilter?.UniqName ?? "";
		if(str.IsNullOrWhiteSpace(PreFilterUniqNameText) && po.PreFilterId != IdPreFilter.Zero){
			PreFilterUniqNameText = po.PreFilterId.ToString();
		}
		WeightCalculatorIdText = po.WeightCalculatorId.ToString();
		WeightCalculatorUniqNameText = BoStudyPlan.PoWeightCalculator?.UniqName ?? "";
		WeightArgIdText = po.WeightArgId.ToString();
		WeightArgUniqNameText = BoStudyPlan.PoWeightArg?.UniqName ?? "";
	}

	PoStudyPlan BuildPoFromFields(){
		var po = ClonePoStudyPlan(PoStudyPlan);
		po.UniqName = str.IsNullOrWhiteSpace(PoUniqName) ? null : PoUniqName.Trim();
		po.Descr = PoDescr?.Trim() ?? "";
		return po;
	}

	/// <summary>
	/// 由 PoStudyPlan 的外鍵 Id 反查三個關聯實體名，解決僅從列表打開編輯頁時名稱為空的問題。
	/// </summary>
	async Task<nil> RefreshRefUniqNames(CT Ct = default){
		if(AnyNull(SvcStudyPlan, UserCtxMgr)){
			return NIL;
		}
		var curVer = ++RefRefreshVer;
		try{
			var dbCtx = UserCtxMgr.GetDbUserCtx();
			var po = PoStudyPlan ?? new PoStudyPlan();
			var preFilterId = po.PreFilterId;
			var weightCalculatorId = po.WeightCalculatorId;
			var weightArgId = po.WeightArgId;
			if(!po.PreFilterId.IsNullOrDefault()){
				BoStudyPlan.PoPreFilter = await SvcStudyPlan
					.OrdGetPreFilterById(dbCtx, ToolAsyE.ToAsyE([po.PreFilterId]), Ct)
					.FirstOrDefaultAsync(Ct);
			}
			if(curVer != RefRefreshVer){
				return NIL;
			}
			if(!po.WeightCalculatorId.IsNullOrDefault()){
				BoStudyPlan.PoWeightCalculator = await SvcStudyPlan
					.OrdGetWeightCalculatorById(dbCtx, ToolAsyE.ToAsyE([po.WeightCalculatorId]), Ct)
					.FirstOrDefaultAsync(Ct);
			}
			if(curVer != RefRefreshVer){
				return NIL;
			}
			if(!po.WeightArgId.IsNullOrDefault()){
				BoStudyPlan.PoWeightArg = await SvcStudyPlan
					.OrdGetWeightArgById(dbCtx, ToolAsyE.ToAsyE([po.WeightArgId]), Ct)
					.FirstOrDefaultAsync(Ct);
			}
			if(curVer != RefRefreshVer){
				return NIL;
			}
			var latestPo = PoStudyPlan ?? new PoStudyPlan();
			if(latestPo.PreFilterId != preFilterId || latestPo.WeightCalculatorId != weightCalculatorId || latestPo.WeightArgId != weightArgId){
				return NIL;
			}
			SyncFromBo();
		}catch(Exception e){
			HandleErr(e);
		}
		return NIL;
	}

	public nil ApplySelectedPreFilter(PoPreFilter? Po){
		if(Po is null){
			return NIL;
		}
		RefRefreshVer++;
		PoStudyPlan.PreFilterId = Po.Id;
		PreFilterIdText = Po.Id.ToString();
		PreFilterUniqNameText = Po.UniqName ?? "";
		if(str.IsNullOrWhiteSpace(PreFilterUniqNameText)){
			PreFilterUniqNameText = Po.Id.ToString();
		}
		BoStudyPlan.PoPreFilter = Po;
		return NIL;
	}

	public nil ApplySelectedWeightCalculator(PoWeightCalculator? Po){
		if(Po is null){
			return NIL;
		}
		RefRefreshVer++;
		PoStudyPlan.WeightCalculatorId = Po.Id;
		WeightCalculatorIdText = Po.Id.ToString();
		WeightCalculatorUniqNameText = Po.UniqName ?? "";
		BoStudyPlan.PoWeightCalculator = Po;
		return NIL;
	}

	public nil ApplySelectedWeightArg(PoWeightArg? Po){
		if(Po is null){
			return NIL;
		}
		RefRefreshVer++;
		PoStudyPlan.WeightArgId = Po.Id;
		WeightArgIdText = Po.Id.ToString();
		WeightArgUniqNameText = Po.UniqName ?? "";
		BoStudyPlan.PoWeightArg = Po;
		return NIL;
	}

	static BoStudyPlan CloneBoStudyPlan(BoStudyPlan? src){
		src ??= new BoStudyPlan();
		return new BoStudyPlan{
			PoStudyPlan = ClonePoStudyPlan(src.PoStudyPlan),
			PoPreFilter = src.PoPreFilter,
			PreFilter = src.PreFilter,
			PoWeightCalculator = src.PoWeightCalculator,
			WeightCalctr = src.WeightCalctr,
			PoWeightArg = src.PoWeightArg,
			WeightArg = src.WeightArg,
		};
	}

	static PoStudyPlan ClonePoStudyPlan(PoStudyPlan? src){
		src ??= new PoStudyPlan();
		return new PoStudyPlan{
			DbCreatedAt = src.DbCreatedAt,
			DbUpdatedAt = src.DbUpdatedAt,
			DelAt = src.DelAt,
			BizCreatedAt = src.BizCreatedAt,
			BizUpdatedAt = src.BizUpdatedAt,
			Id = src.Id,
			Owner = src.Owner,
			UniqName = src.UniqName,
			Descr = src.Descr,
			PreFilterId = src.PreFilterId,
			WeightCalculatorId = src.WeightCalculatorId,
			WeightArgId = src.WeightArgId,
		};
	}
}


