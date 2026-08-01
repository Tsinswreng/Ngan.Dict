namespace Ngaq.Ui.Views.Word.WordEditV2.WordLearnPage;

using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.Media;
using Ngaq.Core.Infra;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;
using Avalonia;
using Ngaq.Ui.Tools;
using Avalonia.Interactivity;
using Tsinswreng.Avln.Grid;
using Tsinswreng.Avln.Dsl;

/// 學習記錄分頁：列表 + 新增，點行進入編輯頁。
public partial class ViewWordLearnPage: AppViewBase{
	public partial ViewWordLearnPage(){
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
			o.Click += (s, e)=>{
				Ctx?.AddRow();
				RebuildGrid();
			};
		});
		root.A(MkGrid());
		this.SetContent(root.Grid);
	}
	private partial Button MkBtnAdd(){
		var o = new Button();
		o.Margin = new(10, 10, 10, 4);
		o.StretchCenter();
		o.SetContent(Icons.Add().ToIcon().WithText(" "+I[K.AddLearn]));
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
		var source = new FlatTreeDataGridSource<VmWordLearnRow>(Ctx.Rows){
			Columns = {
				new TextColumn<VmWordLearnRow, str>("", x=>GetIdxText(x), width: new GridLength(1, GUT.Auto)),
				new TextColumn<VmWordLearnRow, str>(I[K.LearnResult], x=>x.LearnResultDisplayText, width: new GridLength(1, GUT.Auto)),
				new TextColumn<VmWordLearnRow, str>(I[K.Biz_CreatedAt], x=>x.BizCreatedAtDisplay, width: new GridLength(1, GUT.Star)),
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
		if(E.PropertyName == nameof(VmWordLearnPage.Rows)){
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
	private partial str GetIdxText(VmWordLearnRow Row){
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
				if(row.DataContext is VmWordLearnRow vmRow){
					Ctx.RequestEdit(vmRow);
					E.Handled = true;
				}
				return;
			}
		}
	}
}
