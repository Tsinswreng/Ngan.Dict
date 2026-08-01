namespace Ngaq.Ui.Infra.Ctrls;

using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;

/// Reusable JSON text editor based on the MainView.Try sample settings.
/// 注意 TextEditor Text 不可綁定
public static class JsonTextEditorCtrl{
	/// Build a JSON editor with the same visual behavior as MainView.Try.
	public static TextEditor Mk(str? Text = null, bool IsReadOnly = false, double MinHeight = 160){
		var editor = new TextEditor{
			Text = Text ?? "",
			ShowLineNumbers = true,
			FontFamily = new FontFamily("Consolas"),
			FontSize = 14,
			Foreground = Brushes.White,
			HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
			VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
			IsReadOnly = IsReadOnly,
			MinHeight = MinHeight,
		};
		editor.TextArea.TextView.LineTransformers.Add(new JsonColorizer());
		editor.Options.ConvertTabsToSpaces = true;
		editor.Options.IndentationSize = 2;
		editor.Options.EnableHyperlinks = false;
		editor.Options.EnableEmailHyperlinks = false;
		editor.Margin = new(0);
		return editor;
	}

	sealed class JsonColorizer: DocumentColorizingTransformer{
		static readonly Regex StringRegex = new("\"(?:\\\\.|[^\"\\\\])*\"", RegexOptions.Compiled);
		static readonly Regex NumberRegex = new("-?(?:0|[1-9]\\d*)(?:\\.\\d+)?(?:[eE][+-]?\\d+)?", RegexOptions.Compiled);
		static readonly Regex BooleanRegex = new("\\btrue\\b|\\bfalse\\b", RegexOptions.Compiled);
		static readonly Regex NullRegex = new("\\bnull\\b", RegexOptions.Compiled);

		protected override void ColorizeLine(DocumentLine line){
			var text = CurrentContext.Document.GetText(line);
			Colorize(line, text, StringRegex, Brushes.Orange);
			Colorize(line, text, NumberRegex, Brushes.LightGreen);
			Colorize(line, text, BooleanRegex, Brushes.DeepSkyBlue);
			Colorize(line, text, NullRegex, Brushes.Gray);
		}

		void Colorize(DocumentLine line, string text, Regex regex, IBrush brush){
			foreach(Match match in regex.Matches(text)){
				var start = line.Offset + match.Index;
				var end = start + match.Length;
				ChangeLinePart(start, end, element=>element.TextRunProperties.SetForegroundBrush(brush));
			}
		}
	}
}
