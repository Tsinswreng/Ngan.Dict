namespace Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterEdit.PreFilterJsonEdit;

using System.ComponentModel;
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

using Ctx = VmPreFilterJsonEdit;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;


/// PoPreFilter JSON 專用編輯視圖。

public class ViewPreFilterJsonEdit: AppViewBase<Ctx>{

	public ViewPreFilterJsonEdit(){
		Ctx = App.DiOrMk<Ctx>();
		Render();
	}

	GridStack Root = new(IsRow: true);
	TextEditor? JsonEditor;
	bool IsSyncingJsonText = false;
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
		g.Grid.SetColDefs([
			new(1, GUT.Star),
			new(1, GUT.Star),
			new(1, GUT.Star),
		]);
		g.A(new Button(), o=>{
			o.Content = I[K.Back];
			o.Click += (s,e)=>ViewNavi?.Back();
		});
		g.A(new OpBtn(), o=>{
			o._Button.HorizontalContentAlignment = HAlign.Center;
			o.BtnContent = Icons.Delete().ToIcon().WithText(I[K.Delete]);
			o._Button.Background = UiCfg.Inst.DelBtnBg;
			o.SetExe((Ct)=>Ctx?.Delete(Ct));
		});
		g.A(new OpBtn(), o=>{
			o._Button.HorizontalContentAlignment = HAlign.Center;
			o.BtnContent = Icons.Save().ToIcon().WithText(I[K.Save]);
			o._Button.Background = UiCfg.Inst.MainColor;
			o.SetExe((Ct)=>Ctx?.Save(Ct));
		});
		return g.Grid;
	}

	Control JsonText(){
		var editor = JsonTextEditorCtrl.Mk(Ctx?.PoPreFilterJson, IsReadOnly: false, MinHeight: 220);
		editor.Margin = new(10, 4, 10, 10);
		JsonEditor = editor;
		editor.TextChanged += (s,e)=>{
			if(IsSyncingJsonText || Ctx is null){
				return;
			}
			IsSyncingJsonText = true;
			Ctx.PoPreFilterJson = editor.Text;
			IsSyncingJsonText = false;
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
		if(E.PropertyName != nameof(Ctx.PoPreFilterJson) || JsonEditor is null || Ctx is null || IsSyncingJsonText){
			return;
		}
		if(JsonEditor.Text == Ctx.PoPreFilterJson){
			return;
		}
		IsSyncingJsonText = true;
		JsonEditor.Text = Ctx.PoPreFilterJson;
		IsSyncingJsonText = false;
	}
}

