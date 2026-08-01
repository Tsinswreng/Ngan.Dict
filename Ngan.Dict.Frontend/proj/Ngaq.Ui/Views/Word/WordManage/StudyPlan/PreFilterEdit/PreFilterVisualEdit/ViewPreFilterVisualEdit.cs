namespace Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.PreFilterVisualEdit;

using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Ngaq.Ui;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.Ctrls;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;

using Ctx = VmPreFilterVisualEdit;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

/// PreFilter GUI 主頁。
/// 顯示 Po 主信息與內嵌的篩選器編輯表格。
public class ViewPreFilterVisualEdit: AppViewBase<Ctx>, I_MkTitleMenu{

	public ViewPreFilterVisualEdit(){
		Ctx = App.DiOrMk<Ctx>();
		Render();
		InitVisualGridSource();
	}

	GridStack Root = new(IsRow: true);
	TreeDataGrid? CoreGrid;
	TreeDataGrid? PropGrid;
	FlatTreeDataGridSource<Ctx.RowFieldsFilterCard>? CoreGridSource;
	FlatTreeDataGridSource<Ctx.RowFieldsFilterCard>? PropGridSource;
	protected nil Render(){
		Content = Root.Grid;
		Root.SetRowDefs([
			new(1, GUT.Star),
			new(1, GUT.Auto),
		]);
		Root.A(MkBody());
		Root.A(MkBottomBar());
		return NIL;
	}

	Control MkBody(){
		var root = new GridStack(IsRow:true);
		root.Grid.SetRowDefs([
			new(1, GUT.Auto),
			new(1, GUT.Auto),
			new(1, GUT.Star),
		]);
		root.A(MkErrorBar());
		root.A(MkPoSection(), o=>o.Margin = new(10, 10, 10, 8));
		root.A(MkIntegratedDataEditor(), o=>o.Margin = new(10, 0, 10, 10));
		return root.Grid;
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

		sp
		// .A(new TextBlock{
		// 	Text = I[K.PoPreFilter],
		// 	FontSize = UiCfg.Inst.BaseFontSize * 1.1,
		// 	FontWeight = FontWeight.SemiBold,
		// })
		.A(MkIdRow(I[K.Id], CBE.Mk<Ctx>(x=>x.PoIdText, Mode: BindingMode.OneWay)))
		.A(MkInputRow(I[K.Name], CBE.Mk<Ctx>(x=>x.PoUniqName, Mode: BindingMode.TwoWay)))
		.A(MkInputRow(I[K.Description], CBE.Mk<Ctx>(x=>x.PoDescr, Mode: BindingMode.TwoWay), AcceptsReturn: true))
		;
		var typeRow = MkComboRow(I[K.Type], Ctx?.PoTypeOptions ?? [], CBE.Mk<Ctx>(x=>x.PoTypeIndex, Mode: BindingMode.TwoWay));
		typeRow.CBind<Ctx>(IsVisibleProperty, x=>x.ShowPoTypeField, Mode: BindingMode.OneWay);
		sp.A(typeRow);
		return bdr;
	}

	void InitVisualGridSource(){
		if(Ctx is null){
			return;
		}
		if(CoreGrid is not null){
			CoreGridSource = MkGridSource(Ctx.CoreFilterCards);
			CoreGrid.Source = CoreGridSource;
		}
		if(PropGrid is not null){
			PropGridSource = MkGridSource(Ctx.PropFilterCards);
			PropGrid.Source = PropGridSource;
		}
	}

	FlatTreeDataGridSource<Ctx.RowFieldsFilterCard> MkGridSource(IList<Ctx.RowFieldsFilterCard> rows){
		return new FlatTreeDataGridSource<Ctx.RowFieldsFilterCard>(rows){
			Columns = {
				new TextColumn<Ctx.RowFieldsFilterCard, str>("", x=>x.UiIdxText, width: new GridLength(1, GUT.Auto)),
				new TextColumn<Ctx.RowFieldsFilterCard, str>(I[K.Items], x=>x.FilterCountText, width: new GridLength(1, GUT.Auto)),
				new TextColumn<Ctx.RowFieldsFilterCard, str>(I[K.TextPreview], x=>x.ContentPreview, width: new GridLength(1, GUT.Star)),
			},
		};
	}

	Control MkIntegratedDataEditor(){
		if(Ctx is null){
			return new TextBlock{Text = I[K.EditorNotReady]};
		}
		var tabs = new TabControl();
		tabs.Items.Add(new TabItem{
			Header = I[K.CoreFilter],
			Content = MkFilterTab(true),
		});
		tabs.Items.Add(new TabItem{
			Header = I[K.PropFilter],
			Content = MkFilterTab(false),
		});
		return tabs;
	}

	Control MkFilterTab(bool isCore){
		var root = new GridStack(IsRow:true);
		root.Grid.SetRowDefs([
			new(1, GUT.Auto),
			new(1, GUT.Star),
		]);

		var top = new GridStack(IsRow:false);
		top.Grid.SetColDefs([
			new(1, GUT.Star),
			new(1, GUT.Auto),
		]);
		top.A(new TextBlock(), o=>{
			o.Text = "";
		});
		top.A(new Button(), o=>{
			o.Content = Icons.Add().ToIcon().WithText(I[K.AddGroup]);
			o.HorizontalContentAlignment = HAlign.Center;
			o.Click += (s,e)=>{
				if(isCore){
					Ctx?.AddCoreGroup();
				}else{
					Ctx?.AddPropGroup();
				}
			};
		});
		root.A(top.Grid);

		var grid = new TreeDataGrid{
			MinHeight = 220,
			HorizontalAlignment = HAlign.Stretch,
		};
		grid.Styles.Add(
			Sty.OfType<TreeDataGridRow>(x=>x.Class(":pointerover"))
			.Set(x=>x.Background, new SolidColorBrush(Color.FromRgb(46, 46, 46)))
		);
		grid.Styles.Add(
			Sty.OfType<TreeDataGridRow>(x=>x.Class(":pressed"))
			.Set(x=>x.Background, new SolidColorBrush(Color.FromRgb(70, 70, 70)))
		);
		if(isCore){
			CoreGrid = grid;
			grid.AddHandler(InputElement.TappedEvent, OnCoreGridTapped, RoutingStrategies.Bubble, true);
		}else{
			PropGrid = grid;
			grid.AddHandler(InputElement.TappedEvent, OnPropGridTapped, RoutingStrategies.Bubble, true);
		}
		root.A(grid);
		return root.Grid;
	}

	void OnCoreGridTapped(object? sender, TappedEventArgs e){
		OnFieldsGridTapped(e, true);
	}

	void OnPropGridTapped(object? sender, TappedEventArgs e){
		OnFieldsGridTapped(e, false);
	}

	void OnFieldsGridTapped(TappedEventArgs e, bool isCore){
		if(Ctx is null){
			return;
		}
		if(e.Source is not StyledElement src){
			return;
		}
		for(StyledElement? cur = src; cur is not null; cur = cur.Parent){
			if(cur is ToggleButton){
				return;
			}
			if(cur is TreeDataGridRow row){
				if(row.DataContext is Ctx.RowFieldsFilterCard vmRow){
					if(isCore){
						Ctx.OpenCoreFilterCard(vmRow);
					}else{
						Ctx.OpenPropFilterCard(vmRow);
					}
					e.Handled = true;
				}
				return;
			}
		}
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
			MaxHeight = AcceptsReturn ? 140 : double.PositiveInfinity,
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
		var value = new SelectableTextBlock{
			FontSize = UiCfg.Inst.BaseFontSize * 0.8,
		};
		value.Bind(TextBlock.TextProperty, Binding);
		row.Children.Add(value);
		return row;
	}
	public Control MkTitleMenu(){
		var menu = new ContextMenu();
		menu.Items.A(new MenuItem(), o=>{
			o.Header = I[K.OpenJson];
			o.Click += (s,e)=>Ctx?.OpenPayloadJsonEditor();
		});
		return menu;
	}
}


