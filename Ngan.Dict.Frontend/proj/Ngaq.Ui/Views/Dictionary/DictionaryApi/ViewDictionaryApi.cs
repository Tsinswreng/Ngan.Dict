namespace Ngaq.Ui.Views.Dictionary.DictionaryApi;

using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Ngaq.Core.Shared.Word.Models.DictionaryApi;
using Ngaq.Ui;
using Ngaq.Ui.Infra;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsI18n;
using Ctx = VmDictionaryApi;
public partial class ViewDictionaryApi
	: AppViewBase<Ctx>
{

	public ViewDictionaryApi() {
		Ctx = App.DiOrMk<Ctx>();
		Style();
		Render();
	}

	public partial class Cls {

	}

	protected nil Style() {
		return NIL;
	}

	GridStack Root = new(IsRow: true);

	SelectableTextBlock Txt(){
		return new();
	}

	public void Render() {
		// 整頁只有一個列表，高度自適應
		Root.SetRowDefs([
			new(1, GUT.Star)
		]);
		this.Content = Root.Grid;

		// ListBox 綁定 Words
		Root.A(new ListBox(), lst => {
			lst.Bind(ItemsControl.ItemsSourceProperty, CBE.Mk<Ctx>(x => x.Words));
			// 每條目自己長什麼樣
			lst.ItemTemplate = new FuncDataTemplate<DictionaryApiWord>((word, _) => {
				var itemPanel = new GridStack(IsRow: true);

				// 單詞 + 音標
				itemPanel.A(Txt(), o=>{
					o.FontSize = 20;
					o.FontWeight = Avalonia.Media.FontWeight.Bold;
					o.Foreground = Brushes.DarkCyan;
					o.Text = $"{word.word}  {word.phonetics?.FirstOrDefault()?.text ?? ""}";
				});

				// 每個 meaning
				foreach (var m in word.meanings ?? new List<Meaning>()) {
					itemPanel.A(Txt(), o=>{
						o.FontWeight = FontWeight.SemiBold;
						o.Margin = new(0, 6, 0, 0);
						o.Text = m.partOfSpeech ?? "" ;
					});

					foreach (var d in m.definitions ?? new List<Definition>()) {
						itemPanel.A(Txt(), o=>{
							o.TextWrapping = TextWrapping.Wrap;
							o.Text = $"• {d.definition}" +
								(string.IsNullOrEmpty(d.example) ? "" : $"\n    E.g. {d.example}");
						});
					}
				}

				return itemPanel.Grid;
			});
		});
	}


}
/*
我現在調後端接口拿到了IList<DictionaryApiWord>
你把拿到的數據顯示出來
 */
