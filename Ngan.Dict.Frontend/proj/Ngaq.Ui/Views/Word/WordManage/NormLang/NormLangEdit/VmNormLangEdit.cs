namespace Ngaq.Ui.Views.Word.WordManage.NormLang.NormLangEdit;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.Dictionary.Models.Po.NormLang;
using Ngaq.Core.Shared.Word.Svc;
using Ngaq.Ui.Infra;
using Tsinswreng.CsTools;
using Ctx = VmNormLangEdit;using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

/// NormLang 編輯頁 ViewModel。
public partial class VmNormLangEdit: ViewModelBase, IMk<Ctx>{
	protected VmNormLangEdit(){}

	public static Ctx Mk(){
		return new Ctx();
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmNormLangEdit(){
		#if DEBUG
		{
			var o = new Ctx();
			Samples.Add(o);
		}
		#endif
	}

	ISvcNormLang? SvcNormLang{get;set;}
	IFrontendUserCtxMgr? UserCtxMgr{get;set;}

	public VmNormLangEdit(
		ISvcNormLang? SvcNormLang
		,IFrontendUserCtxMgr? UserCtxMgr
	){
		this.SvcNormLang = SvcNormLang;
		this.UserCtxMgr = UserCtxMgr;
	}

	public bool IsCreateMode{
		get;
		set{SetProperty(ref field, value);}
	} = true;

	public str LastError{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public bool HasError => !str.IsNullOrWhiteSpace(LastError);

	public IReadOnlyList<ELangIdentType> TypeValues{get;} = Enum
		.GetValues<ELangIdentType>()
		.Where(x=>x != ELangIdentType.Unknown)
		.ToList();

	public IReadOnlyList<str> TypeOptions{get;} = Enum
		.GetValues<ELangIdentType>()
		.Where(x=>x != ELangIdentType.Unknown)
		.Select(x=>x.ToString())
		.ToList();

	public bool ShowTypeField => true;

	public str PoIdText{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public str PoCode{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public str PoNativeName{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public str PoEnglishName{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public str PoWeightText{
		get;
		set{SetProperty(ref field, value);}
	} = "0";

	public i32 PoTypeIndex{
		get;
		set{SetProperty(ref field, value);}
	} = 0;

	public PoNormLang PoNormLang{
		get;
		set{SetProperty(ref field, value);}
	} = new PoNormLang{
		Type = ELangIdentType.Bcp47,
	};

	public nil SetCreateMode(bool IsCreate){
		IsCreateMode = IsCreate;
		return NIL;
	}

	public nil FromPoNormLang(PoNormLang? PoNormLang){
		this.PoNormLang = ClonePoNormLang(PoNormLang);
		IsCreateMode = PoNormLang is null;
		SyncFromPo();
		return NIL;
	}

	public async Task<nil> Save(CT Ct = default){
		if(AnyNull(SvcNormLang, UserCtxMgr)){
			return NIL;
		}
		try{
			var po = BuildPoFromFields();
			var dbCtx = UserCtxMgr.GetDbUserCtx();
			if(IsCreateMode){
				po.Owner = dbCtx.UserCtx.UserId;
				await SvcNormLang.OrdAddNormLang(dbCtx, ToolAsyE.ToAsyE([po]), Ct);
			}else{
				await SvcNormLang.OrdUpdNormLang(dbCtx, ToolAsyE.ToAsyE([po]), Ct);
			}

			PoNormLang = po;
			IsCreateMode = false;
			SyncFromPo();
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
		if(AnyNull(SvcNormLang, UserCtxMgr)){
			return NIL;
		}
		try{
			var po = BuildPoFromFields();
			if(!IsCreateMode){
				await SvcNormLang.OrdSoftDelNormLang(UserCtxMgr.GetDbUserCtx(), ToolAsyE.ToAsyE([po]), Ct);
			}
			PoNormLang = new PoNormLang{
				Type = ELangIdentType.Bcp47,
			};
			IsCreateMode = true;
			SyncFromPo();
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

	void SyncFromPo(){
		var po = PoNormLang ?? new PoNormLang{
			Type = ELangIdentType.Bcp47,
		};
		PoIdText = po.Id.ToString();
		PoCode = po.Code ?? "";
		PoNativeName = po.NativeName ?? "";
		PoEnglishName = po.EnglishName ?? "";
		PoWeightText = po.Weight.ToString(CultureInfo.InvariantCulture);
		PoTypeIndex = GetTypeIndex(po.Type);
		LastError = "";
		OnPropertyChanged(nameof(HasError));
	}

	PoNormLang BuildPoFromFields(){
		var po = ClonePoNormLang(PoNormLang);
		po.Type = GetTypeByIndex(PoTypeIndex);
		po.Code = PoCode?.Trim() ?? "";
		po.NativeName = PoNativeName?.Trim() ?? "";
		po.EnglishName = PoEnglishName?.Trim() ?? "";
		po.Weight = ParseWeight(PoWeightText);
		return po;
	}

	static PoNormLang ClonePoNormLang(PoNormLang? Src){
		Src ??= new PoNormLang{
			Type = ELangIdentType.Bcp47,
		};
		return new PoNormLang{
			DbCreatedAt = Src.DbCreatedAt,
			DbUpdatedAt = Src.DbUpdatedAt,
			DelAt = Src.DelAt,
			BizCreatedAt = Src.BizCreatedAt,
			BizUpdatedAt = Src.BizUpdatedAt,
			Id = Src.Id,
			Owner = Src.Owner,
			Type = Src.Type,
			Code = Src.Code,
			NativeName = Src.NativeName,
			EnglishName = Src.EnglishName,
			Weight = Src.Weight,
		};
	}

	static f64 ParseWeight(str? Text){
		var raw = Text?.Trim() ?? "";
		if(str.IsNullOrWhiteSpace(raw)){
			return 0;
		}
		if(f64.TryParse(raw, CultureInfo.InvariantCulture, out var value)){
			return value;
		}
		if(f64.TryParse(raw, CultureInfo.CurrentCulture, out value)){
			return value;
		}
		throw new FormatException($"{nameof(PoNormLang.Weight)} format invalid: {raw}");
	}

	static i32 ClampIndex(i32 Value, i32 Count){
		if(Count <= 0){
			return 0;
		}
		if(Value < 0){
			return 0;
		}
		if(Value >= Count){
			return Count - 1;
		}
		return Value;
	}

	i32 GetTypeIndex(ELangIdentType Type){
		if(TypeValues.Count == 0){
			return 0;
		}
		for(i32 i = 0; i < TypeValues.Count; i++){
			if(TypeValues[i] == Type){
				return i;
			}
		}
		return 0;
	}

	ELangIdentType GetTypeByIndex(i32 Index){
		if(TypeValues.Count == 0){
			return ELangIdentType.Bcp47;
		}
		var i = ClampIndex(Index, TypeValues.Count);
		return TypeValues[i];
	}
}

