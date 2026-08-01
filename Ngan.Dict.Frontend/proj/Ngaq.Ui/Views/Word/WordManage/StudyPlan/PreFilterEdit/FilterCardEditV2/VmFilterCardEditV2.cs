namespace Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.FilterCardEditV2;

using System;
using System.Collections.Generic;
using System.Linq;
using Ngaq.Core.Shared.StudyPlan.Models.PreFilter;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.PreFilterVisualEditV2;

using Ctx = VmFilterCardEditV2;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

/// <summary>
/// 單個篩選卡片 V2 編輯 VM。
/// 規則：字段僅 1 個、篩選項僅 1 個，且全部在同一頁編輯。
/// </summary>
public class VmFilterCardEditV2: ViewModelBase, IMk<Ctx>{
	public static Ctx Mk(){ return new Ctx(); }

	VmPreFilterVisualEditV2? Owner{get;set;}
	VmPreFilterVisualEditV2.VmFieldsFilterRow? Target{get;set;}
	u64 RowIdx{get;set;}
	public event Action? OnBackRequested;
	public event Action<str>? OnDialogRequested;
	public event Action<str>? OnToastRequested;
	static readonly i32 DefaultOperationRawIndex = (i32)EFilterOperationMode.Eq;
	static readonly i32 DefaultValueTypeRawIndex = (i32)EValueType.String;

	public str Field{
		get=>field;
		set{
			if(SetProperty(ref field, value)){
				OnPropertyChanged(nameof(FieldDisplay));
			}
		}
	} = "";
	public str FieldDisplay{
		get=>field;
		set{
			if(SetProperty(ref field, value)){
				Field = ToFieldRaw(value);
			}
		}
	} = "";
	public i32 OperationIndex{
		get=>field;
		set{
			if(SetProperty(ref field, value)){
				OnPropertyChanged(nameof(OperationOptionIndex));
			}
		}
	} = DefaultOperationRawIndex;
	public i32 ValueTypeIndex{
		get=>field;
		set{
			if(SetProperty(ref field, value)){
				OnPropertyChanged(nameof(ValueTypeOptionIndex));
			}
		}
	} = DefaultValueTypeRawIndex;

	public str ValuesText{get=>field;set{SetProperty(ref field, value);}} = "";

	public IReadOnlyList<str> FieldOptionsRaw{get;} = VmPreFilterVisualEditV2.DefaultFieldOptions;
	public IReadOnlyList<str> FieldOptionsDisplay{get;}
	public IReadOnlyList<str> OperationOptionsDisplay{get;}
	public IReadOnlyList<str> ValueTypeOptionsDisplay{get;}
	public IReadOnlyList<i32> OperationRawIndices{get;} = Enum
		.GetValues<EFilterOperationMode>()
		.Select((x, idx)=>(x, idx))
		.Where(x=>x.x != EFilterOperationMode.Null)
		.Select(x=>(i32)x.idx)
		.ToList();
	public IReadOnlyList<i32> ValueTypeRawIndices{get;} = Enum
		.GetValues<EValueType>()
		.Select((x, idx)=>(x, idx))
		.Where(x=>x.x != EValueType.Null)
		.Select(x=>(i32)x.idx)
		.ToList();

	public i32 OperationOptionIndex{
		get=>field;
		set{
			if(SetProperty(ref field, value)){
				OperationIndex = OperationOptionIndexToRawIndex(value);
			}
		}
	} = 0;
	public i32 ValueTypeOptionIndex{
		get=>field;
		set{
			if(SetProperty(ref field, value)){
				ValueTypeIndex = ValueTypeOptionIndexToRawIndex(value);
			}
		}
	} = 0;

	public VmFilterCardEditV2(){
		FieldOptionsDisplay = FieldOptionsRaw.Select(ToFieldDisplay).ToList();
		OperationOptionsDisplay = OperationRawIndices.Select(ToOperationDisplayByRawIndex).ToList();
		ValueTypeOptionsDisplay = ValueTypeRawIndices.Select(ToValueTypeDisplayByRawIndex).ToList();
	}

	public nil Load(VmPreFilterVisualEditV2 Owner, VmPreFilterVisualEditV2.VmFieldsFilterRow Target, u64 RowIdx){
		this.Owner = Owner;
		this.Target = Target;
		this.RowIdx = RowIdx;

		Field = Target.Fields.FirstOrDefault()?.Value ?? nameof(PoWord.Lang);
		if(str.IsNullOrWhiteSpace(Field)){
			Field = nameof(PoWord.Lang);
		}
		FieldDisplay = ToFieldDisplay(Field);
		var item = Target.Items.FirstOrDefault();
		OperationIndex = item?.OperationIndex ?? (i32)EFilterOperationMode.IncludeAny;
		ValueTypeIndex = item?.ValueTypeIndex ?? DefaultValueTypeRawIndex;
		OperationOptionIndex = ToOperationOptionIndex(OperationIndex);
		ValueTypeOptionIndex = ToValueTypeOptionIndex(ValueTypeIndex);
		ValuesText = item?.ValuesText ?? "";
		OnPropertyChanged(nameof(FieldOptionsDisplay));
		OnPropertyChanged(nameof(OperationOptionsDisplay));
		OnPropertyChanged(nameof(ValueTypeOptionsDisplay));
		OnPropertyChanged(nameof(FieldDisplay));
		OnPropertyChanged(nameof(OperationOptionIndex));
		OnPropertyChanged(nameof(ValueTypeOptionIndex));
		return NIL;
	}

	public nil Save(){
		if(Owner is null || Target is null){
			OnDialogRequested?.Invoke(I18n[K.EditorNotReady]);
			return NIL;
		}

		// step 1: 僅保留 1 個字段。
		Target.Fields.Clear();
		var field = ToFieldRaw(FieldDisplay);
		if(!str.IsNullOrWhiteSpace(field)){
			Target.Fields.Add(new VmPreFilterVisualEditV2.VmFieldValueRow{Value = field});
		}

		// step 2: 僅保留 1 個篩選項。
		Target.Items.Clear();
		if(!str.IsNullOrWhiteSpace(ValuesText)){
			Target.Items.Add(new VmPreFilterVisualEditV2.VmFilterItemRow{
				OperationIndex = OperationOptionIndexToRawIndex(OperationOptionIndex),
				ValueTypeIndex = ValueTypeOptionIndexToRawIndex(ValueTypeOptionIndex),
				ValuesText = ValuesText,
			});
		}

		Owner.RefreshFieldsFilterCards();
		OnDialogRequested?.Invoke(I18n.Get(K.Saved__Filter__No__, I18n[K.PreFilter], RowIdx));
		OnBackRequested?.Invoke();
		return NIL;
	}

	public nil Delete(){
		if(Owner is null || Target is null){
			OnDialogRequested?.Invoke(I18n[K.EditorNotReady]);
			return NIL;
		}
		Owner.FilterRows.Remove(Target);
		Owner.RefreshFieldsFilterCards();
		OnToastRequested?.Invoke(I18n[K.Deleted]);
		OnBackRequested?.Invoke();
		return NIL;
	}

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

	public str ToFieldRaw(str? displayOrRaw){
		var text = displayOrRaw?.Trim() ?? "";
		if(str.IsNullOrWhiteSpace(text)){
			return "";
		}
		for(i32 i = 0; i < FieldOptionsRaw.Count; i++){
			var raw = FieldOptionsRaw[i];
			if(ToFieldDisplay(raw) == text || raw == text){
				return raw;
			}
		}
		return text;
	}

	public i32 OperationOptionIndexToRawIndex(i32 optionIndex){
		if(OperationRawIndices.Count == 0){
			return DefaultOperationRawIndex;
		}
		if(optionIndex < 0){
			optionIndex = 0;
		}
		if(optionIndex >= OperationRawIndices.Count){
			optionIndex = OperationRawIndices.Count - 1;
		}
		return OperationRawIndices[optionIndex];
	}

	public i32 ValueTypeOptionIndexToRawIndex(i32 optionIndex){
		if(ValueTypeRawIndices.Count == 0){
			return DefaultValueTypeRawIndex;
		}
		if(optionIndex < 0){
			optionIndex = 0;
		}
		if(optionIndex >= ValueTypeRawIndices.Count){
			optionIndex = ValueTypeRawIndices.Count - 1;
		}
		return ValueTypeRawIndices[optionIndex];
	}

	public i32 ToOperationOptionIndex(i32 rawIndex){
		for(i32 i = 0; i < OperationRawIndices.Count; i++){
			if(OperationRawIndices[i] == rawIndex){
				return i;
			}
		}
		return 0;
	}

	public i32 ToValueTypeOptionIndex(i32 rawIndex){
		for(i32 i = 0; i < ValueTypeRawIndices.Count; i++){
			if(ValueTypeRawIndices[i] == rawIndex){
				return i;
			}
		}
		return 0;
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

	static TEnum EnumByRawIndex<TEnum>(i32 rawIndex) where TEnum : struct, Enum{
		var values = Enum.GetValues<TEnum>();
		if(rawIndex < 0 || rawIndex >= values.Length){
			return values[0];
		}
		return values[rawIndex];
	}
}

