namespace Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.PreFilterVisualEditV2;

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Shared.StudyPlan.Models;
using Ngaq.Core.Shared.StudyPlan.Models.Po.PreFilter;
using Ngaq.Core.Shared.StudyPlan.Models.PreFilter;
using Ngaq.Core.Shared.StudyPlan.Svc;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Tools.Json;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.I18n;
using Ngaq.Ui.Tools;
using Tsinswreng.CsTools;

using Ctx = VmPreFilterVisualEditV2;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

/// <summary>
/// PreFilter 可視化編輯器 V2。
/// V2 不再區分 Core/Prop 兩組，全部按單一列表編輯，解析責任交由後端。
/// </summary>
public class VmPreFilterVisualEditV2: ViewModelBase, IMk<Ctx>{
	protected VmPreFilterVisualEditV2(){
		InitRowEvents();
	}

	public static Ctx Mk(){
		return new Ctx();
	}

	ISvcStudyPlan? SvcStudyPlan{get;set;}
	IFrontendUserCtxMgr? UserCtxMgr{get;set;}
	IJsonSerializer JsonSerializer{get;set;} = AppJsonSerializer.Inst;
	bool _isHydrating = false;
	public event Action<BoPreFilter>? OnOpenPayloadJsonRequested;
	public event Action<BoPreFilter>? OnOpenLegacyVisualEditorRequested;
	public event Action<RowFieldsFilterCard>? OnOpenFilterCardRequested;
	public event Action<str>? OnDialogRequested;
	public event Action<str>? OnToastRequested;
	public static readonly IReadOnlyList<str> DefaultFieldOptions = [
		nameof(PoWord.Head),
		nameof(PoWord.Lang),
		nameof(PoWord.BizCreatedAt),
		nameof(PoWord.BizUpdatedAt),
		"summary",
		"description",
		"note",
		"tag",
		"source",
		"alias",
		"pronunciation",
		"weight",
		"learn",
		"usage",
		"example",
		"relation",
		"ref",
	];

	public VmPreFilterVisualEditV2(
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
		public str ContentPreview{get;set;} = "";
		public str FilterCountText{get;set;} = "";
		public VmFieldsFilterRow? Raw{get;set;}
	}

	public BoPreFilter BoPreFilter{
		get;
		set{SetProperty(ref field, value);}
	} = MkEmptyBoPreFilter();

	public str LastError{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public bool HasError => !str.IsNullOrWhiteSpace(LastError);

	public bool IsCreateMode{
		get;
		set{SetProperty(ref field, value);}
	} = true;

	public ObservableCollection<VmFieldsFilterRow> FilterRows{get;set;} = [];
	public ObservableCollection<RowFieldsFilterCard> FilterCards{get;set;} = [];

	public IReadOnlyList<EPreFilterType> PoTypeValues{get;} = Enum
		.GetValues<EPreFilterType>()
		.Where(x=>x != EPreFilterType.Unknown)
		.ToList();
	public IReadOnlyList<str> PoTypeOptions{get;} = Enum
		.GetValues<EPreFilterType>()
		.Where(x=>x != EPreFilterType.Unknown)
		.Select(x=>x.ToString())
		.ToList();
	public bool ShowPoTypeField => PoTypeOptions.Count > 1;

	public str PoIdText{get=>field;set{SetProperty(ref field, value);}} = "";
	public str PoUniqName{get=>field;set{SetProperty(ref field, value);}} = "";
	public str PoDescr{get=>field;set{SetProperty(ref field, value);}} = "";
	public i32 PoTypeIndex{get=>field;set{SetProperty(ref field, value);}} = 0;
	public str PreFilterVersion{get=>field;set{SetProperty(ref field, value);}} = "1.0.0.0";

	void InitRowEvents(){
		FilterRows.CollectionChanged += OnFieldsRowsChanged;
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

	void OnFieldValueChanged(object? sender, PropertyChangedEventArgs e){ TouchRows(); }
	void OnFilterItemChanged(object? sender, PropertyChangedEventArgs e){ TouchRows(); }

	void TouchRows(){
		if(_isHydrating){
			return;
		}
		RefreshFieldsFilterCards();
	}

	public nil RefreshFieldsFilterCards(){
		FilterCards.Clear();
		for(u64 i = 0; i < (u64)FilterRows.Count; i++){
			var row = FilterRows[(i32)i];
			FilterCards.Add(new RowFieldsFilterCard{
				UiIdx = i + 1,
				UiIdxText = (i + 1).ToString(),
				ContentPreview = BuildContentPreview(row),
				FilterCountText = (row.Items?.Count ?? 0).ToString(),
				Raw = row,
			});
		}
		return NIL;
	}

	public nil SetCreateMode(bool IsCreate){
		IsCreateMode = IsCreate;
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

	public nil OpenPayloadJsonEditor(){
		if(!TryBuildBoFromVisual(out var bo, out var err)){
			LastError = err;
			OnPropertyChanged(nameof(HasError));
			OnDialogRequested?.Invoke(err);
			return NIL;
		}
		LastError = "";
		OnPropertyChanged(nameof(HasError));
		BoPreFilter = bo;
		OnOpenPayloadJsonRequested?.Invoke(bo);
		return NIL;
	}

	public void OnPayloadJsonSavedOrDeleted(PoPreFilter? Po){
		if(Po is null){
			IsCreateMode = true;
			FromPoPreFilter(null);
			return;
		}
		IsCreateMode = Po.Id == default;
		FromPoPreFilter(Po);
	}

	public nil OpenLegacyVisualEditor(){
		if(!TryBuildBoFromVisual(out var bo, out var err)){
			OnDialogRequested?.Invoke(err);
			return NIL;
		}
		OnOpenLegacyVisualEditorRequested?.Invoke(bo);
		return NIL;
	}

	public async Task<nil> Save(CT Ct = default){
		if(AnyNull(SvcStudyPlan, UserCtxMgr)){
			return NIL;
		}
		if(!TryBuildBoFromVisual(out var bo, out var err)){
			LastError = err;
			OnPropertyChanged(nameof(HasError));
			OnDialogRequested?.Invoke(err);
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
			OnToastRequested?.Invoke(I18n[K.Saved]);
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
			OnToastRequested?.Invoke(I18n[K.Deleted]);
		}catch(Exception e){
			HandleErr(e);
		}
		return NIL;
	}

	public nil AddGroup(){
		FilterRows.Add(new VmFieldsFilterRow());
		RefreshFieldsFilterCards();
		return NIL;
	}

	public nil OpenFilterCard(RowFieldsFilterCard? card){
		if(card?.Raw is null){
			return NIL;
		}
		OnOpenFilterCardRequested?.Invoke(card);
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
			PreFilterVersion = (pre.Version ?? new Version(1,0,0,0)).ToString();
			FilterRows.Clear();
			foreach(var row in (pre.CoreFilter ?? []).Concat(pre.PropFilter ?? []).Select(MkRowFromModel)){
				FilterRows.Add(row);
			}
			RefreshFieldsFilterCards();
			LastError = "";
			OnPropertyChanged(nameof(HasError));
		}finally{
			_isHydrating = false;
		}
	}

	VmFieldsFilterRow MkRowFromModel(FieldsFilter row){
		var vm = new VmFieldsFilterRow();
		foreach(var field in row.Fields ?? []){
			vm.Fields.Add(new VmFieldValueRow{Value = field ?? ""});
		}
		foreach(var item in (row.Filters ?? [])){
			vm.Items.Add(new VmFilterItemRow{
				OperationIndex = (i32)item.Operation,
				ValueTypeIndex = (i32)item.ValueType,
				ValuesText = string.Join(Environment.NewLine, (item.Values ?? []).Select(x => x?.ToString() ?? "")),
			});
		}
		return vm;
	}

	bool TryBuildBoFromVisual(out BoPreFilter Bo, out str Err){
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

			var rows = BuildFieldsFilterList(FilterRows, out var rowErr);
			if(rowErr is not null){
				Err = rowErr;
				return false;
			}

			var pre = new Ngaq.Core.Shared.StudyPlan.Models.PreFilter.PreFilter{
				Version = ver,
				CoreFilter = rows,
				PropFilter = [],
			};
			po.DataSchemaVer = pre.Version;
			po.Text = JsonSerializer.Stringify(pre);
			po.Binary = null;
			Bo = new BoPreFilter{ PoPreFilter = po, PreFilter = pre };
			return true;
		}catch(Exception e){
			Err = e.Message;
			return false;
		}
	}

	IList<FieldsFilter> BuildFieldsFilterList(IEnumerable<VmFieldsFilterRow> rows, out str? Err){
		Err = null;
		var ans = new List<FieldsFilter>();
		var rowIdx = 0;
		foreach(var row in rows){
			rowIdx++;
			var fields = row.Fields.Select(x=>x.Value?.Trim() ?? "").Where(x=>!str.IsNullOrWhiteSpace(x)).Take(1).ToList();
			var filters = new List<FilterItem>();
			if(row.Items.Count > 0){
				var item = row.Items[0];
				var valueType = EnumOrDefault<EValueType>(item.ValueTypeIndex);
				var values = ParseValues(item.ValuesText, valueType, out var parseErr);
				if(parseErr is not null){
					Err = $"Row#{rowIdx}: {parseErr}";
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
			ans.Add(new FieldsFilter{ Fields = fields, Filters = filters });
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
		return Text.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Where(x=>!str.IsNullOrWhiteSpace(x));
	}

	static TEnum EnumOrDefault<TEnum>(i32 Index) where TEnum : struct, Enum{
		var values = Enum.GetValues<TEnum>();
		if(Index < 0 || Index >= values.Length){
			return values[0];
		}
		return values[Index];
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
		var values = SplitLines(item.ValuesText).ToList();
		if(values.Count == 0){
			return "-";
		}
		return $"{ToFieldText(field)} {ToOpText(item.OperationIndex)} {string.Join(" | ", values)}";
	}

	str ToFieldText(str raw){
		if(str.IsNullOrWhiteSpace(raw)){
			return "";
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

	static BoPreFilter MkEmptyBoPreFilter(){
		return new BoPreFilter{
			PoPreFilter = new PoPreFilter{ Type = EPreFilterType.Json },
			PreFilter = new Ngaq.Core.Shared.StudyPlan.Models.PreFilter.PreFilter{
				Version = new Version(1, 0, 0, 0),
				CoreFilter = [],
				PropFilter = [],
			},
		};
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
		if(idx < 0){ idx = 0; }
		if(idx >= PoTypeValues.Count){ idx = PoTypeValues.Count - 1; }
		return PoTypeValues[idx];
	}
}

