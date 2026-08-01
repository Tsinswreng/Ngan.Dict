namespace Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.PreFilterVisualEditV2;

using System.Collections.Generic;
using System.Linq;
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
using Ngaq.Core.Shared.StudyPlan.Models;
using Ngaq.Ui;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.Ctrls;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.FilterCardEditV2;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.PreFilterPayloadJsonEdit;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.PreFilterVisualEdit;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;

using Ctx = VmPreFilterVisualEditV2;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

/// <summary>
/// PreFilter V2 主頁。
/// 上半區保留 Po 基礎字段；下半區統一展示單一篩選列表。
/// </summary>
public class ViewPreFilterVisualEditV2: AppViewBase<Ctx>, I_MkTitleMenu{

	public ViewPreFilterVisualEditV2(){
		Ctx = App.DiOrMk<Ctx>();
		if(Ctx is not null){
			Ctx.OnOpenPayloadJsonRequested += OpenPayloadJsonEditor;
			Ctx.OnOpenLegacyVisualEditorRequested += OpenLegacyVisualEditor;
			Ctx.OnOpenFilterCardRequested += OpenFilterCard;
			Ctx.OnDialogRequested += msg=>MainView.Inst.ShowDialog(msg);
			Ctx.OnToastRequested += msg=>MainView.Inst.ShowToast(msg);
		}
		Render();
		InitVisualGridSource();
	}

	GridStack Root = new(IsRow: true);
	TreeDataGrid? Grid;
	FlatTreeDataGridSource<Ctx.RowFieldsFilterCard>? GridSource;

	protected nil Render(){
		Content = Root.Grid;
		Root.SetRowDefs([
			new(1, GUT.Star),
			new(1, GUT.Auto),
		]);
		Root
		.A(MkBody())
		.A(MkBottomBar())
		;
		return NIL;
	}

	Control MkBody(){
		var root = new GridStack(IsRow:true);
		root.Grid.SetRowDefs([
			new(1, GUT.Auto),
			new(1, GUT.Auto),
			new(1, GUT.Star),
		]);
		root
		.A(MkErrorBar())
		.A(MkPoSection(), o=>o.Margin = new(10, 10, 10, 8))
		.A(MkIntegratedDataEditor(), o=>o.Margin = new(10, 0, 10, 10))
		;
		return root.Grid;
	}

	Control MkErrorBar(){
		var b = new Border{
			Background = new SolidColorBrush(Color.FromArgb(80, 180, 30, 30)),
			Padding = new(10, 6),
			IsVisible = false,
		};
		b.CBind<Ctx>(IsVisibleProperty, x=>x.HasError, Mode: BindingMode.OneWay);
		var txt = new TextBlock{ Foreground = Brushes.White };
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
		.A(MkIdRow(I[K.Id], CBE.Mk<Ctx>(x=>x.PoIdText, Mode: BindingMode.OneWay)))
		.A(MkInputRow(I[K.Name], CBE.Mk<Ctx>(x=>x.PoUniqName, Mode: BindingMode.TwoWay)))
		.A(MkInputRow(I[K.Description], CBE.Mk<Ctx>(x=>x.PoDescr, Mode: BindingMode.TwoWay), AcceptsReturn: true));
		var typeRow = MkComboRow(I[K.Type], Ctx?.PoTypeOptions ?? [], CBE.Mk<Ctx>(x=>x.PoTypeIndex, Mode: BindingMode.TwoWay));
		typeRow.CBind<Ctx>(IsVisibleProperty, x=>x.ShowPoTypeField, Mode: BindingMode.OneWay);
		sp.A(typeRow);
		return bdr;
	}

	void InitVisualGridSource(){
		if(Ctx is null || Grid is null){
			return;
		}
		GridSource = new FlatTreeDataGridSource<Ctx.RowFieldsFilterCard>(Ctx.FilterCards){
			Columns = {
				new TextColumn<Ctx.RowFieldsFilterCard, str>("", x=>x.UiIdxText, width: new GridLength(1, GUT.Auto)),
				new TextColumn<Ctx.RowFieldsFilterCard, str>(I[K.Items], x=>x.FilterCountText, width: new GridLength(1, GUT.Auto)),
				new TextColumn<Ctx.RowFieldsFilterCard, str>(I[K.TextPreview], x=>x.ContentPreview, width: new GridLength(1, GUT.Star)),
			},
		};
		Grid.Source = GridSource;
	}

	Control MkIntegratedDataEditor(){
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
		top.A(new TextBlock(), o=>{ o.Text = I[K.PreFilter]; })
		.A(new Button(), o=>{
			o.Content = Icons.Add().ToIcon().WithText(I[K.AddGroup]);
			o.HorizontalContentAlignment = HAlign.Center;
			o.Click += (s,e)=>Ctx?.AddGroup();
		})
		;
		root.A(top.Grid);

		Grid = new TreeDataGrid{ MinHeight = 220, HorizontalAlignment = HAlign.Stretch };
		Grid.Styles
			.A(Sty.OfType<TreeDataGridRow>(x=>x.Class(":pointerover")).Set(x=>x.Background, new SolidColorBrush(Color.FromRgb(46,46,46))))
			.A(Sty.OfType<TreeDataGridRow>(x=>x.Class(":pressed")).Set(x=>x.Background, new SolidColorBrush(Color.FromRgb(70,70,70))))
			;
		Grid.AddHandler(InputElement.TappedEvent, OnGridTapped, RoutingStrategies.Bubble, true);
		root.A(Grid);
		return root.Grid;
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
				if(row.DataContext is Ctx.RowFieldsFilterCard vmRow){
					Ctx.OpenFilterCard(vmRow);
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
		}).A(new OpBtn(), o=>{
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
		var row = new StackPanel{Spacing = 6, Orientation = Orientation.Horizontal};
		row.Children.Add(new TextBlock{ Text = Label + ":", FontSize = UiCfg.Inst.BaseFontSize * 0.8 });
		var value = new SelectableTextBlock{ FontSize = UiCfg.Inst.BaseFontSize * 0.8 };
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
		menu.Items.A(new MenuItem(), o=>{
			o.Header = I[K.OpenLegacyVisualEditor];
			o.Click += (s,e)=>Ctx?.OpenLegacyVisualEditor();
		});
		return menu;
	}

	void OpenPayloadJsonEditor(BoPreFilter bo){
		var view = new ViewPreFilterPayloadJsonEdit();
		view.Ctx?.Load(bo.PoPreFilter, po=>{
			Ctx?.OnPayloadJsonSavedOrDeleted(po);
		});
		ViewNavi?.GoTo(ToolView.WithTitle(I[K.TextPayload], view));
	}

	void OpenLegacyVisualEditor(BoPreFilter bo){
		var view = new ViewPreFilterVisualEdit();
		view.Ctx?.SetCreateMode(Ctx?.IsCreateMode == true);
		view.Ctx?.FromBoPreFilter(bo);
		var title = Ctx?.PoUniqName ?? I[K.PreFilter];
		ViewNavi?.GoTo(ToolView.WithTitle(title, view));
	}

	void OpenFilterCard(Ctx.RowFieldsFilterCard card){
		if(Ctx is null || card.Raw is null){
			return;
		}
		var view = new ViewFilterCardEditV2();
		view.Ctx?.Load(Ctx, card.Raw, card.UiIdx);
		ViewNavi?.GoTo(ToolView.WithTitle(I[K.PreFilter] + " #" + card.UiIdx, view));
	}
}

