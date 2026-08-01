namespace Ngaq.Ui.Views.Word.WordEditV2.WordPropPage;

using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.Layout;
using Avalonia.Media;
using Ngaq.Core.Infra;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;
using Avalonia;
using Avalonia.Interactivity;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Infra.Ctrls;
using Tsinswreng.Avln.Grid;
using Tsinswreng.Avln.Dsl;

/// 屬性分頁：列表 + 新增，點行進入編輯頁。
public partial class ViewWordPropPage
	: AppViewBase
{
	public partial ViewWordPropPage(){
		Render();
		DataContextChanged += (s, e)=>OnCtxChanged();
	}
	partial void Render(){
		var root = new GridStack(IsRow: true);
		root.Grid.SetRowDefs([
			new(1, GUT.Auto),
			new(9, GUT.Star),
		]);
		root.A(MkBtnAdd(), o=>{
			BtnAddProp = o.ToIBtn();
			o.SetExe(ct=>{
				Ctx?.AddRow();
				RebuildGrid();
				return Task.FromResult(NIL);
			});
		});
		root.A(MkGrid());
		this.SetContent(root.Grid);
	}
	private partial OpBtn MkBtnAdd(){
		var o = new OpBtn();
		o.Margin = new(10, 10, 10, 4);
		o._Button.StretchCenter();
		o.BtnContent = Icons.Add().ToIcon().WithText(" "+I[K.AddProp]);
		return o;
	}
	private partial Control MkGrid(){
		Grid = new TreeDataGrid{
			Margin = new(10, 4, 10, 10),
			MinHeight = 260,
			HorizontalAlignment = HAlign.Stretch,
		};
		Grid.Styles.Add(
			Sty.OfType<TreeDataGridRow>(x=>x.Class(":pointerover"))
			.Set(x=>x.Background, new SolidColorBrush(Color.FromRgb(46, 46, 46)))
		);
		Grid.Styles.Add(
			Sty.OfType<TreeDataGridRow>(x=>x.Class(":pressed"))
			.Set(x=>x.Background, new SolidColorBrush(Color.FromRgb(70, 70, 70)))
		);
		Grid.AddHandler(InputElement.TappedEvent, OnGridTapped, RoutingStrategies.Bubble, true);
		RebuildGrid();
		return Grid;
	}
	partial void RebuildGrid(){
		if(Ctx is null || Grid is null){
			return;
		}
		var source = new FlatTreeDataGridSource<VmWordPropRow>(Ctx.Rows){
			Columns = {
				new TextColumn<VmWordPropRow, str>("", x=>GetIdxText(x), width: new GridLength(1, GUT.Auto)),
				new TextColumn<VmWordPropRow, str>(I[K.Key], x=>x.KeyDisplayText, width: new GridLength(1, GUT.Auto)),
				new TextColumn<VmWordPropRow, str>(I[K.Values], x=>x.ValueDisplayText, width: new GridLength(1, GUT.Star)),
			},
		};
		Grid.Source = source;
	}
	partial void OnCtxChanged(){
		if(SubscribedCtx is not null){
			SubscribedCtx.PropertyChanged -= OnCtxPropertyChanged;
			SubscribedCtx = null;
		}
		if(RowsNotifier is not null){
			RowsNotifier.CollectionChanged -= OnRowsChanged;
			RowsNotifier = null;
		}
		if(Ctx is null){
			return;
		}
		SubscribedCtx = Ctx;
		Ctx.PropertyChanged += OnCtxPropertyChanged;
		HookRowsChanged();
		RebuildGrid();
	}
	partial void OnCtxPropertyChanged(object? Sender, PropertyChangedEventArgs E){
		if(E.PropertyName == nameof(VmWordPropPage.Rows)){
			HookRowsChanged();
			RebuildGrid();
		}
	}
	partial void HookRowsChanged(){
		if(RowsNotifier is not null){
			RowsNotifier.CollectionChanged -= OnRowsChanged;
		}
		RowsNotifier = Ctx?.Rows;
		if(RowsNotifier is not null){
			RowsNotifier.CollectionChanged += OnRowsChanged;
		}
	}
	partial void OnRowsChanged(object? Sender, NotifyCollectionChangedEventArgs E){
		RebuildGrid();
	}
	private partial str GetIdxText(VmWordPropRow Row){
		if(Ctx is null){
			return "";
		}
		var idx = Ctx.Rows.IndexOf(Row);
		return idx < 0 ? "" : (idx+1)+"";
	}
	partial void OnGridTapped(object? Sender, TappedEventArgs E){
		if(Ctx is null){
			return;
		}
		if(E.Source is not StyledElement src){
			return;
		}
		for(StyledElement? cur = src; cur is not null; cur = cur.Parent){
			if(cur is TreeDataGridRow row){
				if(row.DataContext is VmWordPropRow vmRow){
					Ctx.RequestEdit(vmRow);
					E.Handled = true;
				}
				return;
			}
		}
	}
}
