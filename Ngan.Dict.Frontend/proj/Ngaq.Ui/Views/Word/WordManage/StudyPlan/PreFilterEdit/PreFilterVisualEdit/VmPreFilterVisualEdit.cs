namespace Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.PreFilterVisualEdit;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Shared.StudyPlan.Models;
using Ngaq.Core.Shared.StudyPlan.Models.Po.PreFilter;
using Ngaq.Core.Shared.StudyPlan.Models.PreFilter;
using Ngaq.Core.Shared.StudyPlan.Svc;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Tools.Json;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.I18n;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.FieldsFilterCardEdit;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.PreFilterPayloadJsonEdit;
using Tsinswreng.CsTools;

using Ctx = VmPreFilterVisualEdit;using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;
using Ngaq.Core.Infra.IF;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.PreFilterPayloadEdit;

/// <summary>
/// PreFilter visual editor VM.
/// </summary>
public class VmPreFilterVisualEdit: ViewModelBase, IMk<Ctx>{
	protected VmPreFilterVisualEdit(){
		InitRowEvents();
	}

	public static Ctx Mk(){
		return new Ctx();
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmPreFilterVisualEdit(){
		#if DEBUG
		{
			var o = new Ctx();
			Samples.Add(o);
		}
		#endif
	}

	ISvcStudyPlan? SvcStudyPlan{get;set;}
	IFrontendUserCtxMgr? UserCtxMgr{get;set;}
	IJsonSerializer JsonSerializer{get;set;} = AppJsonSerializer.Inst;
	bool _isHydrating = false;

	/// <summary>
	/// Allowed PoWord fields for CoreFilter.
	/// </summary>
	public static readonly IReadOnlyList<str> CoreWordFieldOptions = [
		nameof(PoWord.Head),
		nameof(PoWord.Lang),
		nameof(PoWord.BizCreatedAt),
		nameof(PoWord.BizUpdatedAt),
	];

	static readonly HashSet<str> CoreWordFieldSet = CoreWordFieldOptions.ToHashSet();

	/// <summary>
	/// Builtin prop keys from <see cref="KeysProp"/> for PropFilter suggestions.
	/// </summary>
	public IReadOnlyList<str> PropFieldOptions{get;} = BuildPropFieldOptions();

	public VmPreFilterVisualEdit(
		ISvcStudyPlan? SvcStudyPlan
		,IFrontendUserCtxMgr? UserCtxMgr
		,IJsonSerializer? JsonSerializer
	){
		this.SvcStudyPlan = SvcStudyPlan;
		this.UserCtxMgr = UserCtxMgr;
		this.JsonSerializer = JsonSerializer ?? AppJsonSerializer.Inst;
		InitRowEvents();
		BoPreFilter = MkEmptyBoPreFilter();
		SyncFromBo();
	}

	public class VmFilterItemRow: ViewModelBase{
		public i32 OperationIndex{
			get;
			set{SetProperty(ref field, value);}
		} = (i32)EFilterOperationMode.Eq;

		public i32 ValueTypeIndex{
			get;
			set{SetProperty(ref field, value);}
		} = (i32)EValueType.String;

		public str ValuesText{
			get;
			set{SetProperty(ref field, value);}
		} = "";
	}

	public class VmFieldValueRow: ViewModelBase{
		public str Value{
			get;
			set{SetProperty(ref field, value);}
		} = "";
	}

	public class VmFieldsFilterRow: ViewModelBase{
		public ObservableCollection<VmFieldValueRow> Fields{get;set;} = [];
		public ObservableCollection<VmFilterItemRow> Items{get;set;} = [];
	}

	public class RowFieldsFilterCard{
		public u64 UiIdx{get;set;}
		public str UiIdxText{get;set;} = "";
		public str Kind{get;set;} = "";
		public str FieldsPreview{get;set;} = "";
		public str ContentPreview{get;set;} = "";
		public str FilterCountText{get;set;} = "";
		public VmFieldsFilterRow? Raw{get;set;}
	}

	public BoPreFilter BoPreFilter{
		get;
		set{SetProperty(ref field, value);}
	} = MkEmptyBoPreFilter();

	public str PoTextPreview{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public str LastError{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public bool HasError => !str.IsNullOrWhiteSpace(LastError);

	public bool IsCreateMode{
		get;
		set{SetProperty(ref field, value);}
	} = true;

	public ObservableCollection<VmFieldsFilterRow> CoreFilterRows{get;set;} = [];
	public ObservableCollection<VmFieldsFilterRow> PropFilterRows{get;set;} = [];

	public ObservableCollection<RowFieldsFilterCard> CoreFilterCards{get;set;} = [];
	public ObservableCollection<RowFieldsFilterCard> PropFilterCards{get;set;} = [];

	public IReadOnlyList<EPreFilterType> PoTypeValues{get;} = Enum
		.GetValues<EPreFilterType>()
		.Where(x=>x != EPreFilterType.Unknown)
		.ToList();
	public IReadOnlyList<str> PoTypeOptions{get;} = Enum
		.GetValues<EPreFilterType>()
		.Where(x=>x != EPreFilterType.Unknown)
		.Select(x=>x.ToString())
		.ToList();
	public IReadOnlyList<str> OperationOptions{get;} = Enum.GetNames<EFilterOperationMode>();
	public IReadOnlyList<str> ValueTypeOptions{get;} = Enum.GetNames<EValueType>();
	public bool ShowPoTypeField => PoTypeOptions.Count > 1;

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
	} = 0;

	public str PreFilterVersion{
		get;
		set{SetProperty(ref field, value);}
	} = "1.0.0.0";

	void InitRowEvents(){
		CoreFilterRows.CollectionChanged += OnFieldsRowsChanged;
		PropFilterRows.CollectionChanged += OnFieldsRowsChanged;
	}

	void OnFieldsRowsChanged(object? sender, NotifyCollectionChangedEventArgs e){
		if(e.NewItems is not null){
			foreach(var row in e.NewItems.OfType<VmFieldsFilterRow>()){
				row.Fields.CollectionChanged += OnFieldValuesChanged;
				row.Items.CollectionChanged += OnFilterItemsChanged;
				foreach(var field in row.Fields){
					field.PropertyChanged += OnFieldValueChanged;
				}
				foreach(var item in row.Items){
					item.PropertyChanged += OnFilterItemChanged;
				}
			}
		}
		if(e.OldItems is not null){
			foreach(var row in e.OldItems.OfType<VmFieldsFilterRow>()){
				row.Fields.CollectionChanged -= OnFieldValuesChanged;
				row.Items.CollectionChanged -= OnFilterItemsChanged;
				foreach(var field in row.Fields){
					field.PropertyChanged -= OnFieldValueChanged;
				}
				foreach(var item in row.Items){
					item.PropertyChanged -= OnFilterItemChanged;
				}
			}
		}
		TouchRows();
	}

	void OnFieldValuesChanged(object? sender, NotifyCollectionChangedEventArgs e){
		if(e.NewItems is not null){
			foreach(var field in e.NewItems.OfType<VmFieldValueRow>()){
				field.PropertyChanged += OnFieldValueChanged;
			}
		}
		if(e.OldItems is not null){
			foreach(var field in e.OldItems.OfType<VmFieldValueRow>()){
				field.PropertyChanged -= OnFieldValueChanged;
			}
		}
		TouchRows();
	}

	void OnFieldValueChanged(object? sender, PropertyChangedEventArgs e){
		TouchRows();
	}

	void OnFilterItemsChanged(object? sender, NotifyCollectionChangedEventArgs e){
		if(e.NewItems is not null){
			foreach(var item in e.NewItems.OfType<VmFilterItemRow>()){
				item.PropertyChanged += OnFilterItemChanged;
			}
		}
		if(e.OldItems is not null){
			foreach(var item in e.OldItems.OfType<VmFilterItemRow>()){
				item.PropertyChanged -= OnFilterItemChanged;
			}
		}
		TouchRows();
	}

	void OnFilterItemChanged(object? sender, PropertyChangedEventArgs e){
		TouchRows();
	}

	void TouchRows(){
		if(_isHydrating){
			return;
		}
		RefreshFieldsFilterCards();
	}

	public nil RefreshFieldsFilterCards(){
		CoreFilterCards.Clear();
		for(u64 i = 0; i < (u64)CoreFilterRows.Count; i++){
			var row = CoreFilterRows[(i32)i];
			CoreFilterCards.Add(new RowFieldsFilterCard{
				UiIdx = i + 1,
				UiIdxText = (i + 1).ToString(),
				Kind = "Core",
				FieldsPreview = JoinFieldPreview(row),
				ContentPreview = BuildContentPreview(row),
				FilterCountText = (row.Items?.Count ?? 0).ToString(),
				Raw = row,
			});
		}
		PropFilterCards.Clear();
		for(u64 i = 0; i < (u64)PropFilterRows.Count; i++){
			var row = PropFilterRows[(i32)i];
			PropFilterCards.Add(new RowFieldsFilterCard{
				UiIdx = i + 1,
				UiIdxText = (i + 1).ToString(),
				Kind = "Prop",
				FieldsPreview = JoinFieldPreview(row),
				ContentPreview = BuildContentPreview(row),
				FilterCountText = (row.Items?.Count ?? 0).ToString(),
				Raw = row,
			});
		}
		return NIL;
	}

	public nil FromPoPreFilter(PoPreFilter? PoPreFilter){
		var bo = MkEmptyBoPreFilter();
		IsCreateMode = PoPreFilter is null;
		if(PoPreFilter is not null){
			bo.FromPoPreFilter(PoPreFilter);
		}
		return FromBoPreFilter(bo);
	}

	public nil FromBoPreFilter(BoPreFilter? BoPreFilter){
		this.BoPreFilter = BoPreFilter ?? MkEmptyBoPreFilter();
		SyncFromBo();
		return NIL;
	}

	public nil SetCreateMode(bool IsCreate){
		this.IsCreateMode = IsCreate;
		return NIL;
	}
	/// <summary>
	/// 打開載荷 JSON 文本編輯頁；先把當前 GUI 草稿同步到 Po，再交給子頁編輯。
	/// </summary>
	public nil OpenPayloadJsonEditor(){
		if(!TryBuildBoFromVisual(out var bo, out var err)){
			LastError = err;
			OnPropertyChanged(nameof(HasError));
			ShowDialog(err);
			return NIL;
		}
		LastError = "";
		OnPropertyChanged(nameof(HasError));
		BoPreFilter = bo;

		var view = new ViewPreFilterPayloadJsonEdit();
		view.Ctx?.Load(bo.PoPreFilter, OnPayloadJsonSavedOrDeleted);
		ViewNavi?.GoTo(ToolView.WithTitle(I18n[K.TextPayload], view));
		return NIL;
	}

	/// <summary>
	/// 子頁保存後回寫父頁；刪除時重置為空白 PreFilter。
	/// </summary>
	public void OnPayloadJsonSavedOrDeleted(PoPreFilter? Po){
		if(Po is null){
			IsCreateMode = true;
			FromPoPreFilter(null);
			return;
		}
		IsCreateMode = Po.Id == IdPreFilter.Zero;
		FromPoPreFilter(Po);
		return;
	}

	public nil OpenPreFilterDataEditor(){
		var view = new ViewPreFilterPayloadEdit(this);
		ViewNavi?.GoTo(ToolView.WithTitle(I18n[K.PreFilter], view));
		return NIL;
	}

	public bool CommitPreFilterDataDraft(){
		if(!TryBuildBoFromVisual(out var bo, out var err)){
			LastError = err;
			OnPropertyChanged(nameof(HasError));
			ShowDialog(err);
			return false;
		}

		LastError = "";
		OnPropertyChanged(nameof(HasError));
		BoPreFilter = bo;
		RefreshTextPreview();
		ShowDialog(I18n[K.PreFilterDraftUpdated]);
		return true;
	}

	public async Task<nil> Save(CT Ct = default){
		if(AnyNull(SvcStudyPlan, UserCtxMgr)){
			return NIL;
		}
		if(!TryBuildBoFromVisual(out var bo, out var err)){
			LastError = err;
			OnPropertyChanged(nameof(HasError));
			ShowDialog(err);
			return NIL;
		}

		try{
			var dbCtx = UserCtxMgr.GetDbUserCtx();
			var po = bo.PoPreFilter;
			if(IsCreateMode){
				await SvcStudyPlan.OrdAddPreFilter(dbCtx, ToolAsyE.ToAsyE([po]), Ct);
			}else{
				await SvcStudyPlan.OrdUpdPreFilter(dbCtx, ToolAsyE.ToAsyE([po]), Ct);
			}

			IsCreateMode = false;
			BoPreFilter = bo;
			SyncFromBo();
			LastError = "";
			OnPropertyChanged(nameof(HasError));
			ShowToast(I18n[K.Saved]);
		}catch(Exception e){
			HandleErr(e);
		}
		return NIL;
	}

	public async Task<nil> Delete(CT Ct = default){
		if(AnyNull(SvcStudyPlan, UserCtxMgr)){
			return NIL;
		}
		try{
			var po = BoPreFilter?.PoPreFilter ?? new PoPreFilter();
			if(!IsCreateMode){
				await SvcStudyPlan.OrdSoftDelPreFilter(UserCtxMgr.GetDbUserCtx(), ToolAsyE.ToAsyE([po]), Ct);
			}
			BoPreFilter = MkEmptyBoPreFilter();
			IsCreateMode = true;
			SyncFromBo();
			LastError = "";
			OnPropertyChanged(nameof(HasError));
			ShowToast(I18n[K.Deleted]);
		}catch(Exception e){
			HandleErr(e);
		}
		return NIL;
	}

	public nil AddCoreGroup(){
		CoreFilterRows.Add(MkFieldsFilterRow());
		RefreshFieldsFilterCards();
		return NIL;
	}

	public nil AddPropGroup(){
		PropFilterRows.Add(MkFieldsFilterRow());
		RefreshFieldsFilterCards();
		return NIL;
	}

	public nil OpenCoreFilterCard(RowFieldsFilterCard? Card){
		return OpenFieldsFilterCard(Card, true);
	}

	public nil OpenPropFilterCard(RowFieldsFilterCard? Card){
		return OpenFieldsFilterCard(Card, false);
	}

	nil OpenFieldsFilterCard(RowFieldsFilterCard? Card, bool IsCore){
		if(Card?.Raw is null){
			return NIL;
		}
		var view = new ViewFieldsFilterCardEdit();
		view.Ctx?.Load(this, Card.Raw, IsCore, Card.UiIdx);
		var title = (IsCore ? I18n[K.CoreFilter] : I18n[K.PropFilter]) + " #" + Card.UiIdx;
		ViewNavi?.GoTo(ToolView.WithTitle(title, view));
		return NIL;
	}

	void SyncFromBo(){
		var po = BoPreFilter?.PoPreFilter ?? MkEmptyBoPreFilter().PoPreFilter;
		var pre = BoPreFilter?.PreFilter ?? MkEmptyBoPreFilter().PreFilter;

		_isHydrating = true;
		try{
			PoIdText = po.Id.ToString();
			PoUniqName = po.UniqName ?? "";
			PoDescr = po.Descr;
			PoTypeIndex = GetTypeIndex(po.Type);

			PreFilterVersion = (pre.Version ?? new Version(1, 0, 0, 0)).ToString();
			CoreFilterRows.Clear();
			foreach(var row in (pre.CoreFilter ?? []).Select(MkRowFromModel)){
				CoreFilterRows.Add(row);
			}
			PropFilterRows.Clear();
			foreach(var row in (pre.PropFilter ?? []).Select(MkRowFromModel)){
				PropFilterRows.Add(row);
			}
			RefreshFieldsFilterCards();
			RefreshTextPreview();

			LastError = "";
			OnPropertyChanged(nameof(HasError));
		}finally{
			_isHydrating = false;
		}
	}

	void RefreshTextPreview(){
		var fullText = BoPreFilter?.PoPreFilter?.Text ?? "";
		if(str.IsNullOrWhiteSpace(fullText)){
			PoTextPreview = "";
			return;
		}

		const int maxLen = 320;
		PoTextPreview = fullText.Length <= maxLen
			? fullText
			: fullText[..maxLen] + "...";
	}

	VmFieldsFilterRow MkRowFromModel(FieldsFilter row){
		var vm = new VmFieldsFilterRow();
		foreach(var field in row.Fields ?? []){
			vm.Fields.Add(new VmFieldValueRow{Value = field ?? ""});
		}
		foreach(var item in (row.Filters ?? [])){
			vm.Items.Add(new VmFilterItemRow{
				OperationIndex = ClampIndex((i32)item.Operation, OperationOptions.Count),
				ValueTypeIndex = ClampIndex((i32)item.ValueType, ValueTypeOptions.Count),
				ValuesText = string.Join(Environment.NewLine, (item.Values ?? []).Select(x => x?.ToString() ?? "")),
			});
		}
		return vm;
	}

	bool TryBuildBoFromVisual(
		out BoPreFilter Bo,
		out str Err
	){
		Bo = BoPreFilter ?? MkEmptyBoPreFilter();
		Err = "";
		try{
			var po = ClonePoPreFilter(Bo.PoPreFilter);
			po.UniqName = str.IsNullOrWhiteSpace(PoUniqName) ? null : PoUniqName.Trim();
			po.Descr = PoDescr?.Trim() ?? "";
			po.Type = GetTypeByIndex(PoTypeIndex);

			if(!Version.TryParse((PreFilterVersion ?? "").Trim(), out var ver)){
				ver = new Version(1, 0, 0, 0);
			}

			var pre = new Ngaq.Core.Shared.StudyPlan.Models.PreFilter.PreFilter{
				Version = ver,
				CoreFilter = BuildFieldsFilterList(CoreFilterRows, IsCore: true, out var coreErr),
				PropFilter = [],
			};
			if(coreErr is not null){
				Err = coreErr;
				return false;
			}
			pre.PropFilter = BuildFieldsFilterList(PropFilterRows, IsCore: false, out var propErr);
			if(propErr is not null){
				Err = propErr;
				return false;
			}

			po.DataSchemaVer = pre.Version;
			po.Text = JsonSerializer.Stringify(pre);
			po.Binary = null;

			Bo = new BoPreFilter{
				PoPreFilter = po,
				PreFilter = pre,
			};
			return true;
		}catch(Exception e){
			Err = e.Message;
			return false;
		}
	}

	static PoPreFilter ClonePoPreFilter(PoPreFilter? src){
		src ??= new PoPreFilter();
		return new PoPreFilter{
			DbCreatedAt = src.DbCreatedAt,
			DbUpdatedAt = src.DbUpdatedAt,
			DelAt = src.DelAt,
			BizCreatedAt = src.BizCreatedAt,
			BizUpdatedAt = src.BizUpdatedAt,
			Id = src.Id,
			Owner = src.Owner,
			UniqName = src.UniqName,
			Descr = src.Descr,
			Type = src.Type,
			DataSchemaVer = src.DataSchemaVer,
			Text = src.Text,
			Binary = src.Binary?.ToArray() ?? [],
		};
	}

	IList<FieldsFilter> BuildFieldsFilterList(
		IEnumerable<VmFieldsFilterRow> Rows,
		bool IsCore,
		out str? Err
	){
		Err = null;
		var ans = new List<FieldsFilter>();
		var rowIdx = 0;
		foreach(var row in Rows){
			rowIdx++;
			var fields = row.Fields
				.Select(x=>x.Value?.Trim() ?? "")
				.Where(x=>!str.IsNullOrWhiteSpace(x))
				.Distinct()
				.ToList();
			if(IsCore){
				var invalid = fields.Where(x=>!CoreWordFieldSet.Contains(x)).ToList();
				if(invalid.Count > 0){
					Err = I18n.Get(K.Row__InvalidCoreFilterField__, rowIdx, invalid[0]);
					return [];
				}
			}

			var filters = new List<FilterItem>();
			var itemIdx = 0;
			foreach(var item in row.Items){
				itemIdx++;
				var valueType = EnumOrDefault<EValueType>(item.ValueTypeIndex);
				var values = ParseValues(item.ValuesText, valueType, out var parseErr);
				if(parseErr is not null){
					Err = $"Row#{rowIdx}, Item#{itemIdx}: {parseErr}";
					return [];
				}
				filters.Add(new FilterItem{
					Operation = EnumOrDefault<EFilterOperationMode>(item.OperationIndex),
					ValueType = valueType,
					Values = values,
				});
			}
			if(fields.Count == 0 && filters.Count == 0){
				continue;
			}
			ans.Add(new FieldsFilter{
				Fields = fields,
				Filters = filters,
			});
		}
		return ans;
	}

	IList<obj?> ParseValues(str Text, EValueType ValueType, out str? Err){
		Err = null;
		var parts = SplitLines(Text).ToList();
		if(parts.Count == 0){
			return [];
		}
		if(ValueType == EValueType.String){
			return parts.Cast<obj?>().ToList();
		}
		if(ValueType == EValueType.Null){
			return parts.Select(_ => (obj?)null).ToList();
		}
		if(ValueType == EValueType.Number){
			var ans = new List<obj?>();
			foreach(var p in parts){
				if(long.TryParse(p, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i)){
					ans.Add(i);
					continue;
				}
				if(double.TryParse(p, NumberStyles.Float, CultureInfo.InvariantCulture, out var d)){
					ans.Add(d);
					continue;
				}
				Err = I18n.Get(K.__IsNotValidNumber, p);
				return [];
			}
			return ans;
		}
		return parts.Cast<obj?>().ToList();
	}

	static IEnumerable<str> SplitLines(str? Text){
		if(str.IsNullOrWhiteSpace(Text)){
			return [];
		}
		return Text
			.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Where(x=>!str.IsNullOrWhiteSpace(x));
	}

	static TEnum EnumOrDefault<TEnum>(i32 Index)
		where TEnum : struct, Enum
	{
		var values = Enum.GetValues<TEnum>();
		if(Index < 0 || Index >= values.Length){
			return values[0];
		}
		return values[Index];
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

	VmFieldsFilterRow MkFieldsFilterRow(){
		return new VmFieldsFilterRow();
	}

	public static VmFilterItemRow MkFilterItemRow(){
		return new VmFilterItemRow{
			OperationIndex = (i32)EFilterOperationMode.Eq,
			ValueTypeIndex = (i32)EValueType.String,
		};
	}

	static BoPreFilter MkEmptyBoPreFilter(){
		return new BoPreFilter{
			PoPreFilter = new PoPreFilter{
				Type = EPreFilterType.Json,
			},
			PreFilter = new Ngaq.Core.Shared.StudyPlan.Models.PreFilter.PreFilter{
				Version = new Version(1, 0, 0, 0),
				CoreFilter = [],
				PropFilter = [],
			},
		};
	}

	static IReadOnlyList<str> BuildPropFieldOptions(){
		var ans = new List<str>();
		var keys = KeysProp.Inst;
		var t = typeof(KeysProp);

		foreach(var p in t.GetProperties().Where(x=>x.PropertyType == typeof(EItemProp))){
			if(p.GetValue(keys) is EItemProp key){
				ans.Add(key);
			}
		}
		foreach(var f in t.GetFields().Where(x=>x.FieldType == typeof(EItemProp))){
			if(f.GetValue(keys) is EItemProp key){
				ans.Add(key);
			}
		}
		return ans
			.Where(x=>!str.IsNullOrWhiteSpace(x))
			.Distinct()
			.OrderBy(x=>x)
			.ToList();
	}

str JoinFieldPreview(VmFieldsFilterRow row){
	var fields = row.Fields
		.Select(x=>x.Value?.Trim() ?? "")
		.Where(x=>!str.IsNullOrWhiteSpace(x))
		.Select(LocalizeFieldName)
		.ToList();
	if(fields.Count == 0){
		return "-";
	}
	return string.Join(", ", fields);
}

str BuildContentPreview(VmFieldsFilterRow row){
	if(row.Fields.Count != 1 || row.Items.Count != 1){
		return "-";
	}

	var field = row.Fields[0].Value?.Trim() ?? "";
	if(str.IsNullOrWhiteSpace(field)){
		return "-";
	}

	var item = row.Items[0];
	var values = item.ValuesText
		.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
		.Select(x=>x.Trim())
		.Where(x=>!str.IsNullOrWhiteSpace(x))
		.ToArray();
	if(values.Length != 1){
		return "-";
	}

	return $"{LocalizeFieldName(field)} {ToOpText(item.OperationIndex)} {values[0]}";
}

str ToOpText(i32 operationIndex){
	var values = Enum.GetValues<EFilterOperationMode>();
	if(operationIndex < 0 || operationIndex >= values.Length){
		return "=";
	}
	return values[operationIndex] switch{
		EFilterOperationMode.IncludeAny => I18n[K.IncludeAny],
		EFilterOperationMode.IncludeAll => I18n[K.IncludeAll],
		EFilterOperationMode.ExcludeAll => I18n[K.ExcludeAll],
		EFilterOperationMode.Eq => "=",
		EFilterOperationMode.Ne => "!=",
		EFilterOperationMode.Gt => ">",
		EFilterOperationMode.Ge => ">=",
		EFilterOperationMode.Lt => "<",
		EFilterOperationMode.Le => "<=",
		_ => "=",
	};
}

/// <summary>
/// 將字段原始鍵映射成界面顯示文案，便于預覽使用。
/// </summary>
str LocalizeFieldName(str raw){
	if(str.IsNullOrWhiteSpace(raw)){
		return raw;
	}
	return raw switch{
		nameof(PoWord.Head) => I18n[K.Head],
		nameof(PoWord.Lang) => I18n[K.Lang],
		nameof(PoWord.BizCreatedAt) => I18n[K.Biz_CreatedAt],
		nameof(PoWord.BizUpdatedAt) => I18n[K.Biz_UpdatedAt],
		"summary" => I18n[K.Summary],
		"description" => I18n[K.Description],
		"note" => I18n[K.Note],
		"tag" => I18n[K.Tag],
		"source" => I18n[K.Source],
		"alias" => I18n[K.Alias],
		"pronunciation" => I18n[K.Pronunciation],
		"weight" => I18n[K.Weight],
		"learn" => I18n[K.Learn],
		"usage" => I18n[K.Usage],
		"example" => I18n[K.Example],
		"relation" => I18n[K.Relation],
		"ref" => I18n[K.Ref],
		_ => raw,
	};
}

i32 GetTypeIndex(EPreFilterType type){
	if(PoTypeValues.Count == 0){
		return 0;
	}
	for(i32 i = 0; i < PoTypeValues.Count; i++){
		if(PoTypeValues[i] == type){
			return i;
		}
	}
	return 0;
}

EPreFilterType GetTypeByIndex(i32 idx){
	if(PoTypeValues.Count == 0){
		return EPreFilterType.Json;
	}
	var clamped = ClampIndex(idx, PoTypeValues.Count);
	return PoTypeValues[clamped];
}
}



