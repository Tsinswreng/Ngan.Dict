namespace Ngan.Dict.Ui.Views.Dictionary.SimpleWord;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using Ngan.Dict.Core.Shared.Dictionary.Models;
using Ngan.Dict.Ui;
using Ngan.Dict.Ui.Icons;
using Ngan.Dict.Ui.Infra;
using Ngan.Dict.Ui.Infra.Ctrls;
using Tsinswreng.Avln.Grid;
using Tsinswreng.CsCore;

/// ViewSimpleWord 的實現。
public partial class ViewSimpleWord{

	/// View 只能有無參構造器；Vm 在這裏解析，控件樹交給 Declarative 的 `Build`。
	public partial ViewSimpleWord(){
		vm = App.DiOrMk<VmSimpleWord>();
	}

	/// 結果內容縱向排列（詞頭／讀音／釋義），右側預留複製按鈕的位置。
	protected override partial object Build(VmSimpleWord vm){
		var Content = new StackGrid(IsRow: true)
			.Rows("Auto,Auto,Auto")
			.Margin(new Thickness(0, 0, UiCfg.Inst.BaseFontSize*2.2, 0))
			.Children(
				Txt()
					.With(t => {
						_Head = t;
						t.FontSize = UiCfg.Inst.BaseFontSize*1.5;
					})
					.Text(vm, x => x.Head)
				,MkPronunciations(vm)
				,Txt()
					.With(t => _Description = t)
					.Text(vm, x => x.Description)
			)
		;

		// 根是疊放：內容本體 ＋ 右上角複製按鈕。
		return new Grid().Children(
			Content
			,new OpBtn()
				.With(b => {
					_CopyDescriptionBtn = b;
					b.BtnContent = Icons.Copy().ToIcon();
					b._Button.Width = UiCfg.Inst.BaseFontSize*2;
					b._Button.Height = UiCfg.Inst.BaseFontSize*2;
					b._Button.HorizontalAlignment = HAlign.Right;
					b._Button.VerticalAlignment = VAlign.Top;
					b.HorizontalAlignment = HAlign.Right;
					b.VerticalAlignment = VAlign.Top;
					b.ZIndex = 1;
					b.SetExe(Ct => vm.CopyDescription(Ct));
				})
		);
	}

	/// 建立允許選取且自動換行的文本控件。
	protected partial SelectableTextBlock Txt(){
		return new SelectableTextBlock().TextWrapping(TextWrapping.Wrap);
	}

	/// 讀音列表：橫排固定兩列，每行一個讀音。
	protected partial Control MkPronunciations(VmSimpleWord vm){
		var List = new ItemsControl()
			.With(l => _Pronunciations = l)
			.ItemsSource(vm, x => x.Pronunciations)
			.ItemsPanel(new StackPanel{
				Orientation = Orientation.Vertical,
				HorizontalAlignment = HAlign.Left,
			})
		;
		List.ItemTemplate = new FuncDataTemplate<Pronunciation>((P, _) => {
			// 虛擬化回收容器時模板會被以 null 呼叫。
			if(P is null){
				return null;
			}
			return MkPronunciationRow(vm, P);
		});
		return List;
	}

	/// 單行讀音：左邊播放按鈕、右邊讀音文本。
	protected partial Control MkPronunciationRow(VmSimpleWord vm, Pronunciation P){
		return new StackGrid(IsRow: false)
			.Cols("Auto,Auto")
			.Children(
				new OpBtn()
					.With(b => {
						var Icon = Icons.VolHigh().ToIcon();
						Icon.Height = UiCfg.Inst.BaseFontSize*0.8;
						Icon.Width = UiCfg.Inst.BaseFontSize*0.8;
						b.BtnContent = Icon;
						// 按鈕保持方形尺寸，且靠左不拉伸。
						b._Button.Width = UiCfg.Inst.BaseFontSize*1.6;
						b._Button.Height = UiCfg.Inst.BaseFontSize*1.6;
						b._Button.HorizontalAlignment = HAlign.Left;
						b.HorizontalAlignment = HAlign.Left;
						b.SetExe(Ct => vm.PlayHead(Ct));
					})
				,Txt()
					.With(t => {
						t.Text = P.Text;
						t.VerticalAlignment = VAlign.Center;
						t.Margin = new Thickness(UiCfg.Inst.BaseFontSize*0.2, 0, 0, 0);
					})
			)
		;
	}
}
