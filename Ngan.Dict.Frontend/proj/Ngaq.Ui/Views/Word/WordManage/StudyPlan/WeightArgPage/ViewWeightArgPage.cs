namespace Ngaq.Ui.Views.Word.WordManage.StudyPlan.WeightArgPage;

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
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.WeightArgEdit;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsI18n;
using Ctx = VmWeightArgPage;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;
public partial class ViewWeightArgPage
	:AppViewBase<Ctx>
{

	public ViewWeightArgPage(){
		Ctx = App.DiOrMk<Ctx>();
		if(Ctx is not null){
			Ctx.OnOpenDetailRequested += OpenDetail;
		}
		Style();
		Render();
		InitDataGrid();
		Loaded+=async(s,e)=>{
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
	TreeDataGrid? WeightArgGrid;
	FlatTreeDataGridSource<Ctx.RowWeightArg>? GridSource;

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
			o.BtnContent = Icons.Search().ToIcon();
			o._Button.Background = UiCfg.Inst.MainColor;
			o.SetExe((Ct)=>Ctx?.InitSearch(Ct)!);
		})
		.A(new Button(), o=>{
			o.Classes.Add(Cls.FullStretch);
			o.CBind<Ctx>(IsVisibleProperty, x=>x.CanCreate, Mode: BindingMode.OneWay);
			o.Content = Icons.Add().ToIcon();
			o.Click += (s,e)=>Ctx?.OpenDetail();
		});
		return top.Grid;
	}

	protected Control MkGridHost(){
		WeightArgGrid = new TreeDataGrid{
			MinHeight = 280,
			HorizontalAlignment = HAlign.Stretch,
		};
		WeightArgGrid.Styles.Add(
			Sty.OfType<TreeDataGridRow>(x=>x.Class(":pointerover"))
			.Set(x=>x.Background, new SolidColorBrush(Color.FromRgb(46, 46, 46)))
		);
		WeightArgGrid.Styles.Add(
			Sty.OfType<TreeDataGridRow>(x=>x.Class(":pressed"))
			.Set(x=>x.Background, new SolidColorBrush(Color.FromRgb(70, 70, 70)))
		);
		WeightArgGrid.AddHandler(InputElement.TappedEvent, OnGridTapped, RoutingStrategies.Bubble, true);
		return WeightArgGrid;
	}

	protected Control MkPageBarHost(){
		var pageBar = new ViewPageBar();
		if(Ctx is not null){
			pageBar.Ctx = Ctx.PageBar;
		}
		return pageBar;
	}

	protected nil InitDataGrid(){
		if(Ctx is null || WeightArgGrid is null){
			return NIL;
		}
		GridSource = new FlatTreeDataGridSource<Ctx.RowWeightArg>(Ctx.Rows){
			Columns = {
				new TextColumn<Ctx.RowWeightArg, str>("", x=>x.UiIdxText, width: new GridLength(1, GUT.Auto)),
				new TextColumn<Ctx.RowWeightArg, str>(I[K.Name], x=>x.Name, width: new GridLength(1, GUT.Auto)),
				new TextColumn<Ctx.RowWeightArg, str>(I[K.ModifiedTime], x=>x.ModifiedTime, width: new GridLength(1, GUT.Star)),
			},
		};
		WeightArgGrid.Source = GridSource;
		return NIL;
	}

	protected void OnGridTapped(object? sender, TappedEventArgs e){
		if(Ctx is null || WeightArgGrid is null){
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
				if(row.DataContext is Ctx.RowWeightArg vmRow){
					Ctx.OpenDetail(vmRow);
					e.Handled = true;
				}
				return;
			}
		}
	}

	/// <summary>
	/// 普通模式下打开编辑页；选择模式下不会进入该分支。
	/// </summary>
	void OpenDetail(Ctx.RowWeightArg? row){
		var view = new ViewWeightArgEdit();
		view.Ctx?.SetCreateMode(row is null);
		view.Ctx?.FromPoWeightArg(row?.Raw);
		var title = row?.Raw?.UniqName ?? I[K.NewWeightArg];
		var titled = ToolView.WithTitle(title, view);
		ViewNavi?.GoTo(titled);
	}
}





