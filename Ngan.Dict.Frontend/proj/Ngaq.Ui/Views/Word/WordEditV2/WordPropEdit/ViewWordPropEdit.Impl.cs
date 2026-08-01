namespace Ngaq.Ui.Views.Word.WordEditV2.WordPropEdit;

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using Ngaq.Core.Model.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views.Word.WordEditV2.WordPropPage;
using Tsinswreng.Avln.Grid;
using Tsinswreng.Avln.Dsl;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;
using Ctx = VmWordPropEdit;
using Ngaq.Ui.Infra.Ctrls;

/// 單行屬性編輯頁。
public partial class ViewWordPropEdit: AppViewBase<Ctx>{
	public partial ViewWordPropEdit(){
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
				sp.A(MkIdSelectableRow(I[K.Id], CBE.Mk<VmWordPropRow>(x=>x.IdText, Mode: BindingMode.OneWay), o=>{
					IdCtrl = o;
				}));
				sp.A(MkComboRow(I[K.KType], KvTypeOptions, CBE.Mk<VmWordPropRow>(x=>x.KTypeIndex, Mode: BindingMode.TwoWay), o=>{
					KTypeCtrl = o;
				}));
				sp.A(MkEditableComboRow(
					I[K.KeyStr],
					PropKeyDisplayOptions,
					CBE.Mk<VmWordPropRow>(
						x=>x.KStrText,
						Mode: BindingMode.TwoWay,
						Converter: new PropKeyDisplayConverter(this)
					),
					o=>{
						KStrCtrl = o;
					}
				));
				sp.A(MkInputRow(I[K.KeyI64], CBE.Mk<VmWordPropRow>(x=>x.KI64Text, Mode: BindingMode.TwoWay), o=>{
					KI64Ctrl = o;
				}));
				sp.A(MkComboRow(I[K.VType], KvTypeOptions, CBE.Mk<VmWordPropRow>(x=>x.VTypeIndex, Mode: BindingMode.TwoWay), o=>{
					VTypeCtrl = o;
				}));
				sp.A(MkInputRow(I[K.VStr], CBE.Mk<VmWordPropRow>(x=>x.VStrText, Mode: BindingMode.TwoWay), o=>{
					VStrCtrl = o;
				}));
				sp.A(MkInputRow(I[K.VI64], CBE.Mk<VmWordPropRow>(x=>x.VI64Text, Mode: BindingMode.TwoWay), o=>{
					VI64Ctrl = o;
				}));
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
				try{
					await Ctx.DelDirect();
					Ctx.OnDeletedByView();
					ViewNavi?.Back();
				}catch(Exception ex){
					// let VM handle error display
				}
			};
		}).A(new Button(), o=>{
			o.StretchCenter();
			o.Background = UiCfg.Inst.MainColor;
			o.SetContent(Icons.Save().ToIcon().WithText(I[K.Save]));
			o.Click += async (s, e)=>{
				if(Ctx is null) return;
				try{
					await Ctx.SaveDirect();
					ViewNavi?.Back();
				}catch(Exception ex){
					// let VM handle error display
				}
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
		if(E.PropertyName == nameof(VmWordPropEdit.Row)){
			ApplyRowCtx();
		}
	}
	partial void ApplyRowCtx(){
		if(EditorForm is not null){
			EditorForm.DataContext = Ctx?.Row;
		}
	}
	private partial Control MkIdSelectableRow(str Label, IBinding Binding, Action<SelectableTextBlock> Init){
		var tb = new SelectableTextBlock{
			FontSize = UiCfg.Inst.BaseFontSize*0.8,
			TextWrapping = TextWrapping.Wrap,
		};
		Init(tb);
		tb.Bind(TextBlock.TextProperty, Binding);
		return MkFieldRow(Label, tb);
	}
	private partial Control MkInputRow(str Label, IBinding Binding, Action<TextBox> Init){
		var tb = new TextBox();
		Init(tb);
		tb.Bind(TextBox.TextProperty, Binding);
		return MkFieldRow(Label, tb);
	}
	private partial Control MkComboRow(str Label, IEnumerable<str> Items, IBinding Binding, Action<ComboBox> Init){
		var cb = new ComboBox{ItemsSource = Items};
		Init(cb);
		cb.Bind(ComboBox.SelectedIndexProperty, Binding);
		return MkFieldRow(Label, cb);
	}
	private partial Control MkEditableComboRow(str Label, IEnumerable<str> Items, IBinding Binding, Action<ComboBox> Init){
		var cb = new ComboBox{IsEditable = true, ItemsSource = Items};
		Init(cb);
		cb.Bind(ComboBox.TextProperty, Binding);
		return MkFieldRow(Label, cb);
	}
	private partial Control MkFieldRow(str Label, Control Input){
		var sp = new StackPanel{Orientation = Orientation.Vertical, Spacing = 3};
		sp.A(new TextBlock(), o=>{
			o.Text = Label;
		}).A(Input);
		return sp;
	}
	sealed class PropKeyDisplayConverter: IValueConverter{
		readonly ViewWordPropEdit Host;
		public PropKeyDisplayConverter(ViewWordPropEdit Host){
			this.Host = Host;
		}

		public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture){
			if(value is not str raw){
				return "";
			}
			return Host.ToDisplayPropKey(raw);
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture){
			if(value is not str display){
				return "";
			}
			return Host.ToStoredPropKey(display);
		}
	}
	private partial str ToDisplayPropKey(str RawKey){
		return RawKey switch{
			var x when str.Equals(x, VmWordPropRow.DescriptionAlias, StringComparison.OrdinalIgnoreCase) => I[K.Descr],
			var x when x == KeysProp.Inst.summary => I[K.Summary],
			var x when x == KeysProp.Inst.description => I[K.Descr],
			var x when x == KeysProp.Inst.note => I[K.Note],
			var x when x == KeysProp.Inst.tag => I[K.Tag],
			var x when x == KeysProp.Inst.source => I[K.Source],
			var x when x == KeysProp.Inst.alias => I[K.Alias],
			var x when x == KeysProp.Inst.pronunciation => I[K.Pronunciation],
			var x when x == KeysProp.Inst.weight => I[K.Weight],
			var x when x == KeysProp.Inst.learn => I[K.Learn],
			var x when x == KeysProp.Inst.usage => I[K.Usage],
			var x when x == KeysProp.Inst.example => I[K.Example],
			var x when x == KeysProp.Inst.relation => I[K.Relation],
			var x when x == KeysProp.Inst.Ref => I[K.Ref],
			_ => RawKey,
		};
	}
	private partial str ToStoredPropKey(str DisplayKey){
		var trimmed = DisplayKey.Trim();
		if(trimmed == I[K.Descr]){
			return VmWordPropRow.DescriptionAlias;
		}
		if(trimmed == I[K.Summary]){
			return KeysProp.Inst.summary;
		}
		if(trimmed == I[K.Note]){
			return KeysProp.Inst.note;
		}
		if(trimmed == I[K.Tag]){
			return KeysProp.Inst.tag;
		}
		if(trimmed == I[K.Source]){
			return KeysProp.Inst.source;
		}
		if(trimmed == I[K.Alias]){
			return KeysProp.Inst.alias;
		}
		if(trimmed == I[K.Pronunciation]){
			return KeysProp.Inst.pronunciation;
		}
		if(trimmed == I[K.Weight]){
			return KeysProp.Inst.weight;
		}
		if(trimmed == I[K.Learn]){
			return KeysProp.Inst.learn;
		}
		if(trimmed == I[K.Usage]){
			return KeysProp.Inst.usage;
		}
		if(trimmed == I[K.Example]){
			return KeysProp.Inst.example;
		}
		if(trimmed == I[K.Relation]){
			return KeysProp.Inst.relation;
		}
		if(trimmed == I[K.Ref]){
			return KeysProp.Inst.Ref;
		}
		return DisplayKey;
	}
}
