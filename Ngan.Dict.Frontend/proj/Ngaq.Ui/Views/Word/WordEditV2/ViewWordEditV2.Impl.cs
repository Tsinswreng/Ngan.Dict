namespace Ngaq.Ui.Views.Word.WordEditV2;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Ngaq.Ui;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.Ctrls;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views.Word.WordEditV2.PoWordEdit;
using Ngaq.Ui.Views.Word.WordEditV2.WordLearnEdit;
using Ngaq.Ui.Views.Word.WordEditV2.WordLearnPage;
using Ngaq.Ui.Views.Word.WordEditV2.WordPropEdit;
using Ngaq.Ui.Views.Word.WordEditV2.WordPropPage;
using Tsinswreng.Avln.Grid;
using Tsinswreng.Avln.Dsl;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Ctx = VmWordEditV2;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

public partial class ViewWordEditV2{
	public partial ViewWordEditV2(){
		Ctx = App.DiOrMk<Ctx>();
		Render();
		HookSubPageEvents();
	}

	partial void Render(){
		this.SetContent(Root.Grid);
		Root.SetRowDefs([new(9, GUT.Star), new(1, GUT.Auto)]);
		Root.A(MkTabs()).A(MkErrBar());
	}

	partial void HookSubPageEvents(){
		if(Ctx is null){
			return;
		}
		Ctx.WordPropPage.OnEditRequested += Row=>{
			Row.Raw.WordId = Ctx.Draft?.Word.Id ?? Row.Raw.WordId;
			var EditVm = Ctx.MkPropEdit();
			EditVm.Row = Row;
			EditVm.Deleted += deletedRow => {
				Ctx.WordPropPage.RemovePersistedRow(deletedRow);
				Ctx.SyncDeletedPropToLocalState(deletedRow.Raw.Id);
			};
			var EditView = new ViewWordPropEdit{Ctx = EditVm};
			ViewNavi?.GoTo(ToolView.WithTitle(I[K.EditProp], EditView));
		};
		Ctx.WordLearnPage.OnEditRequested += Row=>{
			Row.Raw.WordId = Ctx.Draft?.Word.Id ?? Row.Raw.WordId;
			var EditVm = Ctx.MkLearnEdit();
			EditVm.Row = Row;
			EditVm.Deleted += deletedRow => {
				Ctx.WordLearnPage.RemovePersistedRow(deletedRow);
				Ctx.SyncDeletedLearnToLocalState(deletedRow.Raw.Id);
			};
			var EditView = new ViewWordLearnEdit{Ctx = EditVm};
			ViewNavi?.GoTo(ToolView.WithTitle(I[K.EditLearn], EditView));
		};
	}

	private partial Control MkTabs(){
		var Tab = new TabControl();
		var PoWordEdit = new ViewPoWordEdit{Ctx = Ctx?.PoWordEdit};
		this.PoWordEdit = PoWordEdit;
		PoWordEdit.DeleteBtn?.SetExe(Ct=>Ctx?.Delete(Ct));
		PoWordEdit.SaveBtn?.SetExe(Ct=>Ctx?.Save(Ct));
		Tab.Bind(Tab.PropSelectedIndex, CBE.Mk<Ctx>(X=>X.TabIndex, Mode: BindingMode.TwoWay));
		Tab.Items.A(new TabItem(), O=>{
			O.Header = I[K.Basic];
			O.SetContent(PoWordEdit);
		}).A(new TabItem(), O=>{
			O.Header = I[K.Props];
			O.SetContent(new ViewWordPropPage{Ctx = Ctx?.WordPropPage});
		}).A(new TabItem(), O=>{
			O.Header = I[K.Learns];
			O.SetContent(new ViewWordLearnPage{Ctx = Ctx?.WordLearnPage});
		});
		return Tab;
	}

	private partial Control MkErrBar(){
		var Border = new Border{Background = new SolidColorBrush(Color.FromArgb(70, 180, 20, 20)), Padding = new(10, 6), IsVisible = false};
		Border.Bind(IsVisibleProperty, CBE.Mk<Ctx>(X=>X.HasError, Mode: BindingMode.OneWay));
		Border.SetChild(new TextBlock{Foreground = Brushes.White}, Text=>{
			Text.Bind(TextBlock.TextProperty, CBE.Mk<Ctx>(X=>X.LastError, Mode: BindingMode.OneWay));
		});
		return Border;
	}
}
