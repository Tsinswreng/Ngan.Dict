#if false
namespace Ngaq.Ui.StrokeText;
using Avalonia.Input;
public partial class StrokeTextBlock{
	/* -------------- 交互 -------------- */
	protected override void OnKeyDown(KeyEventArgs e) {
		switch (e.Key) {
			case Key.Left when _caretIndex > 0:
				_caretIndex--;
				break;
			case Key.Right when _caretIndex < Text.Length:
				_caretIndex++;
				break;
			case Key.Back when _caretIndex > 0:
				Text = Text.Remove(--_caretIndex, 1);
				break;
			case Key.Delete when _caretIndex < Text.Length:
				Text = Text.Remove(_caretIndex, 1);
				break;
			case Key.Enter:
				Text = Text.Insert(_caretIndex++, Environment.NewLine);
				break;
			default:
				return;
		}
		InvalidateVisual();
		e.Handled = true;
	}

	protected override void OnTextInput(TextInputEventArgs e) {
		Text = Text.Insert(_caretIndex, e.Text);
		_caretIndex += e.Text.Length;
		InvalidateVisual();
		e.Handled = true;
	}



}

#endif
