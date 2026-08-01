using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Styling;
using AvaloniaEdit.Utils;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;

namespace Ngaq.Ui;

public partial class App{
	public class Cls{
		public const str SpacedStackPanel = nameof(App)+"_"+nameof(SpacedStackPanel);
		public const str ViewPadding = nameof(App)+"_"+nameof(ViewPadding);
		public const str CenterBtn = nameof(App)+"_"+nameof(CenterBtn);
		public const str RoTextBox = nameof(App)+"_"+nameof(RoTextBox);
	}

	Styles RoTextBox(Styles S){
		S.A(
			Sty.Is<TextBox>(
				x=>x.Class(Cls.RoTextBox)
			).Set(
				x=>x.IsReadOnly, true
			).Set(
				x=>x.AcceptsReturn, true
			).Set(
				x=>x.TextWrapping, TextWrapping.Wrap
			).Set(
				x=>x.Focusable, false
			).Set(
				x=>x.IsTabStop, false
			)
		);
		return S;
	}

	Styles CenterBtn(Styles S){
		S.A(
			Sty.Is<Button>(
				x=>x.Class(Cls.CenterBtn)
			).Set(
				x=>x.HorizontalAlignment, HAlign.Center
			).Set(
				x=>x.VerticalAlignment, VAlign.Center
			).Set(
				x=>x.VerticalContentAlignment, VAlign.Center
			).Set(
				x=>x.HorizontalContentAlignment, HAlign.Center
			)
		);
		return S;
	}

	Styles SpacedStackPanel(Styles S){
		S.A(
			Sty.Is<StackPanel>(
				x=>x.Class(Cls.SpacedStackPanel)
			).Set(
				x=>x.Spacing, UiCfg.Inst.BaseFontSize*0.5
			)
		)
		;
		return S;
	}

	Styles ViewPadding(Styles S){
		S.A(
			Sty.Is<ContentControl>(
				x=>x.Class(Cls.ViewPadding)
			).Set(
				x=>x.Padding, new Thickness(UiCfg.Inst.BaseFontSize)
			)
		);
		return S;
	}

	protected nil _Style(){
		var S = Styles;
		SpacedStackPanel(S);
		ViewPadding(S);
		CenterBtn(S);
		RoTextBox(S);

		// 如下設置後 在局部覆蓋全局字體旹 TextBlock生效洏TextBox不效、不效者 字體大小恆不變
		// Styles.A(
		// 	new Style(x=>
		// 		x.Is<Control>()
		// 	).Set(
		// 		TextElement.FontSizeProperty
		// 		,UiCfg.Inst.BaseFontSize
		// 	)
		// );

		Styles.A(
			Sty.Is<Control>()//必須匹配Control。匹配 TextElement 不效、于安卓端漢字猶爲方塊
			.Set(
				TextElement.FontFamilyProperty
				//,new FontFamily("Times New Roman, STSong")
				//,new FontFamily("Times New Roman")
				,FontFamily.Default//不顯式指定Default則珩于android恐缺漢字字體
			)
		);
		//按鈕舒展

		Styles.A(
			Sty.Is<Button>()
			.Set(
				x=>x.HorizontalAlignment
				, HAlign.Stretch
			).Set(
				x=>x.Background
				,new SolidColorBrush(Color.FromArgb(255, 32,32,32))
			)
		)
		.A(
			Sty.Is<Button>(x=>
				x.Class(PsdCls.pointerover)
				.Template().OfType<ContentPresenter>()
			).Set(
				x=>x.BorderBrush
				,UiCfg.Inst.MainColor
			)
		)

		// TreeDataGrid 統一撐滿父容器；各頁列寬約定為「前列 Auto，最後一列 1 Star」以消除表頭右側空白。
		.A(
			Sty.Is<TreeDataGrid>()
			.Set(
				x=>x.HorizontalAlignment
				,HAlign.Stretch
			).Set(
				x=>x.VerticalAlignment
				,VAlign.Stretch
			)
		);

		/*
我想把按鈕的邊框顏色綁定到和他自己的背景顏色一樣、並把這當成一種優先級最低的默認行爲
如果 在 局部 顯示指定了按鈕的邊框 再不再使用默認行爲。
		 */
		TemplatedControl.BackgroundProperty.Changed.AddClassHandler<Button>((btn, e) => {
			btn.SetValue(
				TemplatedControl.BorderBrushProperty
				,e.NewValue as IBrush
				,BindingPriority.Style
			);
		});

		return NIL;
	}
}
