namespace Ngaq.Ui.Views.Word.WordManage.UserLang.UserLangPage;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
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
using Ngaq.Ui.Views.Word.WordManage.UserLang.UserLangEdit;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsI18n;
using Ctx = VmUserLangPage;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

/// UserLang 列表頁視圖。
/// 上部爲搜索與新增，主體爲 TreeDataGrid，底部爲分頁條。
public partial class ViewUserLangPage
	:AppViewBase<Ctx>
	,I_MkTitleMenu
{

	public ViewUserLangPage(){
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

	/// 統一定義通用拉伸樣式。
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
	FlatTreeDataGridSource<Ctx.RowUserLang>? GridSource;

	/// 組裝頁面骨架。
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

	/// 創建頂部搜索條。
	protected Control MkTopBar(){
		var top = new GridStack(IsRow:false);
		top.Grid.SetColDefs([
			new(8, GUT.Star),
			new(1, GUT.Star),
			new(1, GUT.Star),
		]);
		var searchBtn = new OpBtn();
		top.A(new TextBox(), o=>{
			o.Watermark = I[K.InputUserLangNameToSearch];
			o.CBind<Ctx>(o.PropText,x=>x.Input);
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
			o.CBind<Ctx>(IsVisibleProperty, x=>x.ShowManageActions, Mode: BindingMode.OneWay);
			o.Content = Icons.Add().ToIcon();
			o.Click += (s,e)=>Ctx?.OpenDetail();
		});
		return top.Grid;
	}

	/// 頁面標題菜單：放置不常用但重要的批量操作。
	public Control MkTitleMenu(){
		var menu = new ContextMenu();
		menu.Items.A(new MenuItem(), o=>{
			o.Header = Icons.DatabaseImport().ToIcon().WithText(I[K.AutoAddMissing]);
			o.Click += (s,e)=>{
				_ = Ctx?.AddAllUnregisteredUserLangs(default);
			};
		});
		return menu;
	}

	/// 創建列表容器與交互行樣式。
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

	/// 創建底部分頁條。
	protected Control MkPageBarHost(){
		var pageBar = new ViewPageBar();
		if(Ctx is not null){
			pageBar.Ctx = Ctx.PageBar;
		}
		return pageBar;
	}

	/// 建立 TreeDataGrid 列配置。
	protected nil InitDataGrid(){
		if(Ctx is null || Grid is null){
			return NIL;
		}
		GridSource = new FlatTreeDataGridSource<Ctx.RowUserLang>(Ctx.Rows){
			Columns = {
				new TextColumn<Ctx.RowUserLang, str>("", x=>x.UiIdxText, width: new GridLength(1, GUT.Auto)),
				new TextColumn<Ctx.RowUserLang, str>(I[K.Name], x=>x.Name, width: new GridLength(1, GUT.Auto)),
				new TextColumn<Ctx.RowUserLang, str>(I[K.RelLangType], x=>x.RelLangType, width: new GridLength(1, GUT.Auto)),
				new TextColumn<Ctx.RowUserLang, str>(I[K.RelLang], x=>x.RelLang, width: new GridLength(1, GUT.Auto)),
				new TextColumn<Ctx.RowUserLang, str>(I[K.ModifiedTime], x=>x.ModifiedTime, width: new GridLength(1, GUT.Star)),
			},
		};
		Grid.Source = GridSource;
		return NIL;
	}

	/// 捕獲行點擊，忽略 ToggleButton 內部交互並打開詳情。
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
				if(row.DataContext is Ctx.RowUserLang vmRow){
					Ctx.OpenDetail(vmRow);
					e.Handled = true;
				}
				return;
			}
		}
	}

	/// 打開編輯頁：`row==null` 為新增，否則載入既有項。
	void OpenDetail(Ctx.RowUserLang? row){
		var view = new ViewUserLangEdit();
		view.Ctx?.SetCreateMode(row?.Raw is null);
		view.Ctx?.FromPoUserLang(row?.Raw);
		var title = row?.Raw?.UniqName ?? I[K.NewUserLang];
		var titled = ToolView.WithTitle(title, view);
		ViewNavi?.GoTo(titled);
	}
}




