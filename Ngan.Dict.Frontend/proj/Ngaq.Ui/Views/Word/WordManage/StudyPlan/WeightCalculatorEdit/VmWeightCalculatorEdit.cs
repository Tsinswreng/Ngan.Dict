namespace Ngaq.Ui.Views.Word.WordManage.StudyPlan.WeightCalculatorEdit;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightCalculator;
using Ngaq.Core.Shared.StudyPlan.Svc;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Tools;
using Tsinswreng.CsTools;
using Ctx = VmWeightCalculatorEdit;using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

/// WeightCalculator 編輯頁 ViewModel。
/// 負責新增/修改/刪除。
public partial class VmWeightCalculatorEdit: ViewModelBase, IMk<Ctx>{
	protected VmWeightCalculatorEdit(){}

	public static Ctx Mk(){
		return new Ctx();
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmWeightCalculatorEdit(){
		#if DEBUG
		{
			var o = new Ctx();
			Samples.Add(o);
		}
		#endif
	}

	ISvcStudyPlan? SvcStudyPlan{get;set;}
	IFrontendUserCtxMgr? UserCtxMgr{get;set;}

	public VmWeightCalculatorEdit(
		ISvcStudyPlan? SvcStudyPlan
		,IFrontendUserCtxMgr? UserCtxMgr
	){
		this.SvcStudyPlan = SvcStudyPlan;
		this.UserCtxMgr = UserCtxMgr;
	}

	/// 當前頁面是否爲新增模式。true=新增，false=編輯既有實體。
	public bool IsCreateMode{
		get;
		set{SetProperty(ref field, value);}
	} = true;

	public str LastError{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public bool HasError => !str.IsNullOrWhiteSpace(LastError);

	public IReadOnlyList<EWeightCalculatorType> TypeValues{get;} = Enum
		.GetValues<EWeightCalculatorType>()
		.Where(x=>x != EWeightCalculatorType.Unknown)
		.ToList();
	public IReadOnlyList<str> TypeOptions{get;} = Enum
		.GetValues<EWeightCalculatorType>()
		.Where(x=>x != EWeightCalculatorType.Unknown)
		.Select(x=>x.ToString())
		.ToList();
	public bool ShowTypeField => TypeOptions.Count > 1;

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

	public i32 PoTypeIndex{
		get;
		set{SetProperty(ref field, value);}
	} = 2;

	public str PayloadText{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	/// 當前編輯中的實體快照。
	public PoWeightCalculator PoWeightCalculator{
		get;
		set{SetProperty(ref field, value);}
	} = new PoWeightCalculator{
		Type = EWeightCalculatorType.JsV1,
	};

	/// 顯式設置當前編輯模式，供列表頁跳轉時調用。
	public nil SetCreateMode(bool IsCreate){
		IsCreateMode = IsCreate;
		return NIL;
	}

	/// 由列表頁傳入 Po 實體初始化編輯頁。
	public nil FromPoWeightCalculator(PoWeightCalculator? PoWeightCalculator){
		this.PoWeightCalculator = ClonePoWeightCalculator(PoWeightCalculator);
		IsCreateMode = PoWeightCalculator is null || PoWeightCalculator.Id == IdWeightCalculator.Zero;
		SyncFromPo();
		return NIL;
	}

	/// 保存到後端。新增走 BatAddWeightCalculator，編輯走 BatUpdWeightCalculator。
	public async Task<nil> Save(CT Ct = default){
		if(AnyNull(SvcStudyPlan, UserCtxMgr)){
			return NIL;
		}
		try{
			var po = BuildPoFromFields();
			var dbCtx = UserCtxMgr.GetDbUserCtx();
			if(IsCreateMode){
				await SvcStudyPlan.OrdAddWeightCalculator(dbCtx, ToolAsyE.ToAsyE([po]), Ct);
			}else{
				await SvcStudyPlan.OrdUpdWeightCalculator(dbCtx, ToolAsyE.ToAsyE([po]), Ct);
			}

			PoWeightCalculator = po;
			IsCreateMode = false;
			SyncFromPo();
			LastError = "";
			OnPropertyChanged(nameof(HasError));
			ShowToast(I18n[K.Saved]);
		}catch(Exception e){
			HandleErr(e);
		}
		return NIL;
	}

	/// 軟刪除當前 WeightCalculator。新增模式下只清空界面，不調後端刪除接口。
	public async Task<nil> Delete(CT Ct = default){
		if(AnyNull(SvcStudyPlan, UserCtxMgr)){
			return NIL;
		}
		try{
			var po = BuildPoFromFields();
			if(!IsCreateMode && po.Id != IdWeightCalculator.Zero){
				await SvcStudyPlan.OrdSoftDelWeightCalculator(UserCtxMgr.GetDbUserCtx(), ToolAsyE.ToAsyE([po]), Ct);
			}
			PoWeightCalculator = new PoWeightCalculator{
				Type = EWeightCalculatorType.JsV1,
			};
			IsCreateMode = true;
			SyncFromPo();
			LastError = "";
			OnPropertyChanged(nameof(HasError));
			ShowToast(I18n[K.Deleted]);
		}catch(Exception e){
			HandleErr(e);
		}
		return NIL;
	}

	void SyncFromPo(){
		var po = PoWeightCalculator ?? new PoWeightCalculator{
			Type = EWeightCalculatorType.JsV1,
		};
		PoIdText = po.Id.ToString();
		PoUniqName = po.UniqName ?? "";
		PoDescr = po.Descr ?? "";
		PoTypeIndex = GetTypeIndex(po.Type);
		PayloadText = po.Text ?? "";
		LastError = "";
		OnPropertyChanged(nameof(HasError));
	}

	PoWeightCalculator BuildPoFromFields(){
		var po = ClonePoWeightCalculator(PoWeightCalculator);
		po.UniqName = str.IsNullOrWhiteSpace(PoUniqName) ? null : PoUniqName.Trim();
		po.Descr = PoDescr?.Trim() ?? "";
		po.Type = GetTypeByIndex(PoTypeIndex);
		po.Text = PayloadText;
		po.Binary = null;
		return po;
	}

	static PoWeightCalculator ClonePoWeightCalculator(PoWeightCalculator? src){
		src ??= new PoWeightCalculator{
			Type = EWeightCalculatorType.JsV1,
		};
		return new PoWeightCalculator{
			DbCreatedAt = src.DbCreatedAt,
			DbUpdatedAt = src.DbUpdatedAt,
			DelAt = src.DelAt,
			Id = src.Id,
			Owner = src.Owner,
			UniqName = src.UniqName,
			Type = src.Type,
			Text = src.Text,
			Binary = src.Binary?.ToArray() ?? [],
			Descr = src.Descr,
		};
	}

	static i32 ClampIndex(i32 value, i32 count){
		if(count <= 0){
			return 0;
		}
		if(value < 0){
			return 0;
		}
		if(value >= count){
			return count - 1;
		}
		return value;
	}

	i32 GetTypeIndex(EWeightCalculatorType type){
		if(TypeValues.Count == 0){
			return 0;
		}
		for(i32 i = 0; i < TypeValues.Count; i++){
			if(TypeValues[i] == type){
				return i;
			}
		}
		return 0;
	}

	EWeightCalculatorType GetTypeByIndex(i32 index){
		if(TypeValues.Count == 0){
			return EWeightCalculatorType.JsV1;
		}
		var i = ClampIndex(index, TypeValues.Count);
		return TypeValues[i];
	}
}


