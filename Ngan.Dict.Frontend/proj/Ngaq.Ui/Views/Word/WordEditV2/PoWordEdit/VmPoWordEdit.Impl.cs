namespace Ngaq.Ui.Views.Word.WordEditV2.PoWordEdit;

using Ngaq.Core.Infra;
using Ngaq.Core.Shared.User.Models.Po;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Tools;
using Ngaq.Ui.Infra;
using Tsinswreng.CsTempus;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

public partial class VmPoWordEdit{
	/// 保留原始時間字串語義，讓表單可直接進行雙向繫結。
	public partial nil LoadFromPo(PoWord Po){
		WordIdText = Po.Id.ToString(); Head = Po.Head; Lang = Po.Lang; StoredAtIso = Po.StoredAt.ToIso();
		DelAtUnixMs = Po.DelAt.IsNullOrDefault() ? "" : (Po.DelAt.Value+"");
		BizCreatedAtIso = Po.BizCreatedAt.ToIso(); BizUpdatedAtIso = Po.BizUpdatedAt.ToIso();
		return NIL;
	}
	/// 依欄位順序驗證，第一個失敗原因回傳給頁面錯誤列。
	public partial bool TryApplyToPo(PoWord Po, out str Err){
		Err = "";
		if(str.IsNullOrWhiteSpace(Head)){Err = I18n[K.HeadIsRequired]; return false;}
		if(str.IsNullOrWhiteSpace(Lang)){Err = I18n[K.LangIsRequired]; return false;}
		Po.Head = Head.Trim(); Po.Lang = Lang.Trim();
		try{Po.StoredAt = UnixMs.FromIso(StoredAtIso.Trim());}catch{Err = I18n[K.StoredAtMustBeIsoTime]; return false;}
		if(str.IsNullOrWhiteSpace(DelAtUnixMs)){Po.DelAt = default;}else if(!i64.TryParse(DelAtUnixMs.Trim(), out var DelMs)){Err = I18n[K.DelAtMustBeUnixMilliseconds]; return false;}else{Po.DelAt = IdDel.FromUnixMs(DelMs);}
		try{Po.BizCreatedAt = UnixMs.FromIso(BizCreatedAtIso.Trim());}catch{Err = I18n[K.BizCreatedAtMustBeIsoTime]; return false;}
		try{Po.BizUpdatedAt = UnixMs.FromIso(BizUpdatedAtIso.Trim());}catch{Err = I18n[K.BizUpdatedAtMustBeIsoTime]; return false;}
		return true;
	}
}
