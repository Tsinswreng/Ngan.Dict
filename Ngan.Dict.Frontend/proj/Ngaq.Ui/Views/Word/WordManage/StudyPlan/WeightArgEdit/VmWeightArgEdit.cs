namespace Ngaq.Ui.Views.Word.WordManage.StudyPlan.WeightArgEdit;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightArg;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightCalculator;
using Ngaq.Core.Shared.StudyPlan.Svc;
using Ngaq.Core.Tools;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.WeightArgPayloadJsonEdit;
using Tsinswreng.CsTools;

using Ctx = VmWeightArgEdit;using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

/// WeightArg 編輯頁 ViewModel。
/// 負責新增/修改/刪除，並提供 Payload JSON 子頁入口。
public partial class VmWeightArgEdit: ViewModelBase, IMk<Ctx>{
	protected VmWeightArgEdit(){}

	public static Ctx Mk(){
		return new Ctx();
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmWeightArgEdit(){
		#if DEBUG
		{
			var o = new Ctx();
			Samples.Add(o);
		}
		#endif
	}

	ISvcStudyPlan? SvcStudyPlan{get;set;}
	IFrontendUserCtxMgr? UserCtxMgr{get;set;}

	public VmWeightArgEdit(
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

	public IReadOnlyList<EWeightArgType> TypeValues{get;} = Enum
		.GetValues<EWeightArgType>()
		.Where(x=>x != EWeightArgType.Unknown)
		.ToList();
	public IReadOnlyList<str> TypeOptions{get;} = Enum
		.GetValues<EWeightArgType>()
		.Where(x=>x != EWeightArgType.Unknown)
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
	} = 1;

	public str WeightCalculatorUniqNameText{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public IdWeightCalculator WeightCalculatorId{get;set;} = IdWeightCalculator.Zero;

	/// 主頁只顯示 Payload 預覽，完整編輯放到 JSON 子頁。
	public str PayloadTextPreview{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	/// 實際要保存的 Payload 文本。
	public str PayloadText{
		get;
		set{
			if(SetProperty(ref field, value)){
				RefreshPayloadPreview();
			}
		}
	} = "";

	/// 當前編輯中的實體快照。
	public PoWeightArg PoWeightArg{
		get;
		set{SetProperty(ref field, value);}
	} = new PoWeightArg{
		Type = EWeightArgType.Json,
	};

	/// 顯式設置當前編輯模式，供列表頁跳轉時調用。
	public nil SetCreateMode(bool IsCreate){
		IsCreateMode = IsCreate;
		return NIL;
	}

	/// 由列表頁傳入 Po 實體初始化編輯頁。
	public nil FromPoWeightArg(PoWeightArg? PoWeightArg){
		this.PoWeightArg = ClonePoWeightArg(PoWeightArg);
		IsCreateMode = PoWeightArg is null || PoWeightArg.Id == IdWeightArg.Zero;
		WeightCalculatorUniqNameText = "";
		SyncFromPo();
		_ = RefreshWeightCalculatorUniqName();
		return NIL;
	}

	/// 打開 Payload JSON 子編輯頁。
	public nil OpenPayloadJsonEditor(){
		var view = new ViewWeightArgPayloadJsonEdit();
		view.Ctx?.Load(PayloadText, OnPayloadJsonApplied);
		ViewNavi?.GoTo(ToolView.WithTitle(I18n[K.WeightArgPayloadJson], view));
		return NIL;
	}

	/// 保存到後端。新增走 BatAddWeightArg，編輯走 BatUpdWeightArg。
	public async Task<nil> Save(CT Ct = default){
		if(AnyNull(SvcStudyPlan, UserCtxMgr)){
			return NIL;
		}
		try{
			var po = BuildPoFromFields();
			var dbCtx = UserCtxMgr.GetDbUserCtx();
			if(IsCreateMode){
				await SvcStudyPlan.OrdAddWeightArg(dbCtx, ToolAsyE.ToAsyE([po]), Ct);
			}else{
				await SvcStudyPlan.OrdUpdWeightArg(dbCtx, ToolAsyE.ToAsyE([po]), Ct);
			}

			PoWeightArg = po;
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

	/// 軟刪除當前 WeightArg。新增模式下只清空界面，不調後端刪除接口。
	public async Task<nil> Delete(CT Ct = default){
		if(AnyNull(SvcStudyPlan, UserCtxMgr)){
			return NIL;
		}
		try{
			var po = BuildPoFromFields();
			if(!IsCreateMode && po.Id != IdWeightArg.Zero){
				await SvcStudyPlan.OrdSoftDelWeightArg(UserCtxMgr.GetDbUserCtx(), ToolAsyE.ToAsyE([po]), Ct);
			}
			PoWeightArg = new PoWeightArg{
				Type = EWeightArgType.Json,
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

	void OnPayloadJsonApplied(str PayloadJson){
		PayloadText = PayloadJson ?? "";
	}

	public nil ApplySelectedWeightCalculator(PoWeightCalculator? Po){
		if(Po is null){
			return NIL;
		}
		PoWeightArg.WeightCalculatorId = Po.Id;
		WeightCalculatorId = Po.Id;
		WeightCalculatorUniqNameText = Po.UniqName ?? "";
		return NIL;
	}

	void SyncFromPo(){
		var po = PoWeightArg ?? new PoWeightArg{
			Type = EWeightArgType.Json,
		};
		PoIdText = po.Id.ToString();
		PoUniqName = po.UniqName ?? "";
		PoDescr = po.Descr ?? "";
		WeightCalculatorId = po.WeightCalculatorId;
		if(WeightCalculatorId.IsNullOrDefault()){
			WeightCalculatorUniqNameText = "";
		}
		PoTypeIndex = GetTypeIndex(po.Type);
		PayloadText = po.Text ?? "";
		LastError = "";
		OnPropertyChanged(nameof(HasError));
	}

	/// <summary>
	/// 根據當前 <see cref="PoWeightArg.WeightCalculatorId"/> 回查算法名稱，
	/// 供 GUI 顯示 UniqName（界面不展示 Id）。
	/// </summary>
	public async Task<nil> RefreshWeightCalculatorUniqName(CT Ct = default){
		if(AnyNull(SvcStudyPlan, UserCtxMgr)){
			return NIL;
		}
		try{
			if(WeightCalculatorId.IsNullOrDefault()){
				WeightCalculatorUniqNameText = "";
				return NIL;
			}
			var dbCtx = UserCtxMgr.GetDbUserCtx();
			var po = await SvcStudyPlan
				.OrdGetWeightCalculatorById(dbCtx, ToolAsyE.ToAsyE([WeightCalculatorId]), Ct)
				.FirstOrDefaultAsync(Ct);
			WeightCalculatorUniqNameText = po?.UniqName ?? "";
		}catch(Exception e){
			HandleErr(e);
		}
		return NIL;
	}

	PoWeightArg BuildPoFromFields(){
		var po = ClonePoWeightArg(PoWeightArg);
		po.UniqName = str.IsNullOrWhiteSpace(PoUniqName) ? null : PoUniqName.Trim();
		po.Descr = PoDescr?.Trim() ?? "";
		po.WeightCalculatorId = WeightCalculatorId;
		po.Type = GetTypeByIndex(PoTypeIndex);
		po.Text = PayloadText;
		po.Binary = null;
		return po;
	}

	void RefreshPayloadPreview(){
		if(str.IsNullOrWhiteSpace(PayloadText)){
			PayloadTextPreview = "";
			return;
		}
		const int maxLen = 320;
		PayloadTextPreview = PayloadText.Length <= maxLen
			? PayloadText
			: PayloadText[..maxLen] + "...";
	}

	static PoWeightArg ClonePoWeightArg(PoWeightArg? src){
		src ??= new PoWeightArg{
			Type = EWeightArgType.Json,
		};
		return new PoWeightArg{
			DbCreatedAt = src.DbCreatedAt,
			DbUpdatedAt = src.DbUpdatedAt,
			DelAt = src.DelAt,
			BizCreatedAt = src.BizCreatedAt,
			BizUpdatedAt = src.BizUpdatedAt,
			Id = src.Id,
			Owner = src.Owner,
			UniqName = src.UniqName,
			Type = src.Type,
			Text = src.Text,
			Binary = src.Binary?.ToArray() ?? [],
			WeightCalculatorId = src.WeightCalculatorId,
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

	i32 GetTypeIndex(EWeightArgType type){
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

	EWeightArgType GetTypeByIndex(i32 index){
		if(TypeValues.Count == 0){
			return EWeightArgType.Json;
		}
		var i = ClampIndex(index, TypeValues.Count);
		return TypeValues[i];
	}
}


