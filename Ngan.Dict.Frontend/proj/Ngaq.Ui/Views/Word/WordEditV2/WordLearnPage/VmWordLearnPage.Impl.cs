namespace Ngaq.Ui.Views.Word.WordEditV2.WordLearnPage;

using System.Collections.ObjectModel;
using System.Globalization;
using Ngaq.Core.Infra;
using Ngaq.Core.Model.Po.Learn_;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Shared.Word.Models.Po.Learn;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Views.Word;
using Tsinswreng.CsTempus;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

/// 學習記錄分頁 ViewModel：管理列表、新增、轉換。
public partial class VmWordLearnPage: ViewModelBase{
	public partial nil LoadFromPoLearns(IList<PoWordLearn> Learns){
		Rows = new ObservableCollection<VmWordLearnRow>(Learns.Select(VmWordLearnRow.FromPo));
		return NIL;
	}
	public partial nil AddRow(){
		Rows.Add(VmWordLearnRow.NewRow());
		return NIL;
	}
	public partial nil RemoveRow(VmWordLearnRow Row){
		if(Row.DmlState == EDmlState.Added){
			Rows.Remove(Row);
		}else{
			Row.DmlState = EDmlState.Removed;
			Rows.Remove(Row);
			RemovedRows.Add(Row);
		}
		return NIL;
	}
	public partial nil RemovePersistedRow(VmWordLearnRow Row){
		Rows.Remove(Row);
		RemovedRows.Remove(Row);
		return NIL;
	}
	public partial nil RequestEdit(VmWordLearnRow Row){
		OnEditRequested?.Invoke(Row);
		return NIL;
	}
	public partial nil OnSaved(){
		foreach(var row in Rows){
			row.DmlState = EDmlState.Unchanged;
		}
		RemovedRows.Clear();
		return NIL;
	}
	public partial bool TryBuildPoLearns(IdWord WordId, out List<PoWordLearn> Learns, out str Err){
		Learns = [];
		Err = "";
		for(i32 i = 0; i < Rows.Count; i++){
			var row = Rows[i];
			if(!row.TryToPo(WordId, out var po, out var rowErr)){
				Err = I18n.Get(K.Learn__Err__, i+1, rowErr);
				return false;
			}
			Learns.Add(po);
		}
		return true;
	}
}

/// 學習記錄行 ViewModel。
public partial class VmWordLearnRow: ViewModelBase{
	partial void MarkModified(){
		if(DmlState == EDmlState.Unchanged){
			DmlState = EDmlState.Modified;
		}
	}
	public static partial VmWordLearnRow NewRow(){
		return new VmWordLearnRow{
			Raw = new PoWordLearn{
				Id = new IdWordLearn(),
			},
			LearnResultIndex = 0,
			// 放最後，覆蓋屬性初始化時觸發的 MarkModified
			DmlState = EDmlState.Added,
		};
	}
	public static partial VmWordLearnRow FromPo(PoWordLearn Po){
		return new VmWordLearnRow{
			Raw = (PoWordLearn)Po.ShallowCloneSelf(),
			LearnResultIndex = GetLearnResultIndex(Po.LearnResult),
			BizCreatedAtIso = Po.BizCreatedAt.ToIso(),
			// 放最後，覆蓋屬性初始化時觸發的 MarkModified
			DmlState = EDmlState.Unchanged,
		};
	}
	public partial bool TryToPo(IdWord WordId, out PoWordLearn Po, out str Err){
		Err = "";
		Po = (PoWordLearn)Raw.ShallowCloneSelf();
		Po.WordId = WordId;
		Po.LearnResult = GetLearnResultByIndex(LearnResultIndex);
		try{
			Po.BizCreatedAt = UnixMs.FromIso(BizCreatedAtIso);
		}catch{
			Err = I18n[K.BizCreatedAtMustBeIsoTime];
			return false;
		}
		return true;
	}
	private static partial i32 GetLearnResultIndex(ELearn Learn){
		for(i32 i = 0; i < LearnResults.Count; i++){
			if(LearnResults[i] == Learn){
				return i;
			}
		}
		return 0;
	}
	private static partial ELearn GetLearnResultByIndex(i32 Index){
		if(Index < 0 || Index >= LearnResults.Count){
			return ELearn.Add;
		}
		return LearnResults[Index];
	}
	public partial str TranslateLearnResult(ELearn Learn){
		return Learn switch{
			ELearn.Add => I18n[K.Learn_Add],
			ELearn.Rmb => I18n[K.Learn_Rmb],
			ELearn.Fgt => I18n[K.Learn_Fgt],
			_ => Learn.ToString(),
		};
	}
}
