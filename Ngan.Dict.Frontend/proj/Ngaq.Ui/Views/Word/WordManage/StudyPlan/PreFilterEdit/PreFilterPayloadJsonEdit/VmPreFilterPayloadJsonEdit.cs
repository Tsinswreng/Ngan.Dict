namespace Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.PreFilterPayloadJsonEdit;

using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Shared.StudyPlan.Models.Po.PreFilter;
using Ngaq.Core.Shared.StudyPlan.Svc;
using Ngaq.Ui.Infra;
using Tsinswreng.CsTools;

using Ctx = VmPreFilterPayloadJsonEdit;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;
using JsonNode = System.Text.Json.Nodes.JsonNode;
using JsonObject = System.Text.Json.Nodes.JsonObject;
using JsonValue = System.Text.Json.Nodes.JsonValue;

/// <summary>
/// PreFilter 載荷 JSON 子頁 ViewModel。
/// 只編輯 <see cref="PoPreFilter.Text"/>，保存/刪除直接調後端。
/// </summary>
public class VmPreFilterPayloadJsonEdit: ViewModelBase, IMk<Ctx>{
	protected VmPreFilterPayloadJsonEdit(){}

	public static Ctx Mk(){
		return new Ctx();
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmPreFilterPayloadJsonEdit(){
		#if DEBUG
		{
			var o = new Ctx();
			Samples.Add(o);
		}
		#endif
	}

	ISvcStudyPlan? SvcStudyPlan{get;set;}
	IFrontendUserCtxMgr? UserCtxMgr{get;set;}

	/// <summary>
	/// 當保存或刪除完成後，把最新實體回寫給父頁；刪除時回傳 null。
	/// </summary>
	Action<PoPreFilter?>? OnSavedOrDeleted{get;set;}

	/// <summary>
	/// 當前正在編輯的 PreFilter 實體快照。
	/// </summary>
	PoPreFilter EditingPo{get;set;} = new PoPreFilter{
		Type = EPreFilterType.Json,
	};

	public VmPreFilterPayloadJsonEdit(
		ISvcStudyPlan? SvcStudyPlan
		,IFrontendUserCtxMgr? UserCtxMgr
	){
		this.SvcStudyPlan = SvcStudyPlan;
		this.UserCtxMgr = UserCtxMgr;
	}

	public str LastError{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public bool HasError => !str.IsNullOrWhiteSpace(LastError);

	/// <summary>
	/// 供文本編輯器雙向同步的載荷 JSON 文本。
	/// </summary>
	public str PayloadJson{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	/// <summary>
	/// 由父頁傳入當前實體與回寫回調。
	/// </summary>
	public nil Load(PoPreFilter? PoPreFilter, Action<PoPreFilter?>? OnSavedOrDeleted){
		EditingPo = ClonePoPreFilter(PoPreFilter);
		this.OnSavedOrDeleted = OnSavedOrDeleted;
		PayloadJson = FormatJson(EditingPo.Text ?? "");
		LastError = "";
		OnPropertyChanged(nameof(HasError));
		return NIL;
	}

	/// <summary>
	/// 驗證載荷 JSON 並保存到後端。
	/// </summary>
	public async Task<nil> Save(CT Ct = default){
		if(AnyNull(SvcStudyPlan, UserCtxMgr)){
			return NIL;
		}
		if(!TryBuildPo(out var po, out var err)){
			LastError = err;
			OnPropertyChanged(nameof(HasError));
			ShowDialog(err);
			return NIL;
		}

		try{
			var dbCtx = UserCtxMgr.GetDbUserCtx();
			if(po.Id == IdPreFilter.Zero){
				await SvcStudyPlan.OrdAddPreFilter(dbCtx, ToolAsyE.ToAsyE([po]), Ct);
			}else{
				await SvcStudyPlan.OrdUpdPreFilter(dbCtx, ToolAsyE.ToAsyE([po]), Ct);
			}
			EditingPo = po;
			PayloadJson = FormatJson(po.Text ?? "");
			LastError = "";
			OnPropertyChanged(nameof(HasError));
			OnSavedOrDeleted?.Invoke(po);
			ShowToast(I18n[K.Saved]);
		}catch(Exception e){
			HandleErr(e);
		}
		return NIL;
	}

	/// <summary>
	/// 軟刪除當前 PreFilter；新增模式下只提示不可刪。
	/// </summary>
	public async Task<nil> Delete(CT Ct = default){
		if(AnyNull(SvcStudyPlan, UserCtxMgr)){
			return NIL;
		}
		if(EditingPo.Id == IdPreFilter.Zero){
			ShowDialog(I18n[K.NoPersistedIdToDelete]);
			return NIL;
		}

		try{
			await SvcStudyPlan.OrdSoftDelPreFilter(UserCtxMgr.GetDbUserCtx(), ToolAsyE.ToAsyE([EditingPo]), Ct);
			EditingPo = new PoPreFilter{
				Type = EPreFilterType.Json,
			};
			PayloadJson = "";
			LastError = "";
			OnPropertyChanged(nameof(HasError));
			OnSavedOrDeleted?.Invoke(null);
			ShowToast(I18n[K.Deleted]);
			ViewNavi?.Back();
		}catch(Exception e){
			HandleErr(e);
		}
		return NIL;
	}

	/// <summary>
	/// 解析載荷 JSON，生成待保存的實體。
	/// 不再依賴強類型反序列化，避免 AOT/元數據缺失導致的保存失敗。
	/// </summary>
	bool TryBuildPo(out PoPreFilter Po, out str Err){
		Po = ClonePoPreFilter(EditingPo);
		Err = "";
		try{
			var node = JsonNode.Parse(PayloadJson);
			if(node is null){
				Err = I18n[K.JsonParseFailed];
				return false;
			}
			Po.Text = node.ToJsonString(new JsonSerializerOptions{
				WriteIndented = true,
				Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
			});
			Po.DataSchemaVer = ReadVersion(node) ?? Po.DataSchemaVer ?? new Version(1, 0, 0, 0);
			Po.Type = EPreFilterType.Json;
			Po.Binary = null;
			return true;
		}catch(Exception e){
			Err = e.Message;
			return false;
		}
	}

	/// <summary>
	/// 從 JSON 根節點讀取 Version 字段；缺失或非法時返回 null。
	/// </summary>
	static Version? ReadVersion(JsonNode Node){
		if(Node is not JsonObject obj){
			return null;
		}
		if(!obj.TryGetPropertyValue(nameof(Ngaq.Core.Shared.StudyPlan.Models.PreFilter.PreFilter.Version), out var versionNode) || versionNode is null){
			return null;
		}
		if(versionNode is JsonValue val){
			if(val.TryGetValue<str>(out var verText) && Version.TryParse(verText, out var parsed)){
				return parsed;
			}
		}
		return null;
	}

	/// <summary>
	/// 格式化 JSON 供編輯器展示；失敗時返回原文。
	/// </summary>
	static str FormatJson(str UglyJson){
		if(str.IsNullOrWhiteSpace(UglyJson)){
			return "";
		}
		try{
			var node = JsonNode.Parse(UglyJson);
			return node?.ToJsonString(new JsonSerializerOptions{
				WriteIndented = true,
				Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
			}) ?? UglyJson;
		}catch{
			return UglyJson;
		}
	}

	/// <summary>
	/// 複製 Po，避免子頁直接修改父頁持有的引用。
	/// </summary>
	static PoPreFilter ClonePoPreFilter(PoPreFilter? Src){
		Src ??= new PoPreFilter{
			Type = EPreFilterType.Json,
		};
		return new PoPreFilter{
			DbCreatedAt = Src.DbCreatedAt,
			DbUpdatedAt = Src.DbUpdatedAt,
			DelAt = Src.DelAt,
			BizCreatedAt = Src.BizCreatedAt,
			BizUpdatedAt = Src.BizUpdatedAt,
			Id = Src.Id,
			Owner = Src.Owner,
			UniqName = Src.UniqName,
			Descr = Src.Descr,
			Type = Src.Type,
			DataSchemaVer = Src.DataSchemaVer,
			Binary = Src.Binary?.ToArray() ?? [],
			Text = Src.Text,
		};
	}
}
