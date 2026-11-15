// 從 Tsinswreng.AvlnTools 原樣搬入本專案（0.3 系）：屬性選擇器與常用樣式糖。
// 該庫尚未遷入 TswgLib；本項目用它寫界面，故只搬這幾個純糖檔案，行為不變。
// 全域以 `global using static ...ExtnProp;` 使用，故調用點寫法照舊（txt.PropText 等）。

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using Tsinswreng.Avln.Dsl;

namespace Ngan.Dict.Ui.Infra.AvlnSugar;

public static class MkrStyle{

	public static Style MkStyForAnyControl(){
		var Ans = new Style(x=>
			x.Is<Control>()
		);
		return Ans;
	}

	public static Style GridShowLines(
		this Style z
	){
		z.Set(
			Grid.ShowGridLinesProperty
			,true
		);
		return z;
	}

	public static Style GridShowLines(
		//this Avalonia.StyledElement.Styles z
	){
		var Ans = new Style(x=>
			x.Is<Grid>()
		);
		Ans.GridShowLines();
		return Ans;
	}



	public static Style NoCornerRadius(
		this Style z
	){
		z.Set(
			TemplatedControl.CornerRadiusProperty
			, new CornerRadius(0)
		);
		return z;
	}

	public static Style NoPadding(
		this Style z
	){
		z.Set(
			TemplatedControl.PaddingProperty
			,new Thickness(0)
		);
		return z;
	}

	public static Style NoMargin(
		this Style z
	){
		z.Set(
			global::Avalonia.Layout.Layoutable.MarginProperty
			, new Thickness(0)
		);
		return z;
	}

	public static Style BgTrnsp(
		this Style z
	){
		z.Set(TemplatedControl.BackgroundProperty, Brushes.Transparent);
		return z;
	}


}
