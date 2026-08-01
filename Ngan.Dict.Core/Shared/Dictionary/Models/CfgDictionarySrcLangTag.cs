namespace Ngaq.Core.Shared.Dictionary.Models;

/// 詞典源語言快捷標籤的本地配置項。
/// 語言身份由 Type 與 Code 穩定標識；Text 僅控制快捷欄顯示文字。
public partial class CfgDictionarySrcLangTag{
	/// 語言代碼採用的標識類型。
	public ELangIdentType Type{get;set;} = ELangIdentType.Bcp47;
	/// 語言代碼；與 Type 共同定位標準語言。
	public str Code{get;set;} = "";
	/// 可選的短標籤文字；空白時由界面回退顯示 Code。
	public str Text{get;set;} = "";
}
