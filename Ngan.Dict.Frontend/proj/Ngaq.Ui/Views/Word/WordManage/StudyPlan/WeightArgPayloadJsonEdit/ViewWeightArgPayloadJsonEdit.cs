namespace Ngaq.Ui.Views.Word.WordManage.StudyPlan.WeightArgPayloadJsonEdit;

using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using AvaloniaEdit;
using Ngaq.Ui;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.Ctrls;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;

using Ctx = VmWeightArgPayloadJsonEdit;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

/// WeightArg Payload(JSON) 編輯視圖。
public class ViewWeightArgPayloadJsonEdit: AppViewBase<Ctx>{

	public ViewWeightArgPayloadJsonEdit(){
		Ctx = App.DiOrMk<Ctx>();
		Render();
	}

	GridStack Root = new(IsRow: true);
	TextEditor? PayloadEditor;
	bool IsSyncingPayloadText = false;
	protected nil Render(){
		Content = Root.Grid;
		Root.SetRowDefs([
			new(1, GUT.Auto),
			new(1, GUT.Star),
			new(1, GUT.Auto),
		]);
		Root.A(MkErrorBar());
		Root.A(JsonText());
		Root.A(MkBottomBar());
		return NIL;
	}

	//不符代碼規範
	Control MkErrorBar(){
		var b = new Border{
			Background = new SolidColorBrush(Color.FromArgb(80, 180, 30, 30)),
			Padding = new(10, 6),
			IsVisible = false,
			Margin = new(10, 10, 10, 4),
		};
		b.CBind<Ctx>(IsVisibleProperty, x=>x.HasError, Mode: BindingMode.OneWay);
		var txt = new TextBlock{
			Foreground = Brushes.White,
		};
		txt.CBind<Ctx>(TextBlock.TextProperty, x=>x.LastError, Mode: BindingMode.OneWay);
		b.Child = txt;
		return b;
	}

	Control MkBottomBar(){
		var g = new GridStack(IsRow:false);
		g.Grid.ColumnDefinitions.Add(new(1, GUT.Star));
		g.A(new OpBtn(), o=>{
			o._Button.HorizontalContentAlignment = HAlign.Center;
			o.BtnContent = Icons.Save().ToIcon().WithText(I[K.Apply]);
			o._Button.Background = UiCfg.Inst.MainColor;
			o.SetExe((Ct)=>{
				Ctx?.ApplyAndBack();
				return Task.FromResult(NIL);
			});
		});
		return g.Grid;
	}

	Control JsonText(){
		var editor = JsonTextEditorCtrl.Mk(Ctx?.PayloadJson, IsReadOnly: false, MinHeight: 220);
		editor.Margin = new(10, 4, 10, 10);
		PayloadEditor = editor;
		editor.TextChanged += (s,e)=>{
			if(IsSyncingPayloadText || Ctx is null){
				return;
			}
			IsSyncingPayloadText = true;
			Ctx.PayloadJson = editor.Text;
			IsSyncingPayloadText = false;
		};
		if(Ctx is not null){
			Ctx.PropertyChanged += OnVmPropertyChanged;
		}
		DetachedFromVisualTree += (s,e)=>{
			if(Ctx is not null){
				Ctx.PropertyChanged -= OnVmPropertyChanged;
			}
		};
		return editor;
	}

	void OnVmPropertyChanged(object? Sender, PropertyChangedEventArgs E){
		if(E.PropertyName != nameof(Ctx.PayloadJson) || PayloadEditor is null || Ctx is null || IsSyncingPayloadText){
			return;
		}
		if(PayloadEditor.Text == Ctx.PayloadJson){
			return;
		}
		IsSyncingPayloadText = true;
		PayloadEditor.Text = Ctx.PayloadJson;
		IsSyncingPayloadText = false;
	}
}



