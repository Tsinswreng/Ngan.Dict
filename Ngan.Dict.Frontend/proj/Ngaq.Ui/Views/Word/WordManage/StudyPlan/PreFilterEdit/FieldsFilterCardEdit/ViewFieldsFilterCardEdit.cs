namespace Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.FieldsFilterCardEdit;

using System.Collections.Generic;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.Styling;
using Ngaq.Ui;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.PreFilterVisualEdit;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;

using Ctx = VmFieldsFilterCardEdit;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;


/// Editor view for a single fields-filter group.
public class ViewFieldsFilterCardEdit: AppViewBase<Ctx>{

	public ViewFieldsFilterCardEdit(){
		Ctx = App.DiOrMk<Ctx>();
		Render();
		InitGridSource();
	}

	GridStack Root = new(IsRow: true);
	TreeDataGrid? ItemGrid;
	FlatTreeDataGridSource<Ctx.RowFilterItemCard>? ItemGridSource;

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
		var tabs = new TabControl();
		tabs.Items.Add(new TabItem{
			Header = I[K.Fields],
			Content = MkFieldsTab(),
		});
		tabs.Items.Add(new TabItem{
			Header = I[K.FilterItems],
			Content = MkFilterItemsTab(),
		});
		return tabs;
	}

	Control MkFieldsTab(){
		var sv = new ScrollViewer();
		var root = new StackPanel{
			Spacing = 10,
			Margin = new(10),
		};
		sv.Content = root;

		var addBtn = new Button{
			Content = Icons.Add().ToIcon().WithText(I[K.AddField]),
			HorizontalAlignment = HAlign.Left,
		};
		addBtn.Click += (s,e)=>Ctx?.AddField();
		root.Children.Add(addBtn);

		var list = new ItemsControl();
		list.Bind(ItemsControl.ItemsSourceProperty, CBE.Mk<Ctx>(x=>x.Fields, Mode: BindingMode.OneWay));
		list.SetItemsPanel(()=>new StackPanel{Spacing = 8});
		list.SetItemTemplate<VmPreFilterVisualEdit.VmFieldValueRow>((item, ns)=>{
			var row = new GridStack(IsRow:false);
			row.Grid.SetColDefs([
				new(1, GUT.Star),
				new(1, GUT.Auto),
			]);

			var cb = new ComboBox{
				IsEditable = true,
				HorizontalAlignment = HAlign.Stretch,
			};
			cb.Bind(ComboBox.ItemsSourceProperty, CBE.Mk<Ctx>(x=>x.FieldOptionRows, Source: Ctx, Mode: BindingMode.OneWay));
			cb.ItemTemplate = new FuncDataTemplate<Ctx.RowTextOption>((option, _) =>
				new TextBlock{Text = option?.Display ?? ""}
			);
			InitFieldCombo(cb, item);
			row.A(cb);

			var rm = new Button{
				Content = Icons.Delete().ToIcon().WithText(I[K.Remove]),
				Background = UiCfg.Inst.DelBtnBg
			};
			rm.Click += (s,e)=>Ctx?.RemoveField(item);
			row.A(rm);
			return row.Grid;
		});
		root.Children.Add(list);
		return sv;
	}

	/// <summary>
	/// 字段下拉框顯示本地化文字，但實際仍回寫原始字段鍵。
	/// </summary>
	void InitFieldCombo(ComboBox Combo, VmPreFilterVisualEdit.VmFieldValueRow Row){
		void RefreshText(){
			if(Ctx is null){
				return;
			}
			Combo.Text = Ctx.ToFieldDisplay(Row.Value);
		}

		void CommitText(){
			if(Ctx is null){
				return;
			}
			var raw = Ctx.ToFieldRaw(Combo.Text);
			if(Row.Value != raw){
				Row.Value = raw;
			}
			var display = Ctx.ToFieldDisplay(Row.Value);
			if(Combo.Text != display){
				Combo.Text = display;
			}
		}

		RefreshText();
		Combo.SelectionChanged += (s,e)=>{
			if(Combo.SelectedItem is Ctx.RowTextOption option){
				Row.Value = option.Raw;
				Combo.Text = option.Display;
			}
		};
		Combo.LostFocus += (s,e)=>CommitText();
		Row.PropertyChanged += (s,e)=>{
			if(e.PropertyName == nameof(VmPreFilterVisualEdit.VmFieldValueRow.Value)){
				RefreshText();
			}
		};
	}

	Control MkFilterItemsTab(){
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
			o.Text = I[K.TapRowToEditOneFilterItem];
			o.VerticalAlignment = VAlign.Center;
		});
		top.A(new Button(), o=>{
			o.Content = Icons.Add().ToIcon().WithText(I[K.AddItem]);
			o.Click += (s,e)=>Ctx?.AddItem();
		});
		root.A(top.Grid);

		ItemGrid = new TreeDataGrid{
			MinHeight = 220,
			HorizontalAlignment = HAlign.Stretch,
		};
		ItemGrid.Styles.A(
			Sty.OfType<TreeDataGridRow>(x=>x.Class(":pointerover"))
			.Set(x=>x.Background, new SolidColorBrush(Color.FromRgb(46, 46, 46)))
		).A(
			Sty.OfType<TreeDataGridRow>(x=>x.Class(":pressed"))
			.Set(x=>x.Background, new SolidColorBrush(Color.FromRgb(70, 70, 70)))
		);
		ItemGrid.AddHandler(InputElement.TappedEvent, OnGridTapped, RoutingStrategies.Bubble, true);
		root.A(ItemGrid);
		return root.Grid;
	}

	void InitGridSource(){
		if(Ctx is null || ItemGrid is null){
			return;
		}
		ItemGridSource = new FlatTreeDataGridSource<Ctx.RowFilterItemCard>(Ctx.ItemCards){
			Columns = {
				new TextColumn<Ctx.RowFilterItemCard, str>("", x=>x.UiIdxText, width: new GridLength(1, GUT.Auto)),
				new TextColumn<Ctx.RowFilterItemCard, str>(I[K.Operation], x=>x.Operation, width: new GridLength(1, GUT.Auto)),
				new TextColumn<Ctx.RowFilterItemCard, str>(I[K.ValueType], x=>x.ValueType, width: new GridLength(1, GUT.Auto)),
				new TextColumn<Ctx.RowFilterItemCard, str>(I[K.Values], x=>x.ValuesPreview, width: new GridLength(1, GUT.Star)),
			},
		};
		ItemGrid.Source = ItemGridSource;
	}

	void OnGridTapped(object? sender, TappedEventArgs e){
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
				if(row.DataContext is Ctx.RowFilterItemCard vmRow){
					Ctx.OpenFilterItem(vmRow);
					e.Handled = true;
				}
				return;
			}
		}
	}

	Control MkBottomBar(){
		var g = new GridStack(IsRow:false);
		g.Grid.SetColDefs([
			new(1, GUT.Star),
			new(1, GUT.Star),
		]);
		g.A(new Button(), o=>{
			o.HorizontalContentAlignment = HAlign.Center;
			o.Background = UiCfg.Inst.DelBtnBg;
			o.Content = Icons.Delete().ToIcon().WithText(I[K.Delete]);
			o.Click += (s,e)=>Ctx?.Delete();
		});
		g.A(new Button(), o=>{
			o.Content = Icons.Save().ToIcon().WithText(I[K.SaveGroup]);
			o.Background = UiCfg.Inst.MainColor;
			o.Click += (s,e)=>Ctx?.Save();
			o.StretchCenter();
		});
		return g.Grid;
	}
}

