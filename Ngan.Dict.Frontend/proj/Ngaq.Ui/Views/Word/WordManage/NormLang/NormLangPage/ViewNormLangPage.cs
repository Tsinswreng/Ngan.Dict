namespace Ngaq.Ui.Views.Word.WordManage.NormLang.NormLangPage;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Ngaq.Ui;
using Ngaq.Ui.Components.PageBar;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.Ctrls;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views.Word.WordManage.NormLang.NormLangEdit;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsI18n;
using Ctx = VmNormLangPage;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

public partial class ViewNormLangPage
	:AppViewBase<Ctx>
	,I_MkTitleMenu
{

	public ViewNormLangPage(){
		Ctx = App.DiOrMk<Ctx>();
		if(Ctx is not null){
			Ctx.OnOpenDetailRequested += OpenDetail;
		}
		Style();
		Render();
		InitDataGrid();
		Loaded += async(s,e)=>{
			_ = Ctx?.InitSearch(default);
			// 語言選擇頁打開時聚焦搜索框，並全選已有文本，便於直接覆寫。
			Dispatcher.UIThread.Post(()=>{
				FocusAndSelectSearchInput();
			});
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
	FlatTreeDataGridSource<Ctx.RowNormLang>? GridSource;
	TextBox? SearchInput;

	protected nil Render(){
		this.Content = Root.Grid;
		Root.SetRowDefs([
			new(1, GUT.Auto),
			new(1, GUT.Star),
			new(1, GUT.Auto),
		])
		.A(MkTopBar())
		.A(MkGridHost())
		.A(MkPageBarHost());
		return NIL;
	}

	protected Control MkTopBar(){
		var top = new GridStack(IsRow:false);
		top.SetColDefs([
			new(1, GUT.Star),
			new(7, GUT.Star),
			new(1, GUT.Star),
			new(1, GUT.Star),
		]);
		var clearBtn = new OpBtn();
		var searchBtn = new OpBtn();
		top.A(clearBtn, o=>{
			o.Classes.Add(Cls.FullStretch);
			o.BtnContent = Icons.CloseX();
			// 清空按鈕直接複用搜索流程，保持與手動「清空後點搜索」一致。
			o.SetExe((Ct)=>ClearSearchAndSearch(Ct, searchBtn));
		})
		.A(new TextBox(), o=>{
			SearchInput = o;
			o.Watermark = I[K.InputLangNameOrCode];
			o.CBind<Ctx>(o.PropText, x=>x.Input);
			o.KeyBindings.Add(new KeyBinding{
				Gesture = new KeyGesture(Key.Enter),
				Command = new RelayCommand(()=>searchBtn.PerformClick()),
			});
		})
		.A(searchBtn, o=>{
			o.Classes.Add(Cls.FullStretch);
			o.BtnContent = Icons.Search();
			o._Button.Background = UiCfg.Inst.MainColor;
			o.SetExe((Ct)=>Ctx?.InitSearch(Ct)!);
		})
		.A(new Button(), o=>{
			o.Classes.Add(Cls.FullStretch);
			o.CBind<Ctx>(IsVisibleProperty, x=>x.CanCreate, Mode: BindingMode.OneWay);
			o.Content = Icons.Add();
			o.Click += (s,e)=>Ctx?.OpenDetail();
		});
		return top.Grid;
	}

	/// 清空搜索框後立刻重新搜索，等價於「手動刪空文本後再按一次搜索」。
	async Task<nil> ClearSearchAndSearch(CT Ct, OpBtn SearchBtn){
		if(SearchInput is not null){
			SearchInput.Text = "";
		}
		if(Ctx is not null){
			Ctx.Input = "";
		}
		await SearchBtn.ClickAndWaitDone(Ct);
		return NIL;
	}

	nil FocusAndSelectSearchInput(){
		if(SearchInput is null){
			return NIL;
		}
		SearchInput.Focus();
		SearchInput.SelectAll();
		return NIL;
	}

	protected Control MkGridHost(){
		Grid = new TreeDataGrid{
			MinHeight = 280,
			HorizontalAlignment = HAlign.Stretch,
		};
		Grid.Styles.A(
			Sty.OfType<TreeDataGridRow>(x=>x.Class(":pointerover"))
			.Set(x=>x.Background, new SolidColorBrush(Color.FromRgb(46, 46, 46)))
		).A(
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
		GridSource = new FlatTreeDataGridSource<Ctx.RowNormLang>(Ctx.Rows){
			Columns = {
				new TextColumn<Ctx.RowNormLang, str>("", x=>x.UiIdxText, width: new GridLength(1, GUT.Auto)),
				new TextColumn<Ctx.RowNormLang, str>(I[K.Code], x=>x.Code, width: new GridLength(1, GUT.Auto)),
				new TextColumn<Ctx.RowNormLang, str>(I[K.TranslatedName], x=>x.TranslatedName, width: new GridLength(1, GUT.Auto)),
				new TextColumn<Ctx.RowNormLang, str>(I[K.NativeName], x=>x.NativeName, width: new GridLength(1, GUT.Star)),
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
				if(row.DataContext is Ctx.RowNormLang vmRow){
					Ctx.OpenDetail(vmRow);
					e.Handled = true;
				}
				return;
			}
		}
	}

	void OpenDetail(Ctx.RowNormLang? row){
		var view = new ViewNormLangEdit();
		view.Ctx?.SetCreateMode(row?.Raw is null);
		view.Ctx?.FromPoNormLang(row?.Raw);
		var title = row?.Raw?.Code ?? I[K.AddNormLang];
		var titled = ToolView.WithTitle(title, view);
		ViewNavi?.GoTo(titled);
	}

	public Control MkTitleMenu(){
		var menu = new ContextMenu();
		menu.Items.A(new MenuItem(), o=>{
			o.Header = I[K.InitBuiltin];
			o.Click += async(s,e)=>{
				if(Ctx is null){
					return;
				}
				await Ctx.InitBuiltinNormLang(default);
			};
		});
		return menu;
	}
}


