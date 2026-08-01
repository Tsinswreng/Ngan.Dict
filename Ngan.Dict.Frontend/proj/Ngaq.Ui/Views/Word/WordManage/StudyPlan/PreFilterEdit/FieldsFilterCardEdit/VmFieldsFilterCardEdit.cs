namespace Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.FieldsFilterCardEdit;

using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using Ngaq;
using Ngaq.Core.Shared.StudyPlan.Models.PreFilter;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.FieldsFilterCardEdit.FilterItemEdit;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.PreFilterVisualEdit;

using Ctx = VmFieldsFilterCardEdit;using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

/// <summary>
/// Editor VM for one FieldsFilter group.
/// It separates field editing and filter-item editing.
/// </summary>
public class VmFieldsFilterCardEdit: ViewModelBase, IMk<Ctx>{
	protected VmFieldsFilterCardEdit(){}

	static readonly i32 DefaultOperationRawIndex = (i32)EFilterOperationMode.Eq;
	static readonly i32 DefaultValueTypeRawIndex = (i32)EValueType.String;

	public static Ctx Mk(){
		return new Ctx();
	}

	VmPreFilterVisualEdit? Owner{get;set;}
	VmPreFilterVisualEdit.VmFieldsFilterRow? Target{get;set;}
	bool IsCore{get;set;}
	u64 RowIdx{get;set;}

	public class RowTextOption{
		public str Raw{get;set;} = "";
		public str Display{get;set;} = "";
	}

	public class RowFilterItemCard{
		public u64 UiIdx{get;set;}
		public str UiIdxText{get;set;} = "";
		public str Operation{get;set;} = "";
		public str ValueType{get;set;} = "";
		public str ValuesPreview{get;set;} = "";
		public VmPreFilterVisualEdit.VmFilterItemRow? Raw{get;set;}
	}

	public ObservableCollection<VmPreFilterVisualEdit.VmFieldValueRow> Fields{get;set;} = [];
	public ObservableCollection<VmPreFilterVisualEdit.VmFilterItemRow> Items{get;set;} = [];
	public ObservableCollection<RowFilterItemCard> ItemCards{get;set;} = [];

	public IReadOnlyList<str> FieldOptions => IsCore
		? VmPreFilterVisualEdit.CoreWordFieldOptions
		: Owner?.PropFieldOptions ?? [];
	public IReadOnlyList<RowTextOption> FieldOptionRows => FieldOptions
		.Select(x=>new RowTextOption{
			Raw = x,
			Display = ToFieldDisplay(x),
		})
		.ToList();

	public IReadOnlyList<i32> OperationRawIndices{get;} = Enum
		.GetValues<EFilterOperationMode>()
		.Select((x, idx)=>(x, idx))
		.Where(x=>x.x != EFilterOperationMode.Null)
		.Select(x=>(i32)x.idx)
		.ToList();
	public IReadOnlyList<str> OperationOptionsDisplay => OperationRawIndices
		.Select(ToOperationDisplayByRawIndex)
		.ToList();

	public IReadOnlyList<i32> ValueTypeRawIndices{get;} = Enum
		.GetValues<EValueType>()
		.Select((x, idx)=>(x, idx))
		.Where(x=>x.x != EValueType.Null)
		.Select(x=>(i32)x.idx)
		.ToList();
	public IReadOnlyList<str> ValueTypeOptionsDisplay => ValueTypeRawIndices
		.Select(ToValueTypeDisplayByRawIndex)
		.ToList();

	public nil Load(
		VmPreFilterVisualEdit Owner,
		VmPreFilterVisualEdit.VmFieldsFilterRow Target,
		bool IsCore,
		u64 RowIdx
	){
		this.Owner = Owner;
		this.Target = Target;
		this.IsCore = IsCore;
		this.RowIdx = RowIdx;

		Fields.Clear();
		foreach(var field in Target.Fields){
			Fields.Add(new VmPreFilterVisualEdit.VmFieldValueRow{
				Value = field.Value,
			});
		}

		Items.Clear();
		foreach(var item in Target.Items){
			Items.Add(CloneItem(item));
		}

		RefreshItemCards();
		OnPropertyChanged(nameof(FieldOptions));
		OnPropertyChanged(nameof(FieldOptionRows));
		OnPropertyChanged(nameof(OperationOptionsDisplay));
		OnPropertyChanged(nameof(ValueTypeOptionsDisplay));
		return NIL;
	}

	public nil AddField(){
		Fields.Add(new VmPreFilterVisualEdit.VmFieldValueRow());
		return NIL;
	}

	public nil RemoveField(VmPreFilterVisualEdit.VmFieldValueRow Field){
		Fields.Remove(Field);
		return NIL;
	}

	public nil AddItem(){
		Items.Add(VmPreFilterVisualEdit.MkFilterItemRow());
		CommitItemsDraft();
		return NIL;
	}

	public nil RefreshItemCards(){
		ItemCards.Clear();
		for(u64 i = 0; i < (u64)Items.Count; i++){
			var item = Items[(i32)i];
			ItemCards.Add(new RowFilterItemCard{
				UiIdx = i + 1,
				UiIdxText = (i + 1).ToString(),
				Operation = ToOperationDisplayByRawIndex(item.OperationIndex),
				ValueType = ToValueTypeDisplayByRawIndex(item.ValueTypeIndex),
				ValuesPreview = ValuesPreview(item.ValuesText),
				Raw = item,
			});
		}
		return NIL;
	}

	public nil OpenFilterItem(RowFilterItemCard? Card){
		if(Card?.Raw is null){
			return NIL;
		}
		var view = new ViewFilterItemEdit();
		view.Ctx?.Load(this, Card.Raw, Card.UiIdx);
        view.SyncSelectionFromVm();
		ViewNavi?.GoTo(ToolView.WithTitle(I18n[K.FilterItem] + " #" + Card.UiIdx, view));
		return NIL;
	}

	public nil RemoveItem(VmPreFilterVisualEdit.VmFilterItemRow? Item){
		if(Item is null){
			return NIL;
		}
		Items.Remove(Item);
		CommitItemsDraft();
		return NIL;
	}

	public nil CommitItemsDraft(){
		RefreshItemCards();
		if(Target is null || Owner is null){
			return NIL;
		}
		Target.Items.Clear();
		foreach(var item in Items){
			Target.Items.Add(CloneItem(item));
		}
		Owner.RefreshFieldsFilterCards();
		return NIL;
	}

	public nil Save(){
		if(Target is null || Owner is null){
			ShowDialog(I18n[K.EditorNotReady]);
			return NIL;
		}

		Target.Fields.Clear();
		foreach(var field in Fields){
			Target.Fields.Add(new VmPreFilterVisualEdit.VmFieldValueRow{
				Value = field.Value,
			});
		}

		Target.Items.Clear();
		foreach(var item in Items){
			Target.Items.Add(CloneItem(item));
		}

		Owner.RefreshFieldsFilterCards();
		var kind = IsCore ? I18n[K.Core] : I18n[K.Prop];
		ShowToast(I18n.Get(K.Saved__Filter__No__, kind, RowIdx));
		ViewNavi?.Back();
		return NIL;
	}

	public nil Delete(){
		if(Target is null || Owner is null){
			ShowDialog(I18n[K.EditorNotReady]);
			return NIL;
		}
		if(IsCore){
			Owner.CoreFilterRows.Remove(Target);
		}else{
			Owner.PropFilterRows.Remove(Target);
		}
		Owner.RefreshFieldsFilterCards();
		ShowToast(I18n[K.Deleted]);
		ViewNavi?.Back();
		return NIL;
	}

	public i32 ToOperationOptionIndex(i32 rawIndex){
		for(i32 i = 0; i < OperationRawIndices.Count; i++){
			if(OperationRawIndices[i] == rawIndex){
				return i;
			}
		}
		return IndexOfRawOrZero(OperationRawIndices, DefaultOperationRawIndex);
	}

	public i32 ToOperationRawIndex(i32 optionIndex){
		if(OperationRawIndices.Count == 0){
			return DefaultOperationRawIndex;
		}
		var i = ClampIndex(optionIndex, OperationRawIndices.Count);
		return OperationRawIndices[i];
	}

	public i32 ToValueTypeOptionIndex(i32 rawIndex){
		for(i32 i = 0; i < ValueTypeRawIndices.Count; i++){
			if(ValueTypeRawIndices[i] == rawIndex){
				return i;
			}
		}
		return IndexOfRawOrZero(ValueTypeRawIndices, DefaultValueTypeRawIndex);
	}

	public i32 ToValueTypeRawIndex(i32 optionIndex){
		if(ValueTypeRawIndices.Count == 0){
			return DefaultValueTypeRawIndex;
		}
		var i = ClampIndex(optionIndex, ValueTypeRawIndices.Count);
		return ValueTypeRawIndices[i];
	}

	/// <summary>
	/// 將字段原始鍵轉成界面顯示文字；若無對應翻譯則回退原值。
	/// </summary>
	public str ToFieldDisplay(str raw){
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

	/// <summary>
	/// 將界面顯示文字映射回原始字段鍵；找不到對應時保留用戶輸入。
	/// </summary>
	public str ToFieldRaw(str? displayOrRaw){
		var text = displayOrRaw?.Trim() ?? "";
		if(str.IsNullOrWhiteSpace(text)){
			return "";
		}
		foreach(var option in FieldOptionRows){
			if(option.Display == text || option.Raw == text){
				return option.Raw;
			}
		}
		return text;
	}


	/// <summary>
	/// 供編輯頁回填下拉框使用：將 raw 值轉成顯示文字。
	/// </summary>
	public str ToOperationDisplayByRawIndexForEdit(i32 rawIndex){
		return ToOperationDisplayByRawIndex(rawIndex);
	}

	/// <summary>
	/// 供編輯頁回填下拉框使用：將 raw 值轉成顯示文字。
	/// </summary>
	public str ToValueTypeDisplayByRawIndexForEdit(i32 rawIndex){
		return ToValueTypeDisplayByRawIndex(rawIndex);
	}

	/// <summary>
	/// 根據顯示文字反查運算 raw 值；找不到時回退到等於。
	/// </summary>
	public i32 OperationDisplayToRawIndex(str? display){
		var text = display?.Trim() ?? "";
		for(i32 i = 0; i < OperationRawIndices.Count; i++){
			var raw = OperationRawIndices[i];
			if(ToOperationDisplayByRawIndex(raw) == text){
				return raw;
			}
		}
		return DefaultOperationRawIndex;
	}

	/// <summary>
	/// 根據顯示文字反查值類型 raw 值；找不到時回退到字符串。
	/// </summary>
	public i32 ValueTypeDisplayToRawIndex(str? display){
		var text = display?.Trim() ?? "";
		for(i32 i = 0; i < ValueTypeRawIndices.Count; i++){
			var raw = ValueTypeRawIndices[i];
			if(ToValueTypeDisplayByRawIndex(raw) == text){
				return raw;
			}
		}
		return DefaultValueTypeRawIndex;
	}
	str ToOperationDisplayByRawIndex(i32 rawIndex){
		var mode = EnumByRawIndex<EFilterOperationMode>(rawIndex);
		return mode switch{
			EFilterOperationMode.Eq => "=",
			EFilterOperationMode.Ne => "!=",
			EFilterOperationMode.Gt => ">",
			EFilterOperationMode.Ge => ">=",
			EFilterOperationMode.Lt => "<",
			EFilterOperationMode.Le => "<=",
			EFilterOperationMode.IncludeAny => I18n[K.IncludeAny],
			EFilterOperationMode.IncludeAll => I18n[K.IncludeAll],
			EFilterOperationMode.ExcludeAll => I18n[K.ExcludeAll],
			_ => mode.ToString(),
		};
	}

	str ToValueTypeDisplayByRawIndex(i32 rawIndex){
		var type = EnumByRawIndex<EValueType>(rawIndex);
		return type switch{
			EValueType.String => I18n[K.String],
			EValueType.Number => I18n[K.Number],
			EValueType.Null => I18n[K.Null],
			_ => type.ToString(),
		};
	}

	static TEnum EnumByRawIndex<TEnum>(i32 rawIndex)
		where TEnum : struct, Enum
	{
		var values = Enum.GetValues<TEnum>();
		if(rawIndex < 0 || rawIndex >= values.Length){
			return values[0];
		}
		return values[rawIndex];
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

	static i32 IndexOfRawOrZero(IReadOnlyList<i32> raws, i32 targetRaw){
		for(i32 i = 0; i < raws.Count; i++){
			if(raws[i] == targetRaw){
				return i;
			}
		}
		return 0;
	}

	static str ValuesPreview(str text){
		if(str.IsNullOrWhiteSpace(text)){
			return "";
		}
		const i32 maxLen = 80;
		var oneLine = text.Replace("\r", " ").Replace("\n", " | ");
		return oneLine.Length <= maxLen ? oneLine : oneLine[..maxLen] + "...";
	}

	static VmPreFilterVisualEdit.VmFilterItemRow CloneItem(VmPreFilterVisualEdit.VmFilterItemRow Src){
		return new VmPreFilterVisualEdit.VmFilterItemRow{
			OperationIndex = Src.OperationIndex,
			ValueTypeIndex = Src.ValueTypeIndex,
			ValuesText = Src.ValuesText,
		};
	}
}



