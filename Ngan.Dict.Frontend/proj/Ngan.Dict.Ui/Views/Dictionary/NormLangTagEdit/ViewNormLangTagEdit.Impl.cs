namespace Ngan.Dict.Ui.Views.Dictionary.NormLangTagEdit;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Ngan.Dict.Ui.Icons;
using Ngan.Dict.Ui.Infra;
using Ngan.Dict.Ui.Infra.Ctrls;
using Ngan.Dict.Ui.Tools;
using Ngan.Dict.Ui.Views.Dictionary.NormLangTag;
using Ngan.Dict.Ui.Views.Word.WordManage.NormLang.NormLangPage;
using Tsinswreng.Avln.Grid;
using Tsinswreng.CsCore;
using Tsinswreng.CsI18n;
using K = Ngan.Dict.Ui.Infra.I18n.KeysUiI18nCommon;

/// ViewNormLangTagEdit 的實現。
public partial class ViewNormLangTagEdit{
	/// View 只能有無參構造器；Vm 在這裏解析。
	public partial ViewNormLangTagEdit(){
		vm = App.DiOrMk<VmNormLangTagEdit>();
	}

	/// 上面是可滾動的標籤列表，下面是固定操作列。
	protected override partial object Build(VmNormLangTagEdit vm){
		var List = new ItemsControl()
			.With(l => _TagList = l)
			.ItemsSource(vm, x => x.Tags)
			.ItemsPanel(new StackPanel{
				Orientation = Orientation.Vertical,
				Spacing = 6,
				Margin = new Thickness(8),
			})
		;
		List.ItemTemplate = new FuncDataTemplate<VmNormLangTag>((Tag, _) => {
			// 虛擬化回收容器時模板會被以 null 呼叫。
			if(Tag is null){
				return null;
			}
			return MkTagRow(Tag);
		});

		Root.Rows("*,Auto");
		Root.Children(
			new ScrollViewer()
				.HorizontalScrollBarVisibility(ScrollBarVisibility.Disabled)
				.Content(List)
			,MkBottomBar(vm)
		);
		return Root;
	}

	/// 單行編輯：語言碼、母語名、短文字輸入框、上移、下移、刪除。
	protected partial Control MkTagRow(VmNormLangTag Tag){
		var Input = new TextBox()
			.With(t => t.Watermark = Tag.Code)
			.Text(Tag, x => x.Text, BindingMode.TwoWay)
		;
		return new StackGrid(IsRow: false)
			.Cols("Auto,Auto,*,Auto,Auto,Auto")
			.Children(
				new TextBlock()
					.Text(Tag.Code)
					.VerticalAlignment(VAlign.Center)
					.Margin(new Thickness(4))
				,new TextBlock()
					.Text(Tag.NativeName)
					.VerticalAlignment(VAlign.Center)
					.Margin(new Thickness(4))
				,Input
				,new Button()
					.Content("↑")
					.With(b => {
						ToolTip.SetTip(b, Todo.I18n("Move up"));
						b.Click += (_, _) => vm.MoveUp(Tag);
					})
				,new Button()
					.Content("↓")
					.With(b => {
						ToolTip.SetTip(b, Todo.I18n("Move down"));
						b.Click += (_, _) => vm.MoveDown(Tag);
					})
				,new Button()
					.Content(Icons.Delete())
					.With(b => {
						ToolTip.SetTip(b, I[K.Remove]);
						b.Click += (_, _) => vm.Remove(Tag);
					})
			)
		;
	}

	/// 添加與保存。
	protected partial Control MkBottomBar(VmNormLangTagEdit vm){
		var Add = new OpBtn()
			.With(b => {
				_AddBtn = b;
				b.BtnContent = Icons.Add().ToIcon().WithText(I[K.AddNormLang]);
				b._Button.StretchCenter();
				b.SetExe(_ => {
					OpenNormLangSelector();
					return Task.FromResult<nil>(NIL);
				});
			})
		;
		var Save = new OpBtn()
			.With(b => {
				_SaveBtn = b;
				b.BtnContent = Icons.Save().ToIcon().WithText(I[K.Save]);
				b._Button.StretchCenter();
				b._Button.Background = UiCfg.Inst.MainColor;
				b.SetExe(SaveEtBack);
			})
		;
		return new StackGrid(IsRow: false)
			.Cols("*,*")
			.Children(Add, Save)
		;
	}

	/// 打開語言選擇頁，選中後追加到集合。
	protected partial void OpenNormLangSelector(){
		var View = new ViewNormLangPage();
		if(View.Ctx is not null){
			View.Ctx.Input = "";
			View.Ctx.SetSelectMode(Po => {
				vm.Add(Po);
				View.ViewNavi?.Back();
			});
		}
		ViewNavi?.GoTo(ToolView.WithTitle(Todo.I18n("Select shortcut language"), View));
	}

	/// 保存配置並返回。
	protected partial async Task<nil> SaveEtBack(CT Ct){
		await vm.Save(Ct);
		ViewNavi?.Back();
		return NIL;
	}
}
