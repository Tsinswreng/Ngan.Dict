using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;

namespace Ngaq.Ui.Views;

public partial class MainView {
	void Try() {
		return;
		var editor = new TextEditor {
			Text = "{\n  \"name\": \"Ngaq\",\n  \"enabled\": true,\n  \"items\": [\n    1,\n    2,\n    3\n  ]\n}",
			ShowLineNumbers = true,
			FontFamily = new FontFamily("Consolas"),
			FontSize = 14,
			Foreground = Brushes.White,
			HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
			VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
		};
		editor.TextArea.TextView.LineTransformers.Add(new JsonColorizer());
		editor.Options.ConvertTabsToSpaces = true;
		editor.Options.IndentationSize = 2;
		editor.Options.EnableHyperlinks = false;
		editor.Options.EnableEmailHyperlinks = false;

		var window = new Window {
			Title = "AvaloniaEdit JSON Demo",
			Width = 900,
			Height = 600,
			Content = editor,
		};

		window.Show();
	}

	sealed class JsonColorizer : DocumentColorizingTransformer {
		static readonly Regex StringRegex = new("\"(?:\\\\.|[^\"\\\\])*\"", RegexOptions.Compiled);
		static readonly Regex NumberRegex = new("-?(?:0|[1-9]\\d*)(?:\\.\\d+)?(?:[eE][+-]?\\d+)?", RegexOptions.Compiled);
		static readonly Regex BooleanRegex = new("\\btrue\\b|\\bfalse\\b", RegexOptions.Compiled);
		static readonly Regex NullRegex = new("\\bnull\\b", RegexOptions.Compiled);

		protected override void ColorizeLine(DocumentLine line) {
			var text = CurrentContext.Document.GetText(line);
			Colorize(line, text, StringRegex, Brushes.Orange);
			Colorize(line, text, NumberRegex, Brushes.LightGreen);
			Colorize(line, text, BooleanRegex, Brushes.DeepSkyBlue);
			Colorize(line, text, NullRegex, Brushes.Gray);
		}

		void Colorize(DocumentLine line, string text, Regex regex, IBrush brush) {
			foreach(Match match in regex.Matches(text)) {
				var start = line.Offset + match.Index;
				var end = start + match.Length;
				ChangeLinePart(start, end, element => element.TextRunProperties.SetForegroundBrush(brush));
			}
		}
	}
}
