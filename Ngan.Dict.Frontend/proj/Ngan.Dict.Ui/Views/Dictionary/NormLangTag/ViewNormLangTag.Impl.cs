namespace Ngan.Dict.Ui.Views.Dictionary.NormLangTag;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using Avalonia.Styling;
using Ngan.Dict.Ui.Infra;
using Tsinswreng.CsCore;

/// 詞典源語言快捷標籤 View 的實現。
public partial class ViewNormLangTag{

	/// View 只能有無參構造器；Vm 在這裏解析。
	public partial ViewNormLangTag(){
		vm = App.DiOrMk<VmNormLangTag>();
		Margin = new Thickness(0, 0, 4, 0);
	}

	/// 建立標籤按鈕。選中態用樣式類表達，Vm 不必知道任何顏色。
	protected override partial object Build(VmNormLangTag vm){
		var Btn = new Button()
			.HorizontalAlignment(HAlign.Left)
			.HorizontalContentAlignment(HAlign.Center)
			.Padding(new Thickness(8, 2))
			.MinWidth(0)
			.Content(vm, x => x.DisplayText)
			.BindClass(vm, x => x.IsSelected, Cls.Selected)
		;
		_TagButton = Btn;
		Btn.Click += (_, _) => Select();
		return Btn;
	}

	/// 選中態的底色。樣式只掛在本視圖的按鈕上，故選擇器同時限定型別與類名。
	protected override partial StyleGroup? BuildStyles(){
		return [
			new Style<Button>(Sel => Sel.Class(Cls.Selected))
				.Background(UiCfg.Inst.MainColor),
		];
	}

	/// 點擊時通知外部。
	protected partial void Select(){
		OnSelected?.Invoke(vm);
	}
}
