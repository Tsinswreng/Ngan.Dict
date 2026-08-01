namespace Ngaq.Ui.Views.Word.WordManage.StudyPlan.StudyPlanEdit;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Ngaq.Ui;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.Ctrls;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterPage;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.WeightArgPage;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.WeightCalculatorPage;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsI18n;
using Ctx = VmStudyPlanEdit;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

public partial class ViewStudyPlanEdit
	:AppViewBase<Ctx>
{

	public ViewStudyPlanEdit(){
		Ctx = App.DiOrMk<Ctx>();
		Style();
		Render();
	}
	public partial class Cls{}

	protected nil Style(){
		return NIL;
	}

	GridStack Root = new(IsRow: true);
	protected nil Render(){
		this.Content = Root.Grid;
		Root.SetRowDefs([
			new(1, GUT.Star),
			new(1, GUT.Auto),
		])
		.A(MkBody())
		.A(MkBottomBar());
		return NIL;
	}

	Control MkBody(){
		var sv = new ScrollViewer();
		var root = new StackPanel{
			Spacing = 10,
			Margin = new(10),
		};
		sv.Content = root;
		root.Children.Add(MkErrorBar());
		root.Children.Add(MkPoSection());
		return sv;
	}

	Control MkErrorBar(){
		var b = new Border{
			Background = new SolidColorBrush(Color.FromArgb(80, 180, 30, 30)),
			Padding = new(10, 6),
			IsVisible = false,
		};
		b.CBind<Ctx>(IsVisibleProperty, x=>x.HasError, Mode: BindingMode.OneWay);
		var txt = new TextBlock{
			Foreground = Brushes.White,
		};
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
		bdr.SetChild(new StackPanel{Spacing = 8}, sp=>{
			sp.A(new TextBlock{
				Text = I[K.StudyPlan],
				FontSize = UiCfg.Inst.BaseFontSize * 1.1,
				FontWeight = FontWeight.SemiBold,
			})
			.A(MkIdRow(I[K.Id], CBE.Mk<Ctx>(x=>x.PoIdText, Mode: BindingMode.OneWay)))
			.A(MkInputRow(I[K.Name], CBE.Mk<Ctx>(x=>x.PoUniqName, Mode: BindingMode.TwoWay)))
			.A(MkInputRow(I[K.Description], CBE.Mk<Ctx>(x=>x.PoDescr, Mode: BindingMode.TwoWay), AcceptsReturn: true))
			.A(ToolStudyPlanView.MkInputWithBtnRow(
				I[K.PreFilter],
				CBE.Mk<Ctx>(x=>x.PreFilterUniqNameText, Mode: BindingMode.OneWay),
				Icons.ListSelect().WithText(I[K.Choose]),
				()=>{
					var view = new ViewPreFilterPage();
					view.Ctx?.SetSelectMode(po=>{
						Ctx?.ApplySelectedPreFilter(po);
						view.ViewNavi?.Back();
					});
					ViewNavi?.GoTo(ToolView.WithTitle(I[K.SelectPreFilter], view));
				},
				ReadOnly: true
			))
			.A(ToolStudyPlanView.MkInputWithBtnRow(
				I[K.WeightCalculator],
				CBE.Mk<Ctx>(x=>x.WeightCalculatorUniqNameText, Mode: BindingMode.OneWay),
				Icons.ListSelect().ToIcon().WithText(I[K.Choose]),
				()=>{
					var view = new ViewWeightCalculatorPage();
					view.Ctx?.SetSelectMode(po=>{
						Ctx?.ApplySelectedWeightCalculator(po);
						view.ViewNavi?.Back();
					});
					ViewNavi?.GoTo(ToolView.WithTitle(I[K.SelectWeightCalculator], view));
				},
				ReadOnly: true
			))
			.A(ToolStudyPlanView.MkInputWithBtnRow(
				I[K.WeightArg],
				CBE.Mk<Ctx>(x=>x.WeightArgUniqNameText, Mode: BindingMode.OneWay),
				Icons.ListSelect().ToIcon().WithText(I[K.Choose]),
				()=>{
					var view = new ViewWeightArgPage();
					view.Ctx?.SetSelectMode(po=>{
						Ctx?.ApplySelectedWeightArg(po);
						view.ViewNavi?.Back();
					});
					ViewNavi?.GoTo(ToolView.WithTitle(I[K.SelectWeightArg], view));
				},
				ReadOnly: true
			))
			;
		});
		return bdr;
	}

	Control MkBottomBar(){
		var bar = new GridStack(IsRow:false);
		bar.SetColDefs([
			new(1, GUT.Star),
			new(1, GUT.Star),
		]);
		bar.A(new OpBtn(), o=>{
			o._Button.Background = UiCfg.Inst.DelBtnBg;
			o._Button.HorizontalContentAlignment = HAlign.Center;
			o.BtnContent = Icons.Delete().ToIcon().WithText(I[K.Delete]);
			o.SetExe((Ct)=>Ctx?.Delete(Ct));
		})
		.A(new OpBtn(), o=>{
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

	Control MkIdRow(str Label, IBinding Binding){
		var row = new StackPanel{
			Spacing = 6,
			Orientation = Orientation.Horizontal,
		};
		row.Children.Add(new TextBlock{
			Text = Label + ":",
			FontSize = UiCfg.Inst.BaseFontSize * 0.8,
		});
		var value = new SelectableTextBlock{
			FontSize = UiCfg.Inst.BaseFontSize * 0.8,
		};
		value.Bind(TextBlock.TextProperty, Binding);
		row.Children.Add(value);
		return row;
	}

}



