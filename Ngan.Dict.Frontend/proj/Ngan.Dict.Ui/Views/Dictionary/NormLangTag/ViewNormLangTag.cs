namespace Ngan.Dict.Ui.Views.Dictionary.NormLangTag;

using Avalonia.Controls;
using Ngan.Dict.Ui.Infra;
using Tsinswreng.CsCore;

/// 按內容最小寬度顯示的詞典源語言快捷標籤。
/// 標籤是一個按鈕：文字是語言的短名，選中態由樣式類 `Cls.Selected` 表達。
public partial class ViewNormLangTag : AppViewBase<VmNormLangTag>{

	/// View 只能有無參構造器。
	public partial ViewNormLangTag();

	/// 樣式類名常量。禁止在代碼裏硬編碼類名字串。
	public partial class Cls{
		/// 當前源語言被選中時加到標籤按鈕上的類名。
		public const str Selected = nameof(Selected);
	}

	/// 用戶點擊 Tag 時觸發；由詞典頁決定如何切換源語言。
	public event Action<VmNormLangTag>? OnSelected;

	/// 承載短標籤文字與點擊行為的按鈕。
	public Button? _TagButton;

	/// 建立控件樹。
	protected override partial object Build(VmNormLangTag vm);

	/// 建立本視圖的樣式：選中態的底色。
	protected override partial StyleGroup? BuildStyles();

	/// 點擊時通知外部。
	protected partial void Select();
}
