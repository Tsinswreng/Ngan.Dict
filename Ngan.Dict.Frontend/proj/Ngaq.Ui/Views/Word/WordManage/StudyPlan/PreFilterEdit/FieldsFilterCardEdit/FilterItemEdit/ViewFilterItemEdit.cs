namespace Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.FieldsFilterCardEdit.FilterItemEdit;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Ngaq.Ui;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Tools;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;

using Ctx = VmFilterItemEdit;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

/// View for editing a single FilterItem.
public class ViewFilterItemEdit: AppViewBase<Ctx>{

	ComboBox? OperationCombo{get;set;}
	ComboBox? ValueTypeCombo{get;set;}

	public ViewFilterItemEdit(){
		Ctx = App.DiOrMk<Ctx>();
		Render();
	}

	GridStack Root = new(IsRow: true);

	protected nil Render(){
		Content = Root.Grid;
		Root.SetRowDefs([
			new(1, GUT.Star),
			new(1, GUT.Auto),
		]);
		Root.A(MkBody());
		Root.A(MkBottomBar());
		return NIL;
	}

	public nil SyncSelectionFromVm(){
		if(Ctx is null){
			return NIL;
		}
		if(OperationCombo is not null){
			OperationCombo.ItemsSource = Ctx.OperationOptions;
			OperationCombo.SelectedItem = Ctx.SelectedOperation;
		}
		if(ValueTypeCombo is not null){
			ValueTypeCombo.ItemsSource = Ctx.ValueTypeOptions;
			ValueTypeCombo.SelectedItem = Ctx.SelectedValueType;
		}
		return NIL;
	}

	Control MkBody(){
		var sv = new ScrollViewer();
		var root = new StackPanel{
			Spacing = 10,
			Margin = new(10),
		};
		sv.Content = root;

		var top = new GridStack(IsRow:false);
		top.Grid.SetColDefs([
			new(1, GUT.Star),
			new(1, GUT.Star),
		]);
		top.A(MkComboRow(
			I[K.Operation],
			CBE.Mk<Ctx>(x=>x.OperationOptions, Mode: BindingMode.OneWay),
			CBE.Mk<Ctx>(x=>x.SelectedOperation, Mode: BindingMode.TwoWay),
			isOperation: true
		));
		top.A(MkComboRow(
			I[K.ValueType],
			CBE.Mk<Ctx>(x=>x.ValueTypeOptions, Mode: BindingMode.OneWay),
			CBE.Mk<Ctx>(x=>x.SelectedValueType, Mode: BindingMode.TwoWay),
			isOperation: false
		));
		root.Children.Add(top.Grid);
		root.Children.Add(MkInputRow(I[K.ValuesNewlineSeparated], CBE.Mk<Ctx>(x=>x.ValuesText, Mode: BindingMode.TwoWay), AcceptsReturn: true, MaxHeight: 180));
		return sv;
	}

	Control MkBottomBar(){
		var g = new GridStack(IsRow:false);
		g.Grid.SetColDefs([
			new(1, GUT.Star),
			new(1, GUT.Star),
		]);
		g.A(new Button(), o=>{
			o.Content = Icons.Delete().ToIcon().WithText(I[K.Delete]);
			o.Background = UiCfg.Inst.DelBtnBg;
			o.StretchCenter();
			o.Click += (s,e)=>Ctx?.Delete();
		});
		g.A(new Button(), o=>{
			o.HorizontalContentAlignment = HAlign.Center;
			o.Content = Icons.Save().ToIcon().WithText(I[K.Save]);
			o.Background = UiCfg.Inst.MainColor;
			o.StretchCenter();
			o.Click += (s,e)=>Ctx?.Save();
		});
		return g.Grid;
	}

	Control MkInputRow(str Label, IBinding Binding, bool AcceptsReturn = false, double MaxHeight = double.PositiveInfinity){
		var sp = new StackPanel{Spacing = 3};
		sp.Children.Add(new TextBlock{Text = Label});
		var tb = new TextBox{
			AcceptsReturn = AcceptsReturn,
			TextWrapping = AcceptsReturn ? TextWrapping.Wrap : TextWrapping.NoWrap,
			MaxHeight = MaxHeight,
		};
		tb.Bind(TextBox.TextProperty, Binding);
		sp.Children.Add(tb);
		return sp;
	}

	Control MkComboRow(str Label, IBinding ItemsBinding, IBinding SelectedItemBinding, bool isOperation){
		var sp = new StackPanel{Spacing = 3};
		sp.Children.Add(new TextBlock{Text = Label});
		var cb = new ComboBox();
		cb.Bind(ComboBox.ItemsSourceProperty, ItemsBinding);
		cb.Bind(ComboBox.SelectedItemProperty, SelectedItemBinding);
		if(isOperation){
			OperationCombo = cb;
		}else{
			ValueTypeCombo = cb;
		}
		sp.Children.Add(cb);
		return sp;
	}
}

