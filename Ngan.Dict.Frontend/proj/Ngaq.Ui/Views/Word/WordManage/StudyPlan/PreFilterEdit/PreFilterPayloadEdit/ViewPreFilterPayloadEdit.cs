namespace Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.PreFilterPayloadEdit;

using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Ngaq.Ui;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.PreFilterVisualEdit;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;

using Ctx = PreFilterVisualEdit.VmPreFilterVisualEdit;
using K = Infra.I18n.KeysUiI18nCommon;

/// GUI editor for PreFilter payload data (without Po fields).
public class ViewPreFilterPayloadEdit: AppViewBase<Ctx>{

	public ViewPreFilterPayloadEdit(Ctx Ctx){
		this.Ctx = Ctx;
		Render();
		InitVisualGridSource();
	}

	TreeDataGrid? CoreGrid;
	TreeDataGrid? PropGrid;
	FlatTreeDataGridSource<Ctx.RowFieldsFilterCard>? CoreGridSource;
	FlatTreeDataGridSource<Ctx.RowFieldsFilterCard>? PropGridSource;
	GridStack Root = new(IsRow: true);

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
			new(1, GUT.Star),
		]);
		root.A(MkErrorBar());
		var tabs = new TabControl{
			Margin = new(10, 8, 10, 10),
		};
		tabs.Items.Add(new TabItem{
			Header = I[K.CoreFilter],
			Content = MkFieldsFilterGridSection(true),
		});
		tabs.Items.Add(new TabItem{
			Header = I[K.PropFilter],
			Content = MkFieldsFilterGridSection(false),
		});
		root.A(tabs);
		return root.Grid;
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

	FlatTreeDataGridSource<Ctx.RowFieldsFilterCard> MkGridSource(ObservableCollection<Ctx.RowFieldsFilterCard> Rows){
		return new FlatTreeDataGridSource<Ctx.RowFieldsFilterCard>(Rows){
			Columns = {
				new TextColumn<Ctx.RowFieldsFilterCard, str>("", x=>x.UiIdxText, width: new GridLength(1, GUT.Auto)),
				new TextColumn<Ctx.RowFieldsFilterCard, str>(I[K.Items], x=>x.FilterCountText, width: new GridLength(1, GUT.Auto)),
				new TextColumn<Ctx.RowFieldsFilterCard, str>(I[K.TextPreview], x=>x.ContentPreview, width: new GridLength(1, GUT.Star)),
			},
		};
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

	Control MkFieldsFilterGridSection(bool IsCore){
		var box = new Border{
			BorderBrush = Brushes.DimGray,
			BorderThickness = new(1),
			Padding = new(10),
		};
		var root = new GridStack(IsRow:true);
		root.Grid.SetRowDefs([
			new(1, GUT.Auto),
			new(1, GUT.Star),
		]);
		box.Child = root.Grid;

		var title = IsCore ? I[K.CoreFilter] : I[K.PropFilter];
		var top = new GridStack(IsRow:false);
		top.Grid.SetColDefs([
			new(1, GUT.Star),
			new(1, GUT.Auto),
		]);
		top.A(new TextBlock(), o=>{
			o.Text = title;
			o.FontWeight = FontWeight.SemiBold;
			o.VerticalAlignment = VAlign.Center;
		});
		top.A(new Button(), o=>{
			o.Content = Icons.Add().ToIcon().WithText(I[K.AddGroup]);
			o.HorizontalAlignment = HAlign.Right;
			o.Click += (s,e)=>{
				if(IsCore){
					Ctx?.AddCoreGroup();
				}else{
					Ctx?.AddPropGroup();
				}
			};
		});
		root.A(top.Grid);

		var grid = new TreeDataGrid{
			MinHeight = 180,
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
		if(IsCore){
			CoreGrid = grid;
			grid.AddHandler(InputElement.TappedEvent, OnCoreGridTapped, RoutingStrategies.Bubble, true);
		}else{
			PropGrid = grid;
			grid.AddHandler(InputElement.TappedEvent, OnPropGridTapped, RoutingStrategies.Bubble, true);
		}
		root.A(grid);
		return box;
	}

	void OnCoreGridTapped(object? sender, TappedEventArgs e){
		OnFieldsGridTapped(e, IsCore: true);
	}

	void OnPropGridTapped(object? sender, TappedEventArgs e){
		OnFieldsGridTapped(e, IsCore: false);
	}

	void OnFieldsGridTapped(TappedEventArgs e, bool IsCore){
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
					if(IsCore){
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
		var g = new GridStack(IsRow:false);
		g.Grid.ColumnDefinitions.Add(new(1, GUT.Star));
		g.A(new Button(), o=>{
			o.HorizontalContentAlignment = HAlign.Center;
			o.Content = Icons.Save().ToIcon().WithText(I[K.SaveDraft]);
			o.Background = UiCfg.Inst.MainColor;
			o.Click += (s,e)=>{
				if(Ctx?.CommitPreFilterDataDraft() == true){
					ViewNavi?.Back();
				}
			};
		});
		return g.Grid;
	}
}





