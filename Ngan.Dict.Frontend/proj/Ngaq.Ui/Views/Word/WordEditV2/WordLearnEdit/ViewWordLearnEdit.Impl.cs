namespace Ngaq.Ui.Views.Word.WordEditV2.WordLearnEdit;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using Ngaq.Ui.Components.TempusBox;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views.Word.WordEditV2.WordLearnPage;
using Tsinswreng.Avln.Grid;
using Tsinswreng.Avln.Dsl;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsTempus;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

/// 單行學習記錄編輯頁。
public partial class ViewWordLearnEdit: AppViewBase{
	public partial ViewWordLearnEdit(){
		Render();
		DataContextChanged += (s, e)=>OnCtxChanged();
	}
	partial void Render(){
		var root = new GridStack(IsRow: true);
		root.Grid.SetRowDefs([
			new(8, GUT.Star),
			new(1, GUT.Auto),
		]);
		root.A(new ScrollViewer(), sv=>{
			sv.SetContent(new StackPanel(), sp=>{
				EditorForm = sp;
				sp.Margin = new(10);
				sp.Spacing = 8;
				sp.A(MkIdSelectableRow(I[K.Id], CBE.Mk<VmWordLearnRow>(x=>x.IdText, Mode: BindingMode.OneWay)));
				sp.A(MkComboRow(I[K.LearnResult], LearnResultOptions, CBE.Mk<VmWordLearnRow>(x=>x.LearnResultIndex, Mode: BindingMode.TwoWay)));
				sp.A(MkTempusRow(I[K.Biz_CreatedAt], CBE.Mk<VmWordLearnRow>(x=>x.BizCreatedAtIso, Mode: BindingMode.TwoWay, Converter: new IsoToTempusConverter())));
			});
		});
		var bar = new GridStack(IsRow: false);
		bar.Grid.SetColDefs([
			new(1, GUT.Star),
			new(1, GUT.Star),
		]);
		bar.Grid.Margin = new(10, 6, 10, 10);
		bar.A(new Button(), o=>{
			o.StretchCenter();
			o.Background = UiCfg.Inst.DelBtnBg;
			o.SetContent(Icons.Delete().ToIcon().WithText(I[K.Remove]));
			o.Click += async (s, e)=>{
				if(Ctx is null || Ctx.Row.DmlState == EDmlState.Added){
					if(Ctx is not null) Ctx.OnDeletedByView();
					ViewNavi?.Back();
					return;
				}
				await Ctx.DelDirect();
				Ctx.OnDeletedByView();
				ViewNavi?.Back();
			};
		}).A(new Button(), o=>{
			o.StretchCenter();
			o.Background = UiCfg.Inst.MainColor;
			o.SetContent(Icons.Save().ToIcon().WithText(I[K.Save]));
			o.Click += async (s, e)=>{
				if(Ctx is null) return;
				await Ctx.SaveDirect();
				ViewNavi?.Back();
			};
		});
		root.A(bar.Grid);
		this.SetContent(root.Grid);
	}
	partial void OnCtxChanged(){
		if(SubscribedCtx is not null){
			SubscribedCtx.PropertyChanged -= OnEditVmPropertyChanged;
			SubscribedCtx = null;
		}
		if(Ctx is null){
			if(EditorForm is not null){
				EditorForm.DataContext = null;
			}
			return;
		}
		SubscribedCtx = Ctx;
		Ctx.PropertyChanged += OnEditVmPropertyChanged;
		ApplyRowCtx();
	}
	partial void OnEditVmPropertyChanged(object? Sender, System.ComponentModel.PropertyChangedEventArgs E){
		if(E.PropertyName == nameof(VmWordLearnEdit.Row)){
			ApplyRowCtx();
		}
	}
	partial void ApplyRowCtx(){
		if(EditorForm is not null){
			EditorForm.DataContext = Ctx?.Row;
		}
	}
	private partial Control MkIdSelectableRow(str Label, IBinding Binding){
		var tb = new SelectableTextBlock{
			FontSize = UiCfg.Inst.BaseFontSize*0.8,
			TextWrapping = TextWrapping.Wrap,
		};
		tb.Bind(TextBlock.TextProperty, Binding);
		return MkFieldRow(Label, tb);
	}
	private partial Control MkComboRow(str Label, IEnumerable<str> Items, IBinding Binding){
		var cb = new ComboBox{ItemsSource = Items};
		cb.Bind(ComboBox.SelectedIndexProperty, Binding);
		return MkFieldRow(Label, cb);
	}
	private partial Control MkTempusRow(str Label, IBinding Binding){
		var tb = new TempusBox();
		tb.Bind(TempusBox.TempusProperty, Binding);
		tb.FormatItems.Add(TempusFormatItem.yy_MM_DD);
		tb.FormatItems.Add(TempusFormatItem.yy_MM_DD__HH_mm);
		tb.SelectedFormat = TempusFormatItem.yy_MM_DD__HH_mm;
		return MkFieldRow(Label, tb);
	}
	private partial Control MkFieldRow(str Label, Control Input){
		var sp = new StackPanel{Orientation = Orientation.Vertical, Spacing = 3};
		sp.A(new TextBlock(), o=>{
			o.Text = Label;
		}).A(Input);
		return sp;
	}
	sealed class IsoToTempusConverter: IValueConverter{
		public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture){
			if(value is str s && !str.IsNullOrWhiteSpace(s)){
				try{return UnixMs.FromIso(s.Trim());}catch{}
			}
			return UnixMs.Now();
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture){
			if(value is UnixMs t){
				return t.ToIso();
			}
			return "";
		}
	}
}
