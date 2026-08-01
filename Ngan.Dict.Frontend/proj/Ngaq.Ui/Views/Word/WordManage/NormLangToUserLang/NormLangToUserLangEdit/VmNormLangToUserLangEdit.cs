namespace Ngaq.Ui.Views.Word.WordManage.NormLangToUserLang.NormLangToUserLangEdit;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.Dictionary.Models.Po.NormLang;
using Ngaq.Core.Shared.Word.Models.Po.NormLangToUserLang;
using Ngaq.Core.Shared.Word.Models.Po.UserLang;
using Ngaq.Core.Shared.Word.Svc;
using Ngaq.Ui.Infra;
using Tsinswreng.CsTools;
using Ctx = VmNormLangToUserLangEdit;using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

/// 標準語言到用戶語言映射編輯頁 ViewModel。
public partial class VmNormLangToUserLangEdit: ViewModelBase, IMk<Ctx>{
	protected VmNormLangToUserLangEdit(){}

	public static Ctx Mk(){
		return new Ctx();
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmNormLangToUserLangEdit(){
		#if DEBUG
		{
			var o = new Ctx();
			Samples.Add(o);
		}
		#endif
	}

	ISvcNormLangToUserLang? SvcNormLangToUserLang{get;set;}
	IFrontendUserCtxMgr? UserCtxMgr{get;set;}

	public VmNormLangToUserLangEdit(
		ISvcNormLangToUserLang? SvcNormLangToUserLang
		,IFrontendUserCtxMgr? UserCtxMgr
	){
		this.SvcNormLangToUserLang = SvcNormLangToUserLang;
		this.UserCtxMgr = UserCtxMgr;
	}

	public bool IsCreateMode{
		get;
		set{SetProperty(ref field, value);}
	} = true;

	public IReadOnlyList<ELangIdentType> NormLangTypeValues{get;} = Enum
		.GetValues<ELangIdentType>()
		.Where(x=>x != ELangIdentType.Unknown)
		.ToList();

	public IReadOnlyList<str> NormLangTypeOptions{get;} = Enum
		.GetValues<ELangIdentType>()
		.Where(x=>x != ELangIdentType.Unknown)
		.Select(x=>x.ToString())
		.ToList();

	public bool ShowNormLangTypeField => true;

	public str PoIdText{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public str PoNormLang{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public str PoUserLang{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public str PoDescr{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public i32 PoNormLangTypeIndex{
		get;
		set{SetProperty(ref field, value);}
	} = 0;

	public PoNormLangToUserLang PoNormLangToUserLang{
		get;
		set{SetProperty(ref field, value);}
	} = new PoNormLangToUserLang{
		NormLangType = ELangIdentType.Bcp47,
	};

	public nil SetCreateMode(bool IsCreate){
		IsCreateMode = IsCreate;
		return NIL;
	}

	public nil FromPoNormLangToUserLang(PoNormLangToUserLang? Po){
		PoNormLangToUserLang = ClonePoNormLangToUserLang(Po);
		IsCreateMode = Po is null;
		SyncFromPo();
		return NIL;
	}

	public nil ApplyNormLangSelection(PoNormLang Po){
		PoNormLangTypeIndex = GetNormLangTypeIndex(Po.Type);
		PoNormLang = Po.Code ?? "";
		return NIL;
	}

	public nil ApplyUserLangSelection(PoUserLang Po){
		PoUserLang = Po.UniqName ?? "";
		return NIL;
	}

	public async Task<nil> Save(CT Ct = default){
		if(AnyNull(SvcNormLangToUserLang, UserCtxMgr)){
			return NIL;
		}
		try{
			var po = BuildPoFromFields();
			var dbCtx = UserCtxMgr.GetDbUserCtx();
			if(IsCreateMode){
				po.Owner = dbCtx.UserCtx.UserId;
				await SvcNormLangToUserLang.BatAddNormLangToUserLang(dbCtx, ToolAsyE.ToAsyE([po]), Ct);
			}else{
				await SvcNormLangToUserLang.BatUpdNormLangToUserLang(dbCtx, ToolAsyE.ToAsyE([po]), Ct);
			}

			PoNormLangToUserLang = po;
			IsCreateMode = false;
			SyncFromPo();
			ShowToast(I18n[K.Saved]);
		}catch(Exception e){
			HandleErr(e);
		}
		return NIL;
	}

	public async Task<nil> Delete(CT Ct = default){
		if(AnyNull(SvcNormLangToUserLang, UserCtxMgr)){
			return NIL;
		}
		try{
			var po = BuildPoFromFields();
			if(!IsCreateMode){
				await SvcNormLangToUserLang.BatSoftDelNormLangToUserLang(UserCtxMgr.GetDbUserCtx(), ToolAsyE.ToAsyE([po]), Ct);
			}
			PoNormLangToUserLang = new PoNormLangToUserLang{
				NormLangType = ELangIdentType.Bcp47,
			};
			IsCreateMode = true;
			SyncFromPo();
			ShowToast(I18n[K.Deleted]);
		}catch(Exception e){
			HandleErr(e);
		}
		return NIL;
	}

	void SyncFromPo(){
		var po = PoNormLangToUserLang ?? new PoNormLangToUserLang{
			NormLangType = ELangIdentType.Bcp47,
		};
		PoIdText = po.Id.ToString();
		PoNormLang = po.NormLang ?? "";
		PoUserLang = po.UserLang ?? "";
		PoDescr = po.Descr ?? "";
		PoNormLangTypeIndex = GetNormLangTypeIndex(po.NormLangType);
	}

	PoNormLangToUserLang BuildPoFromFields(){
		var po = ClonePoNormLangToUserLang(PoNormLangToUserLang);
		po.NormLangType = GetNormLangTypeByIndex(PoNormLangTypeIndex);
		po.NormLang = PoNormLang?.Trim() ?? "";
		po.UserLang = PoUserLang?.Trim() ?? "";
		po.Descr = PoDescr?.Trim() ?? "";
		return po;
	}

	static PoNormLangToUserLang ClonePoNormLangToUserLang(PoNormLangToUserLang? Src){
		Src ??= new PoNormLangToUserLang{
			NormLangType = ELangIdentType.Bcp47,
		};
		return new PoNormLangToUserLang{
			DbCreatedAt = Src.DbCreatedAt,
			DbUpdatedAt = Src.DbUpdatedAt,
			DelAt = Src.DelAt,
			BizCreatedAt = Src.BizCreatedAt,
			BizUpdatedAt = Src.BizUpdatedAt,
			Id = Src.Id,
			Owner = Src.Owner,
			NormLangType = Src.NormLangType,
			NormLang = Src.NormLang,
			Descr = Src.Descr,
			UserLang = Src.UserLang,
		};
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

	i32 GetNormLangTypeIndex(ELangIdentType Type){
		if(NormLangTypeValues.Count == 0){
			return 0;
		}
		for(i32 i = 0; i < NormLangTypeValues.Count; i++){
			if(NormLangTypeValues[i] == Type){
				return i;
			}
		}
		return 0;
	}

	ELangIdentType GetNormLangTypeByIndex(i32 Index){
		if(NormLangTypeValues.Count == 0){
			return ELangIdentType.Bcp47;
		}
		var i = ClampIndex(Index, NormLangTypeValues.Count);
		return NormLangTypeValues[i];
	}
}

