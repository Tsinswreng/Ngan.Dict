namespace Ngaq.Ui.Views.Word.WordManage.NormLang.NormLangEdit;

using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Ngaq.Ui;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.Ctrls;
using Ngaq.Core.Shared.Dictionary.Models.Po.NormLang;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsI18n;
using Ctx = VmNormLangEdit;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;
using Tsinswreng.Avln.Grid;

/// NormLang 詳情編輯頁。
public partial class ViewNormLangEdit
	:AppViewBase<Ctx>
{

	public ViewNormLangEdit(){
		Ctx = App.DiOrMk<Ctx>();
		Style();
		Render();
	}

	public partial class Cls{}

	protected nil Style(){
		return NIL;
	}

	GridStack Root = new(IsRow: true);

	protected nil Render(){
		this.Content = Root.Grid;
		Root.SetRowDefs([
			new(1, GUT.Star),
			new(1, GUT.Auto),
		]);
		Root.A(MkBody());
		Root.A(MkBottomBar());
		return NIL;
	}

	Control MkBody(){
		var sv = new ScrollViewer();
		var root = new StackPanel{
			Spacing = 10,
			Margin = new(10),
		};
		sv.Content = root;
		root.Children.Add(MkErrorBar());
		root.Children.Add(MkPoSection());
		return sv;
	}

	Control MkErrorBar(){
		var b = new Border{
			Background = new SolidColorBrush(Color.FromArgb(80, 180, 30, 30)),
			Padding = new(10, 6),
			IsVisible = false,
		};
		b.CBind<Ctx>(IsVisibleProperty, x=>x.HasError, Mode: BindingMode.OneWay);
		var txt = new TextBlock{
			Foreground = Brushes.White,
		};
		txt.CBind<Ctx>(TextBlock.TextProperty, x=>x.LastError, Mode: BindingMode.OneWay);
		b.Child = txt;
		return b;
	}

	Control MkPoSection(){
		var bdr = new Border{
			BorderBrush = Brushes.DimGray,
			BorderThickness = new(1),
			Padding = new(10),
		};
		var sp = new StackPanel{Spacing = 8};
		bdr.Child = sp;

		sp.A(new TextBlock{
			Text = I[K.PoNormLang],
			FontSize = UiCfg.Inst.BaseFontSize * 1.1,
			FontWeight = FontWeight.SemiBold,
		})
		.A(MkIdRow(I[K.Id], CBE.Mk<Ctx>(x=>x.PoIdText, Mode: BindingMode.OneWay)))
		.A(MkInputRow(I[K.Code], CBE.Mk<Ctx>(x=>x.PoCode, Mode: BindingMode.TwoWay)))
		.A(MkInputRow(I[K.NativeName], CBE.Mk<Ctx>(x=>x.PoNativeName, Mode: BindingMode.TwoWay)))
		.A(MkInputRow(I[K.EnglishName], CBE.Mk<Ctx>(x=>x.PoEnglishName, Mode: BindingMode.TwoWay)))
		.A(MkInputRow(I[K.Weight], CBE.Mk<Ctx>(x=>x.PoWeightText, Mode: BindingMode.TwoWay)));
		var typeRow = MkComboRow(I[K.Type], Ctx?.TypeOptions ?? [], CBE.Mk<Ctx>(x=>x.PoTypeIndex, Mode: BindingMode.TwoWay));
		typeRow.CBind<Ctx>(IsVisibleProperty, x=>x.ShowTypeField, Mode: BindingMode.OneWay);
		sp.A(typeRow);
		return bdr;
	}

	Control MkBottomBar(){
		var bar = new GridStack(IsRow:false);
		bar.Grid.SetColDefs([
			new(1, GUT.Star),
			new(1, GUT.Star),
		]);
		bar.A(new OpBtn(), o=>{
			o._Button.Background = UiCfg.Inst.DelBtnBg;
			o._Button.HorizontalContentAlignment = HAlign.Center;
			o.BtnContent = Icons.Delete().ToIcon().WithText(I[K.Delete]);
			o.SetExe((Ct)=>Ctx?.Delete(Ct));
		})
		.A(new OpBtn(), o=>{
			o._Button.Background = UiCfg.Inst.MainColor;
			o._Button.HorizontalContentAlignment = HAlign.Center;
			o.BtnContent = Icons.Save().ToIcon().WithText(I[K.Save]);
			o.SetExe((Ct)=>Ctx?.Save(Ct));
		});
		return bar.Grid;
	}

	Control MkInputRow(str Label, IBinding Binding, bool ReadOnly = false, bool AcceptsReturn = false){
		var sp = new StackPanel{Spacing = 3};
		sp.Children.Add(new TextBlock{Text = Label});
		var tb = new TextBox{
			IsReadOnly = ReadOnly,
			AcceptsReturn = AcceptsReturn,
			TextWrapping = AcceptsReturn ? TextWrapping.Wrap : TextWrapping.NoWrap,
			MaxHeight = AcceptsReturn ? 180 : double.PositiveInfinity,
		};
		tb.Bind(TextBox.TextProperty, Binding);
		sp.Children.Add(tb);
		return sp;
	}

	Control MkComboRow(str Label, IEnumerable<str> Items, IBinding Binding){
		var sp = new StackPanel{Spacing = 3};
		sp.Children.Add(new TextBlock{Text = Label});
		var cb = new ComboBox();
		foreach(var item in Items){
			cb.Items.Add(item);
		}
		cb.Bind(ComboBox.SelectedIndexProperty, Binding);
		sp.Children.Add(cb);
		return sp;
	}

	Control MkIdRow(str Label, IBinding Binding){
		var row = new StackPanel{
			Spacing = 6,
			Orientation = Orientation.Horizontal,
		};
		row.Children.Add(new TextBlock{
			Text = Label + ":",
			FontSize = UiCfg.Inst.BaseFontSize * 0.8,
		});
		var value = new TextBlock{
			FontSize = UiCfg.Inst.BaseFontSize * 0.8,
		};
		value.Bind(TextBlock.TextProperty, Binding);
		row.Children.Add(value);
		return row;
	}
}


