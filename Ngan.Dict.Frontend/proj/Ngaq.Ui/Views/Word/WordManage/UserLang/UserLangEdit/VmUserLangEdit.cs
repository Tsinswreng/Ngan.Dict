namespace Ngaq.Ui.Views.Word.WordManage.UserLang.UserLangEdit;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.Dictionary.Models.Po.NormLang;
using Ngaq.Core.Shared.Word.Models.Po.UserLang;
using Ngaq.Core.Shared.Word.Svc;
using Ngaq.Ui.Infra;
using Tsinswreng.CsTools;
using Ctx = VmUserLangEdit;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

/// UserLang 編輯頁 ViewModel。
/// 支持新增與修改（不顯示 Owner，Id 只讀）。
public partial class VmUserLangEdit: ViewModelBase, IMk<Ctx>{
	/// 無參構造器供 `IMk<T>` 和設計期使用。
	protected VmUserLangEdit(){}

	/// 建立 Vm 實例。
	public static Ctx Mk(){
		return new Ctx();
	}

	/// 設計期示例數據。
	public static ObservableCollection<Ctx> Samples = [];
	static VmUserLangEdit(){
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
	public VmUserLangEdit(
		ISvcUserLang? SvcUserLang
		,IFrontendUserCtxMgr? UserCtxMgr
	){
		this.SvcUserLang = SvcUserLang;
		this.UserCtxMgr = UserCtxMgr;
	}

	/// 當前頁面是否爲新增模式。`true=新增`，`false=編輯既有實體`。
	public bool IsCreateMode{
		get;
		set{SetProperty(ref field, value);}
	} = true;

	/// 最近一次錯誤文本。
	public str LastError{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	/// 是否存在可展示錯誤。
	public bool HasError => !str.IsNullOrWhiteSpace(LastError);

	/// 可選語言標識類型。
	public IReadOnlyList<ELangIdentType> RelLangTypeValues{get;} = Enum
		.GetValues<ELangIdentType>()
		.Where(x=>x != ELangIdentType.Unknown)
		.ToList();

	/// 語言標識類型文本選項。
	public IReadOnlyList<str> RelLangTypeOptions{get;} = Enum
		.GetValues<ELangIdentType>()
		.Where(x=>x != ELangIdentType.Unknown)
		.Select(x=>x.ToString())
		.ToList();

	/// 當存在多種類型時才顯示下拉。
	public bool ShowRelLangTypeField => true;

	/// Id 只讀文本。
	public str PoIdText{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	/// UniqName 輸入。
	public str PoUniqName{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	/// 描述輸入。
	public str PoDescr{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	/// RelLang（如 BCP47 字符串）輸入。
	public str PoRelLang{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	/// 語言標識類型下拉索引。
	public i32 PoRelLangTypeIndex{
		get;
		set{SetProperty(ref field, value);}
	} = 0;

	/// 當前編輯中的 Po 快照。
	public PoUserLang PoUserLang{
		get;
		set{SetProperty(ref field, value);}
	} = new PoUserLang{
		RelLangType = ELangIdentType.Bcp47,
	};

	/// 顯式設置當前編輯模式，供列表頁跳轉時調用。
	public nil SetCreateMode(bool IsCreate){
		IsCreateMode = IsCreate;
		return NIL;
	}

	/// 由列表頁傳入 Po 實體初始化編輯頁。
	public nil FromPoUserLang(PoUserLang? PoUserLang){
		this.PoUserLang = ClonePoUserLang(PoUserLang);
		IsCreateMode = PoUserLang is null || PoUserLang.Id == IdUserLang.Zero;
		SyncFromPo();
		return NIL;
	}

	/// 套用標準語言選擇結果到關聯語言字段。
	public nil ApplyRelLangSelection(PoNormLang Po){
		PoRelLangTypeIndex = GetRelLangTypeIndex(Po.Type);
		PoRelLang = Po.Code ?? "";
		return NIL;
	}

	/// 保存到後端。新增走 `BatAddUserLang`，編輯走 `BatUpdUserLang`。
	public async Task<nil> Save(CT Ct = default){
		if(AnyNull(SvcUserLang, UserCtxMgr)){
			return NIL;
		}
		try{
			var po = BuildPoFromFields();
			var dbCtx = UserCtxMgr.GetDbUserCtx();
			if(IsCreateMode){
				// 新增時必須補齊 Owner，否則後端 `CheckOwner` 會拒絕。
				po.Owner = dbCtx.UserCtx.UserId;
				await SvcUserLang.OrdAddUserLang(dbCtx, ToolAsyE.ToAsyE([po]), Ct);
			}else{
				await SvcUserLang.OrdUpdUserLang(dbCtx, ToolAsyE.ToAsyE([po]), Ct);
			}

			PoUserLang = po;
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

	/// 以當前 Po 快照刷新 UI 字段。
	void SyncFromPo(){
		var po = PoUserLang ?? new PoUserLang{
			RelLangType = ELangIdentType.Bcp47,
		};
		PoIdText = po.Id.ToString();
		PoUniqName = po.UniqName ?? "";
		PoDescr = po.Descr ?? "";
		PoRelLang = po.RelLang ?? "";
		PoRelLangTypeIndex = GetRelLangTypeIndex(po.RelLangType);
		LastError = "";
		OnPropertyChanged(nameof(HasError));
	}

	/// 從 UI 字段構建待保存 Po。
	PoUserLang BuildPoFromFields(){
		var po = ClonePoUserLang(PoUserLang);
		po.UniqName = str.IsNullOrWhiteSpace(PoUniqName) ? null : PoUniqName.Trim();
		po.Descr = PoDescr?.Trim() ?? "";
		po.RelLangType = GetRelLangTypeByIndex(PoRelLangTypeIndex);
		po.RelLang = PoRelLang?.Trim() ?? "";
		return po;
	}

	/// 拷貝 Po，避免直接改寫來源實例。
	static PoUserLang ClonePoUserLang(PoUserLang? Src){
		Src ??= new PoUserLang{
			RelLangType = ELangIdentType.Bcp47,
		};
		return new PoUserLang{
			DbCreatedAt = Src.DbCreatedAt,
			DbUpdatedAt = Src.DbUpdatedAt,
			DelAt = Src.DelAt,
			BizCreatedAt = Src.BizCreatedAt,
			BizUpdatedAt = Src.BizUpdatedAt,
			Id = Src.Id,
			Owner = Src.Owner,
			UniqName = Src.UniqName,
			Descr = Src.Descr,
			RelLangType = Src.RelLangType,
			RelLang = Src.RelLang,
		};
	}

	/// 保障索引落在合法區間。
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

	/// 將枚舉值映射到下拉索引。
	i32 GetRelLangTypeIndex(ELangIdentType Type){
		if(RelLangTypeValues.Count == 0){
			return 0;
		}
		for(i32 i = 0; i < RelLangTypeValues.Count; i++){
			if(RelLangTypeValues[i] == Type){
				return i;
			}
		}
		return 0;
	}

	/// 根據下拉索引取回枚舉值。
	ELangIdentType GetRelLangTypeByIndex(i32 Index){
		if(RelLangTypeValues.Count == 0){
			return ELangIdentType.Bcp47;
		}
		var i = ClampIndex(Index, RelLangTypeValues.Count);
		return RelLangTypeValues[i];
	}
}

