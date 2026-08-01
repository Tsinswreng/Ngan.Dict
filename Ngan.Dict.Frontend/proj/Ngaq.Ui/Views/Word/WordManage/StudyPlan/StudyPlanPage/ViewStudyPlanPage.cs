namespace Ngaq.Ui.Views.Word.WordManage.StudyPlan.StudyPlanPage;

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
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.StudyPlanEdit;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsI18n;
using Ctx = VmStudyPlanPage;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

public partial class ViewStudyPlanPage
	:AppViewBase<Ctx>
{

	public ViewStudyPlanPage(){
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
	FlatTreeDataGridSource<Ctx.RowStudyPlan>? GridSource;

	public nil Render(){
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

	Control MkTopBar(){
		var top = new GridStack(IsRow:false);
		top.SetColDefs([
			new(7, GUT.Star),
			new(1, GUT.Star),
			new(1, GUT.Star),
		]);
		var searchBtn = new OpBtn();
		top.A(new TextBox(), o=>{
			o.CBind<Ctx>(
				o.PropText,
				x=>x.Input
			);
			o.KeyBindings.Add(new KeyBinding{
				Gesture = new KeyGesture(Key.Enter),
				Command = new RelayCommand(()=>searchBtn.PerformClick()),
			});
		})
		.A(searchBtn, o=>{
			o.Classes.Add(Cls.FullStretch);
			o._Button.Background = UiCfg.Inst.MainColor;
			o.BtnContent = Icons.Search().ToIcon();
			o.SetExe((Ct)=>Ctx?.InitSearch(Ct)!);
		})
		.A(new Button(), o=>{
			o.Classes.Add(Cls.FullStretch);
			o.CBind<Ctx>(IsVisibleProperty, x=>x.CanCreate, Mode: Avalonia.Data.BindingMode.OneWay);
			o.Content = Icons.Add().ToIcon();
			o.Click += (s,e)=>Ctx?.OpenDetail();
		});
		return top.Grid;
	}

	Control MkGridHost(){
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

	Control MkPageBarHost(){
		var pageBar = new ViewPageBar();
		if(Ctx is not null){
			pageBar.Ctx = Ctx.PageBar;
		}
		return pageBar;
	}

	nil InitDataGrid(){
		if(Ctx is null || Grid is null){
			return NIL;
		}
		GridSource = new FlatTreeDataGridSource<Ctx.RowStudyPlan>(Ctx.Rows){
			Columns = {
				new TextColumn<Ctx.RowStudyPlan, str>("", x=>x.UiIdxText, width: new GridLength(1, GUT.Auto)),
				new TextColumn<Ctx.RowStudyPlan, str>(I[K.Name], x=>x.Name, width: new GridLength(1, GUT.Auto)),
				new TextColumn<Ctx.RowStudyPlan, str>(I[K.ModifiedTime], x=>x.ModifiedTime, width: new GridLength(1, GUT.Star)),
			},
		};
		Grid.Source = GridSource;
		return NIL;
	}

	void OnGridTapped(object? sender, TappedEventArgs e){
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
				if(row.DataContext is Ctx.RowStudyPlan vmRow){
					Ctx.OpenDetail(vmRow);
					e.Handled = true;
				}
				return;
			}
		}
	}

	void OpenDetail(Ctx.RowStudyPlan? row){
		var view = new ViewStudyPlanEdit();
		view.Ctx?.SetCreateMode(row?.Raw is null);
		view.Ctx?.FromPoStudyPlan(row?.Raw);
		var title = row?.Raw?.UniqName ?? I[K.NewStudyPlan];
		var titled = ToolView.WithTitle(title, view);
		ViewNavi?.GoTo(titled);
	}
}





