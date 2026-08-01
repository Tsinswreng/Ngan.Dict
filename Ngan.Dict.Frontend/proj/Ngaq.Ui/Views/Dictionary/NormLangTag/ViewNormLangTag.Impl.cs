namespace Ngaq.Ui.Views.Dictionary.NormLangTag;

using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Ngaq.Ui.Infra;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Ctx = VmNormLangTag;

/// 詞典源語言快捷標籤 View 的函數實現。
public partial class ViewNormLangTag{
	public partial ViewNormLangTag(){
		Ctx = App.DiOrMk<Ctx>();
		InitStyle();
		Render();
	}

	private partial void Render(){
		this.SetContent(new Button(), O=>{
			TagButton = O;
			O.HorizontalAlignment = HAlign.Left;
			O.HorizontalContentAlignment = HAlign.Center;
			O.Padding = new(8, 2);
			O.MinWidth = 0;
			O.CBind<Ctx>(O.PropContent, X=>X.DisplayText);
			O.CBind<Ctx>(
				O.PropBackground,
				X=>X.IsSelected,
				Converter: new FnConvtr<bool, IBrush?>(X=>{
					return X ? UiCfg.Inst.MainColor : Brushes.Transparent;
				})
			);
			O.Click += (_, _)=>Select();
		});
	}

	private partial void InitStyle(){
		Margin = new(0, 0, 4, 0);
	}

	private partial void Select(){
		if(Ctx is not null){
			OnSelected?.Invoke(Ctx);
		}
	}
}
