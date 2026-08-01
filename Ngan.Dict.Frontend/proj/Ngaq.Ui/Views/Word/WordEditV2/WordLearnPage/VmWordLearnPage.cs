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
	public event Action<VmWordLearnRow>? OnEditRequested;

	public ObservableCollection<VmWordLearnRow> Rows{
		get;
		set{SetProperty(ref field, value);}
	} = [];

	/// 已標記刪除的行，保存時走 Del，成功後清空。
	public List<VmWordLearnRow> RemovedRows{get;} = [];

	/// 新增的行（DmlState == Added），保存時走 BatAdd。
	public IEnumerable<VmWordLearnRow> AddedRows => Rows.Where(r => r.DmlState == EDmlState.Added);

	/// 已修改的行（DmlState == Modified），保存時走 BatUpd。
	public IEnumerable<VmWordLearnRow> ModifiedRows => Rows.Where(r => r.DmlState == EDmlState.Modified);

	/// 將持久化資料載入編輯用的 ViewModel 狀態。
	public partial nil LoadFromPoLearns(IList<PoWordLearn> Learns);

	/// 建立並加入新的編輯項目。
	public partial nil AddRow();

	/// 新增行直接移除；已存在的行標記爲 Removed，保存時再刪。
	public partial nil RemoveRow(VmWordLearnRow Row);

	/// 已直接調後端刪除成功的行，只更新本地列表，不再掛到總保存刪除隊列。
	public partial nil RemovePersistedRow(VmWordLearnRow Row);

	/// 執行 RequestEdit 所代表的內部協調操作。
	public partial nil RequestEdit(VmWordLearnRow Row);

	/// 保存成功後重置所有行狀態。
	public partial nil OnSaved();

	/// 驗證目前狀態並嘗試完成轉換；失敗時透過回傳值提供原因。
	public partial bool TryBuildPoLearns(IdWord WordId, out List<PoWordLearn> Learns, out str Err);
}

/// 學習記錄行 ViewModel。
public partial class VmWordLearnRow: ViewModelBase{
	public static IReadOnlyList<ELearn> LearnResults{get;} = [
		ELearn.Add,
		ELearn.Rmb,
		ELearn.Fgt,
	];

	public  PoWordLearn Raw{get;set;} = new();

	/// 行記錄 DML 狀態：決定保存時走 BatAdd/BatUpd/Del。
	public EDmlState DmlState{get;set;}

	public i32 LearnResultIndex{
		get;
		set{
			if(SetProperty(ref field, value)){
				MarkModified();
			}
		}
	} = 0;

	/// 行記錄 Id（僅供顯示，只讀）。
	public str IdText => Raw.Id.ToString();

	public str BizCreatedAtIso{
		get;
		set{
			if(SetProperty(ref field, value)){
				MarkModified();
			}
		}
	} = UnixMs.Now().ToIso();

	public str BizCreatedAtDisplay{
		get{
			try{
				var t = UnixMs.FromIso(BizCreatedAtIso);
				var dto = DateTimeOffset.FromUnixTimeMilliseconds(t.Value);
				var local = TimeZoneInfo.ConvertTime(dto, TimeZoneInfo.Local);
				return local.ToString("yy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
			}catch{
				return BizCreatedAtIso;
			}
		}
	}

	public str LearnResultText => GetLearnResultByIndex(LearnResultIndex).ToString();
	public str LearnResultDisplayText => TranslateLearnResult(GetLearnResultByIndex(LearnResultIndex));

	/// 只有 Unchanged → Modified 才切換，保持 Added / Removed 不被意外覆蓋。
	partial void MarkModified();

	/// 執行 NewRow 所代表的內部協調操作。
	public static partial VmWordLearnRow NewRow();

	/// 執行 FromPo 所代表的內部協調操作。
	public static partial VmWordLearnRow FromPo(PoWordLearn Po);

	/// 驗證目前狀態並嘗試完成轉換；失敗時透過回傳值提供原因。
	public partial bool TryToPo(IdWord WordId, out PoWordLearn Po, out str Err);

	/// 根據目前狀態取得對應的顯示或轉換結果。
	private static partial i32 GetLearnResultIndex(ELearn Learn);

	/// 根據目前狀態取得對應的顯示或轉換結果。
	private static partial ELearn GetLearnResultByIndex(i32 Index);

	/// 將內部列舉或鍵值轉換為使用者可讀的顯示文字。
	public partial str TranslateLearnResult(ELearn Learn);
}
