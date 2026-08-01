namespace Ngaq.Ui;
using Avalonia.Controls;
using System;
using Avalonia;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using Avalonia.Media;
using Avalonia.Styling;

using Ctx = object;//unused
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.AvlnTools.Dsl.PropExtn;

public partial class AppTextLogo
	:UserControl
{
	public Ctx? Ctx{
		get{return DataContext as Ctx;}
		set{DataContext = value;}
	}


	public AppTextLogo(){
		Ctx = new Ctx();
		_Style();
		_Render();
	}


	public  partial class Cls_{
		public str Logo = nameof(Logo);
	}
	public Cls_ Cls{get;set;} = new Cls_();

	protected nil _Style(){
		var cls_logo = Sty.Is<TextBlock>(x=>
			x.Class(Cls.Logo)
		);
		Styles.Add(cls_logo);
		{
			var o = cls_logo;
			o.Set(
				x=>x.FontSize
				, FontSize
			);
			o.Set(
				x=>x.FontFamily
				,new FontFamily("Times New Roman")
			);
			o.Set(
				x=>x.HorizontalAlignment
				, HAlign.Center
			);
		}
		return 0;
	}

	protected nil _Render(){
		var logo = new TextBlock{};
		Content = logo;
		{
			var o = logo;
			o.Classes.Add(Cls.Logo);
			o.Text = "ŋaʔ";
			//o.Text = "TEQVAERŌ";
			//o.Text = "VOLŌ SCĪRE";
			o.Bind(
				o.PropFontSize_()
				,this.GetObservable(FontSizeProperty)
			);
		}
		return 0;
	}


}
