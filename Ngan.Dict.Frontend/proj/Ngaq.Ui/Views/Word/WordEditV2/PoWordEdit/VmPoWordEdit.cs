namespace Ngaq.Ui.Views.Word.WordEditV2.PoWordEdit;

using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Ui.Infra;

/// 編輯 PoWord 核心字段（含 Biz 時間與軟刪時間）。
public partial class VmPoWordEdit: ViewModelBase{
	/// 供只讀欄位顯示的單詞識別碼。
	public str WordIdText{
		get;
		set{SetProperty(ref field, value);}
	} = "";
	/// 單詞拼寫，保存前不可為空。
	public str Head{
		get;
		set{SetProperty(ref field, value);}
	} = "";
	/// 單詞語言，保存前不可為空。
	public str Lang{
		get;
		set{SetProperty(ref field, value);}
	} = "";
	public str StoredAtIso{
		get;
		set{SetProperty(ref field, value);}
	} = "";
	public str DelAtUnixMs{
		get;
		set{SetProperty(ref field, value);}
	} = "";
	public str BizCreatedAtIso{
		get;
		set{SetProperty(ref field, value);}
	} = "";
	public str BizUpdatedAtIso{
		get;
		set{SetProperty(ref field, value);}
	} = "";
	/// 將持久化單詞映射到可綁定編輯字段。
	public partial nil LoadFromPo(PoWord Po);
	/// 驗證並將編輯字段回寫持久化單詞。
	public partial bool TryApplyToPo(PoWord Po, out str Err);
}
