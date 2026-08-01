namespace Ngaq.Ui.Views.Dictionary.NormLangTagEdit;

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.Ctrls;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views.Dictionary.NormLangTag;
using Ngaq.Ui.Views.Word.WordManage.NormLang.NormLangPage;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsI18n;
using Ctx = VmNormLangTagEdit;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

/// 詞典源語言快捷標籤編輯 View 的函數實現。
public partial class ViewNormLangTagEdit{
	public partial ViewNormLangTagEdit(){
		Ctx = App.DiOrMk<Ctx>();
		Render();
	}

	private partial void Render(){
		this.SetContent(Root.Grid);
		Root.SetRowDefs([
			new(1, GUT.Star),
			new(1, GUT.Auto),
		]);
		Root.A(new ScrollViewer(), Sv=>{
			Sv.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
			Sv.SetContent(new ItemsControl(), L=>{
				TagList = L;
				L.CBind<Ctx>(L.PropItemsSource, X=>X.Tags);
				L.SetItemsPanel(()=>new StackPanel{
					Orientation = Orientation.Vertical,
					Spacing = 6,
					Margin = new(8),
				});
				L.SetItemTemplate<VmNormLangTag>((Tag, _)=>{
					return MkTagRow(Tag);
				});
			});
		})
		.A(MkBottomBar());
	}

	private partial Control MkTagRow(VmNormLangTag Tag){
		var Row = new GridStack(IsRow: false);
		Row.SetColDefs([
			new(1, GUT.Auto),
			new(1, GUT.Star),
			new(1, GUT.Star),
			new(1, GUT.Auto),
			new(1, GUT.Auto),
			new(1, GUT.Auto),
		]);
		Row.A(new TextBlock(), O=>{
			O.Text = Tag.Code;
			O.VerticalAlignment = VAlign.Center;
			O.Margin = new(4);
		})
		.A(new TextBlock(), O=>{
			O.Text = Tag.NativeName;
			O.VerticalAlignment = VAlign.Center;
			O.Margin = new(4);
		})
		.A(new TextBox(), O=>{
			O.Watermark = Tag.Code;
			O.CBind<VmNormLangTag>(O.PropText, X=>X.Text, Mode: BindingMode.TwoWay);
			O.DataContext = Tag;
		})
		.A(new Button(), O=>{
			O.SetContent("↑");
			ToolTip.SetTip(O, Todo.I18n("Move up"));
			O.Click += (_, _)=>Ctx?.MoveUp(Tag);
		})
		.A(new Button(), O=>{
			O.SetContent("↓");
			ToolTip.SetTip(O, Todo.I18n("Move down"));
			O.Click += (_, _)=>Ctx?.MoveDown(Tag);
		})
		.A(new Button(), O=>{
			O.SetContent(Icons.Delete());
			ToolTip.SetTip(O, I[K.Remove]);
			O.Click += (_, _)=>Ctx?.Remove(Tag);
		});
		return Row.Grid;
	}

	private partial Control MkBottomBar(){
		var Bar = new GridStack(IsRow: false);
		Bar.SetColDefs([new(1, GUT.Star), new(1, GUT.Star)]);
		Bar.A(new OpBtn(), O=>{
			AddBtn = O;
			O.BtnContent = Icons.Add().ToIcon().WithText(I[K.AddNormLang]);
			O._Button.StretchCenter();
			O.SetExe(Ct=>{
				OpenNormLangSelector();
				return Task.FromResult<nil>(NIL);
			});
		})
		.A(new OpBtn(), O=>{
			SaveBtn = O;
			O.BtnContent = Icons.Save().ToIcon().WithText(I[K.Save]);
			O._Button.StretchCenter();
			O._Button.Background = UiCfg.Inst.MainColor;
			O.SetExe(SaveEtBack);
		});
		return Bar.Grid;
	}

	private partial void OpenNormLangSelector(){
		var View = new ViewNormLangPage();
		if(View.Ctx is not null){
			View.Ctx.Input = "";
			View.Ctx.SetSelectMode(Po=>{
				Ctx?.Add(Po);
				View.ViewNavi?.Back();
			});
		}
		ViewNavi?.GoTo(ToolView.WithTitle(Todo.I18n("Select shortcut language"), View));
	}

	private async partial Task<nil> SaveEtBack(CT Ct){
		if(Ctx is null){
			return NIL;
		}
		await Ctx.Save(Ct);
		ViewNavi?.Back();
		return NIL;
	}
}
