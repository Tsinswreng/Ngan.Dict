namespace Ngaq.Ui.Views.Word.WordManage.NormLangToUserLang.NormLangToUserLangEdit;

using System;
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
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views.Word.WordManage.NormLang.NormLangPage;
using Ngaq.Ui.Views.Word.WordManage.UserLang.UserLangPage;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsI18n;
using Ctx = VmNormLangToUserLangEdit;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

/// 標準語言到用戶語言映射詳情編輯頁。
public partial class ViewNormLangToUserLangEdit
	:AppViewBase<Ctx>
{

	public ViewNormLangToUserLangEdit(){
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
		])
		.A(MkBody())
		.A(MkBottomBar());
		return NIL;
	}

	Control MkBody(){
		var sv = new ScrollViewer();
		sv.SetContent(new StackPanel{
			Spacing = 10,
			Margin = new(10),
		}, o=>{
			o.Children.Add(MkErrorBar());
			o.Children.Add(MkPoSection());
		});
		return sv;
	}

	Control MkErrorBar(){
		var b = new Border{
			Background = new SolidColorBrush(Color.FromArgb(80, 180, 30, 30)),
			Padding = new(10, 6),
			IsVisible = false,
		};
		b.SetChild(new TextBlock{
			Foreground = Brushes.White,
		});
		//b.CBind<Ctx>(IsVisibleProperty, x=>x.HasError, Mode: BindingMode.OneWay);

		//txt.CBind<Ctx>(TextBlock.TextProperty, x=>x.LastError, Mode: BindingMode.OneWay);

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
			Text = I[K.PoNormLangToUserLang],
			FontSize = UiCfg.Inst.BaseFontSize * 1.1,
			FontWeight = FontWeight.SemiBold,
		})
		.A(MkIdRow(I[K.Id], CBE.Mk<Ctx>(x=>x.PoIdText, Mode: BindingMode.OneWay)));
		var typeRow = MkComboRow(I[K.NormLangType], Ctx?.NormLangTypeOptions ?? [], CBE.Mk<Ctx>(x=>x.PoNormLangTypeIndex, Mode: BindingMode.TwoWay));
		typeRow.CBind<Ctx>(IsVisibleProperty, x=>x.ShowNormLangTypeField, Mode: BindingMode.OneWay);
		sp.A(typeRow)
			.A(MkPickerRow(I[K.NormLang], CBE.Mk<Ctx>(x=>x.PoNormLang, Mode: BindingMode.TwoWay), ()=>OpenNormLangSelector()))
			.A(MkPickerRow(I[K.UserLang], CBE.Mk<Ctx>(x=>x.PoUserLang, Mode: BindingMode.TwoWay), ()=>OpenUserLangSelector()))
			.A(MkInputRow(I[K.Descr], CBE.Mk<Ctx>(x=>x.PoDescr, Mode: BindingMode.TwoWay), AcceptsReturn: true));
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
		sp.A(tb);
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

	Control MkPickerRow(str Label, IBinding Binding, Action OnPick){
		var root = new StackPanel{Spacing = 3};
		root.A(new TextBlock{Text = Label});
		var row = new GridStack(IsRow:false);
		root.A(row.Grid, row=>{
			row.SetColDefs([
				new(8, GUT.Star),
				new(2, GUT.Star),
			]);
			row.A(new TextBox(), tb=>{
				tb.Bind(TextBox.TextProperty, Binding);
			});
		})
		.A(new Button(), o=>{
			o.Content = Icons.ListSelect().ToIcon().WithText(I[K.Pick]);
			o.Click += (s,e)=>OnPick();
		});

		return root;
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

	void OpenNormLangSelector(){
		var view = new ViewNormLangPage();
		if(view.Ctx is not null){
			view.Ctx.Input = Ctx?.PoNormLang ?? "";
			view.Ctx.SetSelectMode(po=>{
				Ctx?.ApplyNormLangSelection(po);
				view.ViewNavi?.Back();
			});
		}
		ViewNavi?.GoTo(ToolView.WithTitle(I[K.SelectNormLang], view));
	}

	void OpenUserLangSelector(){
		var view = new ViewUserLangPage();
		if(view.Ctx is not null){
			view.Ctx.Input = Ctx?.PoUserLang ?? "";
			view.Ctx.SetSelectMode(po=>{
				Ctx?.ApplyUserLangSelection(po);
				view.ViewNavi?.Back();
			});
		}
		ViewNavi?.GoTo(ToolView.WithTitle(I[K.SelectUserLang], view));
	}
}



