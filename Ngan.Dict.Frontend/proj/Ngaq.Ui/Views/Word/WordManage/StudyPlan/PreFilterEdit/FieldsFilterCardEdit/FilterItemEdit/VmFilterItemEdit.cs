namespace Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.FieldsFilterCardEdit.FilterItemEdit;

using System.Collections.Generic;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.FieldsFilterCardEdit;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.PreFilterVisualEdit;

using Ctx = VmFilterItemEdit;using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

/// <summary>
/// Editor VM for a single filter item row.
/// </summary>
public class VmFilterItemEdit: ViewModelBase, IMk<Ctx>{
	protected VmFilterItemEdit(){}

	public static Ctx Mk(){
		return new Ctx();
	}

	VmFieldsFilterCardEdit? Owner{get;set;}
	VmPreFilterVisualEdit.VmFilterItemRow? Target{get;set;}
	u64 ItemIdx{get;set;}

	public IReadOnlyList<str> OperationOptions => Owner?.OperationOptionsDisplay ?? [];
	public IReadOnlyList<str> ValueTypeOptions => Owner?.ValueTypeOptionsDisplay ?? [];

	public str SelectedOperation{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public str SelectedValueType{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public str ValuesText{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public nil Load(
		VmFieldsFilterCardEdit Owner,
		VmPreFilterVisualEdit.VmFilterItemRow Target,
		u64 ItemIdx
	){
		this.Owner = Owner;
		this.Target = Target;
		this.ItemIdx = ItemIdx;
		SelectedOperation = Owner.ToOperationDisplayByRawIndexForEdit(Target.OperationIndex);
		SelectedValueType = Owner.ToValueTypeDisplayByRawIndexForEdit(Target.ValueTypeIndex);
		ValuesText = Target.ValuesText;
		OnPropertyChanged(nameof(OperationOptions));
		OnPropertyChanged(nameof(ValueTypeOptions));
		OnPropertyChanged(nameof(SelectedOperation));
		OnPropertyChanged(nameof(SelectedValueType));
		return NIL;
	}

	public nil Save(){
		if(Target is null || Owner is null){
			ShowDialog(I18n[K.EditorNotReady]);
			return NIL;
		}
		Target.OperationIndex = Owner.OperationDisplayToRawIndex(SelectedOperation);
		Target.ValueTypeIndex = Owner.ValueTypeDisplayToRawIndex(SelectedValueType);
		Target.ValuesText = ValuesText;
		Owner.CommitItemsDraft();
		ShowToast(I18n.Get(K.SavedFilterItem__No__, ItemIdx));
		ViewNavi?.Back();
		return NIL;
	}

	public nil Delete(){
		if(Target is null || Owner is null){
			ShowDialog(I18n[K.EditorNotReady]);
			return NIL;
		}
		Owner.RemoveItem(Target);
		ShowToast(I18n.Get(K.DeletedFilterItem__No__, ItemIdx));
		ViewNavi?.Back();
		return NIL;
	}
}
