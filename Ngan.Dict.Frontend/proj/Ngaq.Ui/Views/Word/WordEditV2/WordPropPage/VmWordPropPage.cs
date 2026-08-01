namespace Ngaq.Ui.Views.Word.WordEditV2.WordPropPage;

using System;
using System.Collections.ObjectModel;
using Ngaq.Core.Infra;
using Ngaq.Core.Model.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views.Word;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

/// 屬性分頁 ViewModel：管理列表、新增、轉換。
public partial class VmWordPropPage: ViewModelBase{
	public event Action<VmWordPropRow>? OnEditRequested;

	public ObservableCollection<VmWordPropRow> Rows{
		get;
		set{SetProperty(ref field, value);}
	} = [];

	/// 已標記刪除的行，保存時走 Del，成功後清空。
	public List<VmWordPropRow> RemovedRows{get;} = [];

	/// 新增的行（DmlState == Added），保存時走 BatAdd。
	public IEnumerable<VmWordPropRow> AddedRows => Rows.Where(r => r.DmlState == EDmlState.Added);

	/// 已修改的行（DmlState == Modified），保存時走 BatUpd。
	public IEnumerable<VmWordPropRow> ModifiedRows => Rows.Where(r => r.DmlState == EDmlState.Modified);

	/// 將持久化資料載入編輯用的 ViewModel 狀態。
	public partial nil LoadFromPoProps(IList<PoWordProp> Props);

	/// 建立並加入新的編輯項目。
	public partial nil AddRow();

	/// 新增行直接移除；已存在的行標記爲 Removed，保存時再刪。
	public partial nil RemoveRow(VmWordPropRow Row);

	/// 已直接調後端刪除成功的行，只從當前列表摘除，不再進 `RemovedRows` 等待總保存。
	public partial nil RemovePersistedRow(VmWordPropRow Row);

	/// 執行 RequestEdit 所代表的內部協調操作。
	public partial nil RequestEdit(VmWordPropRow Row);

	/// 保存成功後重置所有行狀態。
	public partial nil OnSaved();

	/// 驗證目前狀態並嘗試完成轉換；失敗時透過回傳值提供原因。
	public partial bool TryBuildPoProps(IdWord WordId, out List<PoWordProp> Props, out str Err);
}

/// 單詞屬性行編輯 ViewModel。
public partial class VmWordPropRow: ViewModelBase{
	/// `Descr` 只是編輯頁裏的默認/顯示別名，落庫仍統一回 `description`。
	public const str DescriptionAlias = "Descr";

	public static IReadOnlyList<EKvType> KvTypes{get;} = [
		EKvType.Str,
		EKvType.I64,
	];

	public  PoWordProp Raw{get;set;} = new();

	/// 行記錄 DML 狀態：決定保存時走 BatAdd/BatUpd/Del。
	public EDmlState DmlState{get;set;}

	public i32 KTypeIndex{
		get;
		set{
			if(SetProperty(ref field, value)){
				MarkModified();
			}
		}
	} = 0;

	public str KStrText{
		get;
		set{
			if(SetProperty(ref field, value)){
				MarkModified();
			}
		}
	} = "";

	public str KI64Text{
		get;
		set{
			if(SetProperty(ref field, value)){
				MarkModified();
			}
		}
	} = "0";

	public i32 VTypeIndex{
		get;
		set{
			if(SetProperty(ref field, value)){
				MarkModified();
			}
		}
	} = 0;

	public str VStrText{
		get;
		set{
			if(SetProperty(ref field, value)){
				MarkModified();
			}
		}
	} = "";

	public str VI64Text{
		get;
		set{
			if(SetProperty(ref field, value)){
				MarkModified();
			}
		}
	} = "0";

	/// 行記錄 Id（僅供顯示，只讀）。
	public str IdText => Raw.Id.ToString();

	public str KeyText => GetKvTypeByIndex(KTypeIndex) == EKvType.I64 ? KI64Text : KStrText;
	public str KeyDisplayText => TranslatePropKey(KeyText);
	public str ValueDisplayText => BuildValueDisplayText();

	/// 只有 Unchanged → Modified 才切換，保持 Added / Removed 不被意外覆蓋。
	partial void MarkModified();

	/// 執行 NewRow 所代表的內部協調操作。
	public static partial VmWordPropRow NewRow();

	/// 執行 FromPo 所代表的內部協調操作。
	public static partial VmWordPropRow FromPo(PoWordProp Po);

	/// 驗證目前狀態並嘗試完成轉換；失敗時透過回傳值提供原因。
	public partial bool TryToPo(IdWord WordId, out PoWordProp Po, out str Err);

	/// 根據目前狀態取得對應的顯示或轉換結果。
	private static partial i32 GetKvTypeIndex(EKvType Type);

	/// 根據目前狀態取得對應的顯示或轉換結果。
	private static partial EKvType GetKvTypeByIndex(i32 Index);

	/// 將數據庫中的內置鍵轉成更貼近編輯語境的展示文本。
	private static partial str ToEditorKeyText(str? StoredKey);

	/// 將編輯頁別名規整回內置鍵，避免破壞既有數據語義。
	private static partial str ToStoredKeyText(str? EditorKey);

	/// 內置 prop 鍵在 UI 中顯示譯文，非內置鍵保持原樣。
	public partial str TranslatePropKey(str? Key);

	/// 將內部列舉或鍵值轉換為使用者可讀的顯示文字。
	public partial str TranslateKvType(EKvType Type);

	/// 属性表只显示实际值；长文本统一走公用略缩格式。
	private partial str BuildValueDisplayText();

	/// 根據目前狀態取得對應的顯示或轉換結果。
	private partial str GetValueRawText();
}
