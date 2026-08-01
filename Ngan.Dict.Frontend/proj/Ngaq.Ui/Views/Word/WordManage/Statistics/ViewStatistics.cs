namespace Ngaq.Ui.Views.Word.WordManage.Statistics;

using System.Linq.Expressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;
using Ngaq.Core.Infra;
using Ngaq.Ui.Components.PageBar;
using Ngaq.Ui.Components.TempusBox;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.Ctrls;
using Ngaq.Ui.Tools;
using ScottPlot.Avalonia;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsI18n;
using Ctx = VmStatistics;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

public partial class ViewStatistics: AppViewBase{

	public Ctx? Ctx{
		get{return DataContext as Ctx;}
		protected set{DataContext = value;}
	}

	public ViewStatistics(){
		Ctx = App.GetRSvc<Ctx>();
		Style();
		Render();
	}



	protected nil Style(){
		Styles.A(
			Sty.Is<TextBox>()
			.Set(
				x=>x.Background
				,new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20))
			)
		);
		return NIL;
	}

	Control MkPageBar(){
		var view = new ViewPageBar();
		if(Ctx is not null){
			view.Ctx = Ctx.PageBar;
		}
		return view;
	}

	Control MkTempusInput(Expression<Func<Ctx, object?>> Expr){
		var o = new TempusBox();
		o.Bind(
			TempusBox.TempusProperty,
			CBE.Mk<Ctx>(Expr, Mode: BindingMode.TwoWay)
		);
		o.FormatItems.Add(TempusFormatItem.yy_MM_DD);
		o.FormatItems.Add(TempusFormatItem.yy_MM_DD__HH_mm);
		o.SelectedFormat = TempusFormatItem.yy_MM_DD__HH_mm;
		return o;
	}

	Control MkIntervalInput(){
		var grid = new Grid();
		grid.SetColDefs([
			new(4, GUT.Star),
			new(4, GUT.Star),
		]);

		var intervalBox = new TextBox();
		intervalBox.CBind<Ctx>(intervalBox.PropText, x=>x.IntervalNoUnit, Mode: BindingMode.TwoWay);
		Grid.SetColumn(intervalBox, 0);
		grid.Children.Add(intervalBox);

		var unitBox = new ComboBox{
			ItemsSource = new str[]{
				I[K.Second],
				I[K.Minute],
				I[K.Hour],
				I[K.Day],
				I[K.Week],
				I[K.Month],
				I[K.Year],
			}
		};
		unitBox.CBind<Ctx>(unitBox.PropSelectedIndex, x=>x.IntervalUnit, Mode: BindingMode.TwoWay);
		Grid.SetColumn(unitBox, 1);
		grid.Children.Add(unitBox);
		return grid;
	}

	Control MkLearnResultInput(){
		var box = new ComboBox{
			ItemsSource = Ctx.LearnResultOptions,
		};
		box.CBind<Ctx>(box.PropSelectedIndex, x=>x.LearnResultIndex, Mode: BindingMode.TwoWay);
		return box;
	}

	Control MkCfgForm(){
		var grid = new Grid();
		grid.SetColDefs([
			new(130, GUT.Pixel),
			new(1, GUT.Star),
		]);
		grid.SetRowDefs([
			new(1, GUT.Auto),
			new(1, GUT.Auto),
			new(1, GUT.Auto),
			new(1, GUT.Auto),
		]);

		i32 row = 0;
		void AddRow(str label, Control input){
			var text = new TextBlock{
				Text = label,
				VerticalAlignment = VAlign.Center,
				Margin = new(0, 4, 8, 4),
			};
			Grid.SetRow(text, row);
			Grid.SetColumn(text, 0);
			grid.Children.Add(text);

			input.Margin = new(0, 4, 0, 4);
			Grid.SetRow(input, row);
			Grid.SetColumn(input, 1);
			grid.Children.Add(input);
			row++;
		}

		AddRow(I[K.StartTime], MkTempusInput(x=>x.TimeStart));
		AddRow(I[K.EndTime], MkTempusInput(x=>x.TimeEnd));
		AddRow(I[K.Interval], MkIntervalInput());
		AddRow(I[K.LearnResult], MkLearnResultInput());

		return grid;
	}

	Control MkCfgPanel(){
		var cfg = new GridStack(IsRow: true);
		cfg.Grid.SetRowDefs([
			new(1, GUT.Star),
			new(1, GUT.Auto),
		]);

		cfg
		.A(new ScrollViewer(), sv=>{
			sv.SetContent(MkCfgForm(), o=>{
				o.Margin = new(8, 8, 8, 4);
			});
		})
		.A(new OpBtn(), o=>{
			o.Margin = new(8, 4, 8, 8);
			o._Button.HAlign(x=>x.Stretch);
			o._Button.StretchCenter();
			o.BtnContent = I[K.Count];
			o._Button.Background = UiCfg.Inst.MainColor;
			o.SetExe((Ct)=>Ctx?.GetDataAsy(Ct));
		});

		return cfg.Grid;
	}

	void DrawPlot(AvaPlot o){
		if(Ctx is null){
			return;
		}
		// 統一套用黑底白字主題，避免刷新後回到默認配色。
		o.Plot.FigureBackground.Color = ScottPlot.Colors.Black;
		o.Plot.DataBackground.Color = ScottPlot.Colors.Black;
		var frameColor = ScottPlot.Colors.Gray;
		o.Plot.Axes.Color(ScottPlot.Colors.White);
		o.Plot.Axes.FrameColor(frameColor);
		o.Plot.Grid.MajorLineColor = frameColor;
		o.Plot.Grid.MajorLineWidth = 0.5f;

		o.Plot.Clear();
		var series = o.Plot.Add.Scatter(Ctx.Points, ScottPlot.Colors.LightBlue);
		series.LineWidth = 1;
		series.MarkerSize = 0;

		var tickPositions = new List<double>();
		var tickLabels = new List<string>();
		for(i32 i = 0; i < Ctx.Points.Count; ++i){
			double ms = Ctx.Points[i].X;
			var dt = DateTimeOffset.FromUnixTimeMilliseconds((long)ms);
			tickPositions.Add(ms);
			tickLabels.Add(dt.ToString("yy-MM-dd"));
		}

		o.Plot.Axes.Bottom.TickGenerator =
			new ScottPlot.TickGenerators.NumericManual(tickPositions.ToArray(), tickLabels.ToArray());
		o.Plot.Axes.Bottom.TickLabelStyle.Rotation = 60;
		//o.Plot.Axes.Bottom.MinimumSize = 96;

		for(i32 i = 0; i < Ctx.Points.Count; i++){
			var p = Ctx.Points[i];
			var txt = o.Plot.Add.Text(p.Y.ToString("0"), p.X, p.Y);
			txt.LabelFontColor = ScottPlot.Colors.White;
			txt.OffsetY = -0.3f;
			txt.LabelAlignment = ScottPlot.Alignment.LowerCenter;
		}
		o.Refresh();
		o.Plot.Axes.AutoScale();
	}

	GridStack Root = new(IsRow: true);
	protected nil Render(){
		this.SetContent(Root.Grid, o=>{
			o.SetRowDefs([
				new(7, GUT.Star),
				new(1, GUT.Auto),
				new(5, GUT.Star),
			]);
		});

		var resultPanel = new GridStack(IsRow: true);
		resultPanel
		.A(new AvaPlot(), o=>{
			Ctx?.GraphChanged += (s, e)=>{
				DrawPlot(o);
			};
			// 首次進入頁面先套主題。
			DrawPlot(o);
		});

		var bottomPanel = new GridStack(IsRow: true);
		bottomPanel.Grid.SetRowDefs([
			new(1, GUT.Auto),
			new(1, GUT.Star),
		]);
		bottomPanel
		.A(MkPageBar(), o=>{
			o.HAlign(x=>x.Center);
			o.VAlign(x=>x.Top);
		})
		.A(MkCfgPanel());

		Root
		.A(resultPanel.Grid)
		.A(new GridSplitter(), o=>{
			o.GrayBarWith3Dots();
		})
		.A(bottomPanel.Grid)
		;
		return NIL;
	}
}

