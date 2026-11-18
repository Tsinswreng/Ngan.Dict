namespace Ngan.Dict.Ui.Views.Dictionary.DictionaryApi;

using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Ngan.Dict.Core.Shared.Word.Models.DictionaryApi;
using Ngan.Dict.Ui;
using Ngan.Dict.Ui.Infra;
using Tsinswreng.Avln.Grid;
using Tsinswreng.CsCore;

/// ViewDictionaryApi 的實現。
public partial class ViewDictionaryApi{
	/// View 只能有無參構造器；Vm 在這裏解析。
	public partial ViewDictionaryApi(){
		vm = App.DiOrMk<VmDictionaryApi>();
	}

	/// 整頁只有一個列表，佔滿全部高度。
	protected override partial object Build(VmDictionaryApi vm){
		var List = new ListBox()
			.With(l => _Words = l)
			.ItemsSource(vm, x => x.Words)
		;
		List.ItemTemplate = new FuncDataTemplate<DictionaryApiWord>((Word, _) => {
			// 虛擬化回收容器時模板會被以 null 呼叫。
			if(Word is null){
				return null;
			}
			return MkItem(Word);
		});
		return new StackGrid(IsRow: true)
			.Rows("*")
			.Children(List)
		;
	}

	/// 單詞條目：詞頭＋音標，然後逐個 meaning 列出詞性與釋義。
	protected partial Control MkItem(DictionaryApiWord Word){
		var Stack = new StackPanel()
			.Orientation(Avalonia.Layout.Orientation.Vertical)
		;
		// 單詞 + 音標
		Stack.Children.Add(new SelectableTextBlock()
			.FontSize(20)
			.FontWeight(FontWeight.Bold)
			.Foreground(Brushes.DarkCyan)
			.Text($"{Word.word}  {Word.phonetics?.FirstOrDefault()?.text ?? ""}")
		);
		// 每個 meaning
		foreach (var M in Word.meanings ?? new List<Meaning>()) {
			Stack.Children.Add(new SelectableTextBlock()
				.FontWeight(FontWeight.SemiBold)
				.Margin(new Avalonia.Thickness(0, 6, 0, 0))
				.Text(M.partOfSpeech ?? "")
			);
			foreach (var D in M.definitions ?? new List<Definition>()) {
				Stack.Children.Add(new SelectableTextBlock()
					.TextWrapping(TextWrapping.Wrap)
					.Text($"• {D.definition}" +
						(string.IsNullOrEmpty(D.example) ? "" : $"\n    E.g. {D.example}"))
				);
			}
		}
		return Stack;
	}
}
