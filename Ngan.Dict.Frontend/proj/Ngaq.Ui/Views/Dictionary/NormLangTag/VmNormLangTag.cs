namespace Ngaq.Ui.Views.Dictionary.NormLangTag;

using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.Dictionary.Models.Po.NormLang;
using Ngaq.Ui.Infra;
using Ctx = VmNormLangTag;

/// 詞典源語言快捷標籤的運行時狀態。
/// 將持久化配置、最新標準語言資料與當前選中狀態聚合給 View 使用。
public partial class VmNormLangTag: ViewModelBase, IMk<Ctx>{
	/// 建立不注入服務的標籤 ViewModel。
	protected partial VmNormLangTag();
	/// 建立空白標籤 ViewModel。
	public static partial Ctx Mk();

	/// 語言代碼採用的標識類型。
	public ELangIdentType Type{
		get;
		set{SetProperty(ref field, value);}
	} = ELangIdentType.Bcp47;
	/// 語言代碼；同時作為自定義文字空白時的後備顯示內容。
	public str Code{
		get;
		set{
			if(SetProperty(ref field, value)){
				OnPropertyChanged(nameof(DisplayText));
			}
		}
	} = "";
	/// 用戶為快捷欄設定的短文字；空白表示直接顯示 Code。
	public str Text{
		get;
		set{
			if(SetProperty(ref field, value)){
				OnPropertyChanged(nameof(DisplayText));
			}
		}
	} = "";
	/// 標準語言的本地名稱，供編輯頁辨識語言，不強制顯示在快捷 Tag 上。
	public str NativeName{
		get;
		set{SetProperty(ref field, value);}
	} = "";
	/// 當前 Tag 是否代表詞典正在使用的源語言。
	public bool IsSelected{
		get;
		set{SetProperty(ref field, value);}
	} = false;
	/// Tag 最終顯示文字；自定義文字空白時回退到語言代碼。
	public str DisplayText{
		get{return str.IsNullOrWhiteSpace(Text) ? Code : Text;}
	}

	/// 從配置項與可選的最新標準語言資料初始化運行時狀態。
	public partial nil FromCfg(CfgDictionarySrcLangTag Cfg, PoNormLang? Po = null);
	/// 導出可寫入本地配置文件的最小資料。
	public partial CfgDictionarySrcLangTag ToCfg();
	/// 判斷此 Tag 是否標識指定語言。
	public partial bool IsLang(ELangIdentType Type, str Code);
}
