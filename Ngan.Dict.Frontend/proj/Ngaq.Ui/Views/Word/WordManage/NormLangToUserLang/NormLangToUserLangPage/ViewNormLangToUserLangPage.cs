namespace Ngaq.Ui.Views.Word.WordManage.NormLangToUserLang.NormLangToUserLangPage;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;
using Ngaq.Ui;
using Ngaq.Ui.Components.PageBar;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.Ctrls;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views.Word.WordManage.NormLangToUserLang.NormLangToUserLangEdit;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsI18n;
using Ctx = VmNormLangToUserLangPage;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

/// 標準語言到用戶語言映射列表頁視圖。
public partial class ViewNormLangToUserLangPage
	:AppViewBase<Ctx>
{

	public ViewNormLangToUserLangPage(){
		Ctx = App.DiOrMk<Ctx>();
		if(Ctx is not null){
			Ctx.OnOpenDetailRequested += OpenDetail;
		}
		Style();
		Render();
		InitDataGrid();
		Loaded += async(s,e)=>{
			_ = Ctx?.InitSearch(default);
		};
	}



	public partial class Cls{
		public static str FullStretch = nameof(FullStretch);
	}

	protected nil Style(){
		var S = Styles;
		S.A(
			Sty.Is<Control>(
				x=>x.Class(Cls.FullStretch)
			)
			.Set(x=>x.HorizontalAlignment, HAlign.Stretch)
			.Set(x=>x.VerticalAlignment, VAlign.Stretch)
		);
		return NIL;
	}

	GridStack Root = new(IsRow: true);
	TreeDataGrid? Grid;
	FlatTreeDataGridSource<Ctx.RowNormLangToUserLang>? GridSource;

	protected nil Render(){
		this.Content = Root.Grid;
		Root.SetRowDefs([
			new(1, GUT.Auto),
			new(1, GUT.Star),
			new(1, GUT.Auto),
		]);
		Root.A(MkTopBar());
		Root.A(MkGridHost());
		Root.A(MkPageBarHost());
		return NIL;
	}

	protected Control MkTopBar(){
		var top = new GridStack(IsRow:false);
		top.Grid.SetColDefs([
			new(7, GUT.Star),
			new(1, GUT.Star),
			new(1, GUT.Star),
		]);
		var searchBtn = new OpBtn();
		top.A(new TextBox(), o=>{
			o.CBind<Ctx>(o.PropText, x=>x.Input);
			o.KeyBindings.Add(new KeyBinding{
				Gesture = new KeyGesture(Key.Enter),
				Command = new RelayCommand(()=>searchBtn.PerformClick()),
			});
		})
		.A(searchBtn, o=>{
			o.Classes.Add(Cls.FullStretch);
			o.BtnContent = Icons.Search().ToIcon();
			o._Button.Background = UiCfg.Inst.MainColor;
			o.SetExe((Ct)=>Ctx?.InitSearch(Ct)!);
		})
		.A(new Button(), o=>{
			o.Classes.Add(Cls.FullStretch);
			o.Content = Icons.Add().ToIcon();
			o.Click += (s,e)=>Ctx?.OpenDetail();
		});
		return top.Grid;
	}

	protected Control MkGridHost(){
		Grid = new TreeDataGrid{
			MinHeight = 280,
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
		return Grid;
	}

	protected Control MkPageBarHost(){
		var pageBar = new ViewPageBar();
		if(Ctx is not null){
			pageBar.Ctx = Ctx.PageBar;
		}
		return pageBar;
	}

	protected nil InitDataGrid(){
		if(Ctx is null || Grid is null){
			return NIL;
		}
		GridSource = new FlatTreeDataGridSource<Ctx.RowNormLangToUserLang>(Ctx.Rows){
			Columns = {
				new TextColumn<Ctx.RowNormLangToUserLang, str>("", x=>x.UiIdxText, width: new GridLength(1, GUT.Auto)),
				new TextColumn<Ctx.RowNormLangToUserLang, str>(I[K.NormLang], x=>x.NormLang, width: new GridLength(1, GUT.Auto)),
				new TextColumn<Ctx.RowNormLangToUserLang, str>(I[K.UserLang], x=>x.UserLang, width: new GridLength(1, GUT.Auto)),
				new TextColumn<Ctx.RowNormLangToUserLang, str>(I[K.ModifiedTime], x=>x.ModifiedTime, width: new GridLength(1, GUT.Star)),
			},
		};
		Grid.Source = GridSource;
		return NIL;
	}

	protected void OnGridTapped(object? sender, TappedEventArgs e){
		if(Ctx is null || Grid is null){
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
				if(row.DataContext is Ctx.RowNormLangToUserLang vmRow){
					Ctx.OpenDetail(vmRow);
					e.Handled = true;
				}
				return;
			}
		}
	}

	void OpenDetail(Ctx.RowNormLangToUserLang? row){
		var view = new ViewNormLangToUserLangEdit();
		view.Ctx?.SetCreateMode(row?.Raw is null);
		view.Ctx?.FromPoNormLangToUserLang(row?.Raw);
		var title = row?.Raw?.NormLang ?? I[K.AddNormLangToUserLang];
		var titled = ToolView.WithTitle(title, view);
		ViewNavi?.GoTo(titled);
	}
}


