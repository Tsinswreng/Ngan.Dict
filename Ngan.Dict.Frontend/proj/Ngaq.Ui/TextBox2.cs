
// using System;
// using System.Collections.Generic;
// using System.Diagnostics.CodeAnalysis;
// using System.Linq;
// using System.Text;
// using Avalonia.Automation.Peers;
// using Avalonia.Controls.Metadata;
// using Avalonia.Controls.Presenters;
// using Avalonia.Controls.Primitives;
// using Avalonia.Controls.Utils;
// using Avalonia.Data;
// using Avalonia.Input;
// using Avalonia.Input.Platform;
// using Avalonia.Input.TextInput;
// using Avalonia.Interactivity;
// using Avalonia.Layout;
// using Avalonia.Media;
// using Avalonia.Media.TextFormatting;
// using Avalonia.Media.TextFormatting.Unicode;
// using Avalonia.Metadata;
// using Avalonia.Reactive;
// using Avalonia.Threading;
// using Avalonia.Utilities;

// namespace Avalonia.Controls;

// //
// // 摘要:
// //     Represents a control that can be used to display or edit unformatted text.
// [TemplatePart("PART_TextPresenter", typeof(TextPresenter), IsRequired = true)]
// [TemplatePart("PART_ScrollViewer", typeof(ScrollViewer))]
// [PseudoClasses(new string[] { ":empty" })]
// public class TextBox : TemplatedControl, UndoRedoHelper<TextBox.UndoRedoState>.IUndoRedoHost {
// 	//
// 	// 摘要:
// 	//     Stores the state information for available actions in the UndoRedoHelper
// 	private readonly struct UndoRedoState : IEquatable<UndoRedoState> {
// 		public string? Text { get; }

// 		public int CaretPosition { get; }

// 		public UndoRedoState(string? text, int caretPosition) {
// 			Text = text;
// 			CaretPosition = caretPosition;
// 		}

// 		public bool Equals(UndoRedoState other) {
// 			if ((object)Text != other.Text) {
// 				return object.Equals(Text, other.Text);
// 			}

// 			return true;
// 		}

// 		public override bool Equals(object? obj) {
// 			if (obj is UndoRedoState other) {
// 				return Equals(other);
// 			}

// 			return false;
// 		}

// 		public override int GetHashCode() {
// 			return Text?.GetHashCode() ?? 0;
// 		}
// 	}

// 	private class LineTextSource : ITextSource {
// 		private readonly int _lines;

// 		public LineTextSource(int lines) {
// 			_lines = lines;
// 		}

// 		public TextRun? GetTextRun(int textSourceIndex) {
// 			if (textSourceIndex >= _lines) {
// 				return null;
// 			}

// 			return new TextEndOfLine();
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.IsInactiveSelectionHighlightEnabled property
// 	public static readonly StyledProperty<bool> IsInactiveSelectionHighlightEnabledProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.ClearSelectionOnLostFocus property
// 	public static readonly StyledProperty<bool> ClearSelectionOnLostFocusProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.AcceptsReturn property
// 	public static readonly StyledProperty<bool> AcceptsReturnProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.AcceptsTab property
// 	public static readonly StyledProperty<bool> AcceptsTabProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.CaretIndex property
// 	public static readonly StyledProperty<int> CaretIndexProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.IsReadOnly property
// 	public static readonly StyledProperty<bool> IsReadOnlyProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.PasswordChar property
// 	public static readonly StyledProperty<char> PasswordCharProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.SelectionBrush property
// 	public static readonly StyledProperty<IBrush?> SelectionBrushProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.SelectionForegroundBrush property
// 	public static readonly StyledProperty<IBrush?> SelectionForegroundBrushProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.CaretBrush property
// 	public static readonly StyledProperty<IBrush?> CaretBrushProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.CaretBlinkInterval property
// 	public static readonly StyledProperty<TimeSpan> CaretBlinkIntervalProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.SelectionStart property
// 	public static readonly StyledProperty<int> SelectionStartProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.SelectionEnd property
// 	public static readonly StyledProperty<int> SelectionEndProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.MaxLength property
// 	public static readonly StyledProperty<int> MaxLengthProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.MaxLines property
// 	public static readonly StyledProperty<int> MaxLinesProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.MinLines property
// 	public static readonly StyledProperty<int> MinLinesProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.Text property
// 	public static readonly StyledProperty<string?> TextProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.TextAlignment property
// 	public static readonly StyledProperty<TextAlignment> TextAlignmentProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Layout.HorizontalAlignment property.
// 	public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Layout.VerticalAlignment property.
// 	public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty;

// 	public static readonly StyledProperty<TextWrapping> TextWrappingProperty;

// 	//
// 	// 摘要:
// 	//     Defines see Avalonia.Controls.Presenters.TextPresenter.LineHeight property.
// 	public static readonly StyledProperty<double> LineHeightProperty;

// 	//
// 	// 摘要:
// 	//     Defines see Avalonia.Controls.TextBlock.LetterSpacing property.
// 	public static readonly StyledProperty<double> LetterSpacingProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.Watermark property
// 	public static readonly StyledProperty<string?> WatermarkProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.UseFloatingWatermark property
// 	public static readonly StyledProperty<bool> UseFloatingWatermarkProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.NewLine property
// 	public static readonly StyledProperty<string> NewLineProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.InnerLeftContent property
// 	public static readonly StyledProperty<object?> InnerLeftContentProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.InnerRightContent property
// 	public static readonly StyledProperty<object?> InnerRightContentProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.RevealPassword property
// 	public static readonly StyledProperty<bool> RevealPasswordProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.CanCut property
// 	public static readonly DirectProperty<TextBox, bool> CanCutProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.CanCopy property
// 	public static readonly DirectProperty<TextBox, bool> CanCopyProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.CanPaste property
// 	public static readonly DirectProperty<TextBox, bool> CanPasteProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.IsUndoEnabled property
// 	public static readonly StyledProperty<bool> IsUndoEnabledProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.UndoLimit property
// 	public static readonly StyledProperty<int> UndoLimitProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.CanUndo property
// 	public static readonly DirectProperty<TextBox, bool> CanUndoProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.CanRedo property
// 	public static readonly DirectProperty<TextBox, bool> CanRedoProperty;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.CopyingToClipboard event.
// 	public static readonly RoutedEvent<RoutedEventArgs> CopyingToClipboardEvent;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.CuttingToClipboard event.
// 	public static readonly RoutedEvent<RoutedEventArgs> CuttingToClipboardEvent;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.PastingFromClipboard event.
// 	public static readonly RoutedEvent<RoutedEventArgs> PastingFromClipboardEvent;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.TextChanged event.
// 	public static readonly RoutedEvent<TextChangedEventArgs> TextChangedEvent;

// 	//
// 	// 摘要:
// 	//     Defines the Avalonia.Controls.TextBox.TextChanging event.
// 	public static readonly RoutedEvent<TextChangingEventArgs> TextChangingEvent;

// 	private TextPresenter? _presenter;

// 	private ScrollViewer? _scrollViewer;

// 	private readonly TextBoxTextInputMethodClient _imClient = new TextBoxTextInputMethodClient();

// 	private readonly UndoRedoHelper<UndoRedoState> _undoRedoHelper;

// 	private bool _isUndoingRedoing;

// 	private bool _canCut;

// 	private bool _canCopy;

// 	private bool _canPaste;

// 	private static readonly string[] invalidCharacters;

// 	private bool _canUndo;

// 	private bool _canRedo;

// 	private int _wordSelectionStart = -1;

// 	private int _selectedTextChangesMadeSinceLastUndoSnapshot;

// 	private bool _hasDoneSnapshotOnce;

// 	private static bool _isHolding;

// 	private int _currentClickCount;

// 	private bool _isDoubleTapped;

// 	private const int _maxCharsBeforeUndoSnapshot = 7;

// 	//
// 	// 摘要:
// 	//     Gets a platform-specific Avalonia.Input.KeyGesture for the Cut action
// 	public static KeyGesture? CutGesture => Application.Current?.PlatformSettings?.HotkeyConfiguration.Cut.FirstOrDefault();

// 	//
// 	// 摘要:
// 	//     Gets a platform-specific Avalonia.Input.KeyGesture for the Copy action
// 	public static KeyGesture? CopyGesture => Application.Current?.PlatformSettings?.HotkeyConfiguration.Copy.FirstOrDefault();

// 	//
// 	// 摘要:
// 	//     Gets a platform-specific Avalonia.Input.KeyGesture for the Paste action
// 	public static KeyGesture? PasteGesture => Application.Current?.PlatformSettings?.HotkeyConfiguration.Paste.FirstOrDefault();

// 	//
// 	// 摘要:
// 	//     Gets or sets a value that determines whether the TextBox shows a selection highlight
// 	//     when it is not focused.
// 	public bool IsInactiveSelectionHighlightEnabled {
// 		get {
// 			return GetValue(IsInactiveSelectionHighlightEnabledProperty);
// 		}
// 		set {
// 			SetValue(IsInactiveSelectionHighlightEnabledProperty, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets or sets a value that determines whether the TextBox clears its selection
// 	//     after it loses focus.
// 	public bool ClearSelectionOnLostFocus {
// 		get {
// 			return GetValue(ClearSelectionOnLostFocusProperty);
// 		}
// 		set {
// 			SetValue(ClearSelectionOnLostFocusProperty, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets or sets a value that determines whether the TextBox allows and displays
// 	//     newline or return characters
// 	public bool AcceptsReturn {
// 		get {
// 			return GetValue(AcceptsReturnProperty);
// 		}
// 		set {
// 			SetValue(AcceptsReturnProperty, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets or sets a value that determins whether the TextBox allows and displays tabs
// 	public bool AcceptsTab {
// 		get {
// 			return GetValue(AcceptsTabProperty);
// 		}
// 		set {
// 			SetValue(AcceptsTabProperty, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets or sets the index of the text caret
// 	public int CaretIndex {
// 		get {
// 			return GetValue(CaretIndexProperty);
// 		}
// 		set {
// 			SetValue(CaretIndexProperty, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets or sets a value whether this TextBox is read-only
// 	public bool IsReadOnly {
// 		get {
// 			return GetValue(IsReadOnlyProperty);
// 		}
// 		set {
// 			SetValue(IsReadOnlyProperty, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets or sets the System.Char that should be used for password masking
// 	public char PasswordChar {
// 		get {
// 			return GetValue(PasswordCharProperty);
// 		}
// 		set {
// 			SetValue(PasswordCharProperty, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets or sets a brush that is used to highlight selected text
// 	public IBrush? SelectionBrush {
// 		get {
// 			return GetValue(SelectionBrushProperty);
// 		}
// 		set {
// 			SetValue(SelectionBrushProperty, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets or sets a brush that is used for the foreground of selected text
// 	public IBrush? SelectionForegroundBrush {
// 		get {
// 			return GetValue(SelectionForegroundBrushProperty);
// 		}
// 		set {
// 			SetValue(SelectionForegroundBrushProperty, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets or sets a brush that is used for the text caret
// 	public IBrush? CaretBrush {
// 		get {
// 			return GetValue(CaretBrushProperty);
// 		}
// 		set {
// 			SetValue(CaretBrushProperty, value);
// 		}
// 	}

// 	public TimeSpan CaretBlinkInterval {
// 		get {
// 			return GetValue(CaretBlinkIntervalProperty);
// 		}
// 		set {
// 			SetValue(CaretBlinkIntervalProperty, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets or sets the starting position of the text selected in the TextBox
// 	public int SelectionStart {
// 		get {
// 			return GetValue(SelectionStartProperty);
// 		}
// 		set {
// 			SetValue(SelectionStartProperty, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets or sets the end position of the text selected in the TextBox
// 	//
// 	// 言论：
// 	//     When the SelectionEnd is equal to Avalonia.Controls.TextBox.SelectionStart, there
// 	//     is no selected text and it marks the caret position
// 	public int SelectionEnd {
// 		get {
// 			return GetValue(SelectionEndProperty);
// 		}
// 		set {
// 			SetValue(SelectionEndProperty, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets or sets the maximum number of characters that the Avalonia.Controls.TextBox
// 	//     can accept. This constraint only applies for manually entered (user-inputted)
// 	//     text.
// 	public int MaxLength {
// 		get {
// 			return GetValue(MaxLengthProperty);
// 		}
// 		set {
// 			SetValue(MaxLengthProperty, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets or sets the maximum number of visible lines to size to.
// 	public int MaxLines {
// 		get {
// 			return GetValue(MaxLinesProperty);
// 		}
// 		set {
// 			SetValue(MaxLinesProperty, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets or sets the minimum number of visible lines to size to.
// 	public int MinLines {
// 		get {
// 			return GetValue(MinLinesProperty);
// 		}
// 		set {
// 			SetValue(MinLinesProperty, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets or sets the spacing between characters
// 	public double LetterSpacing {
// 		get {
// 			return GetValue(LetterSpacingProperty);
// 		}
// 		set {
// 			SetValue(LetterSpacingProperty, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets or sets the line height.
// 	public double LineHeight {
// 		get {
// 			return GetValue(LineHeightProperty);
// 		}
// 		set {
// 			SetValue(LineHeightProperty, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets or sets the Text content of the TextBox
// 	[Content]
// 	public string? Text {
// 		get {
// 			return GetValue(TextProperty);
// 		}
// 		set {
// 			SetValue(TextProperty, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets or sets the text selected in the TextBox
// 	public string SelectedText {
// 		get {
// 			return GetSelection();
// 		}
// 		[param: AllowNull]
// 		set {
// 			if (string.IsNullOrEmpty(value)) {
// 				_selectedTextChangesMadeSinceLastUndoSnapshot++;
// 				SnapshotUndoRedo(ignoreChangeCount: false);
// 				DeleteSelection();
// 			} else {
// 				HandleTextInput(value);
// 			}
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets or sets the horizontal alignment of the content within the control.
// 	public HorizontalAlignment HorizontalContentAlignment {
// 		get {
// 			return GetValue(HorizontalContentAlignmentProperty);
// 		}
// 		set {
// 			SetValue(HorizontalContentAlignmentProperty, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets or sets the vertical alignment of the content within the control.
// 	public VerticalAlignment VerticalContentAlignment {
// 		get {
// 			return GetValue(VerticalContentAlignmentProperty);
// 		}
// 		set {
// 			SetValue(VerticalContentAlignmentProperty, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets or sets the Avalonia.Media.TextAlignment of the TextBox
// 	public TextAlignment TextAlignment {
// 		get {
// 			return GetValue(TextAlignmentProperty);
// 		}
// 		set {
// 			SetValue(TextAlignmentProperty, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets or sets the placeholder or descriptive text that is displayed even if the
// 	//     Avalonia.Controls.TextBox.Text property is not yet set.
// 	public string? Watermark {
// 		get {
// 			return GetValue(WatermarkProperty);
// 		}
// 		set {
// 			SetValue(WatermarkProperty, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets or sets a value indicating whether the Avalonia.Controls.TextBox.Watermark
// 	//     will still be shown above the Avalonia.Controls.TextBox.Text even after a text
// 	//     value is set.
// 	public bool UseFloatingWatermark {
// 		get {
// 			return GetValue(UseFloatingWatermarkProperty);
// 		}
// 		set {
// 			SetValue(UseFloatingWatermarkProperty, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets or sets custom content that is positioned on the left side of the text layout
// 	//     box
// 	public object? InnerLeftContent {
// 		get {
// 			return GetValue(InnerLeftContentProperty);
// 		}
// 		set {
// 			SetValue(InnerLeftContentProperty, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets or sets custom content that is positioned on the right side of the text
// 	//     layout box
// 	public object? InnerRightContent {
// 		get {
// 			return GetValue(InnerRightContentProperty);
// 		}
// 		set {
// 			SetValue(InnerRightContentProperty, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets or sets whether text masked by Avalonia.Controls.TextBox.PasswordChar should
// 	//     be revealed
// 	public bool RevealPassword {
// 		get {
// 			return GetValue(RevealPasswordProperty);
// 		}
// 		set {
// 			SetValue(RevealPasswordProperty, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets or sets the Avalonia.Media.TextWrapping of the TextBox
// 	public TextWrapping TextWrapping {
// 		get {
// 			return GetValue(TextWrappingProperty);
// 		}
// 		set {
// 			SetValue(TextWrappingProperty, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets or sets which characters are inserted when Enter is pressed. Default: System.Environment.NewLine
// 	public string NewLine {
// 		get {
// 			return GetValue(NewLineProperty);
// 		}
// 		set {
// 			SetValue(NewLineProperty, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Property for determining if the Cut command can be executed.
// 	public bool CanCut {
// 		get {
// 			return _canCut;
// 		}
// 		private set {
// 			SetAndRaise(CanCutProperty, ref _canCut, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Property for determining if the Copy command can be executed.
// 	public bool CanCopy {
// 		get {
// 			return _canCopy;
// 		}
// 		private set {
// 			SetAndRaise(CanCopyProperty, ref _canCopy, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Property for determining if the Paste command can be executed.
// 	public bool CanPaste {
// 		get {
// 			return _canPaste;
// 		}
// 		private set {
// 			SetAndRaise(CanPasteProperty, ref _canPaste, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Property for determining whether undo/redo is enabled
// 	public bool IsUndoEnabled {
// 		get {
// 			return GetValue(IsUndoEnabledProperty);
// 		}
// 		set {
// 			SetValue(IsUndoEnabledProperty, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets or sets the maximum number of items that can reside in the Undo stack
// 	public int UndoLimit {
// 		get {
// 			return GetValue(UndoLimitProperty);
// 		}
// 		set {
// 			SetValue(UndoLimitProperty, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets a value that indicates whether the undo stack has an action that can be
// 	//     undone
// 	public bool CanUndo {
// 		get {
// 			return _canUndo;
// 		}
// 		private set {
// 			SetAndRaise(CanUndoProperty, ref _canUndo, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Gets a value that indicates whether the redo stack has an action that can be
// 	//     redone
// 	public bool CanRedo {
// 		get {
// 			return _canRedo;
// 		}
// 		private set {
// 			SetAndRaise(CanRedoProperty, ref _canRedo, value);
// 		}
// 	}

// 	private bool IsPasswordBox => PasswordChar != '\0';

// 	UndoRedoState UndoRedoHelper<UndoRedoState>.IUndoRedoHost.UndoRedoState {
// 		get {
// 			return new UndoRedoState(Text, CaretIndex);
// 		}
// 		set {
// 			SetCurrentValue(TextProperty, value.Text);
// 			SetCurrentValue(CaretIndexProperty, value.CaretPosition);
// 			ClearSelection();
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Raised when content is being copied to the clipboard
// 	public event EventHandler<RoutedEventArgs>? CopyingToClipboard {
// 		add {
// 			AddHandler(CopyingToClipboardEvent, value);
// 		}
// 		remove {
// 			RemoveHandler(CopyingToClipboardEvent, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Raised when content is being cut to the clipboard
// 	public event EventHandler<RoutedEventArgs>? CuttingToClipboard {
// 		add {
// 			AddHandler(CuttingToClipboardEvent, value);
// 		}
// 		remove {
// 			RemoveHandler(CuttingToClipboardEvent, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Raised when content is being pasted from the clipboard
// 	public event EventHandler<RoutedEventArgs>? PastingFromClipboard {
// 		add {
// 			AddHandler(PastingFromClipboardEvent, value);
// 		}
// 		remove {
// 			RemoveHandler(PastingFromClipboardEvent, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Occurs asynchronously after text changes and the new text is rendered.
// 	public event EventHandler<TextChangedEventArgs>? TextChanged {
// 		add {
// 			AddHandler(TextChangedEvent, value);
// 		}
// 		remove {
// 			RemoveHandler(TextChangedEvent, value);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Occurs synchronously when text starts to change but before it is rendered.
// 	//
// 	// 言论：
// 	//     This event occurs just after the Avalonia.Controls.TextBox.Text property value
// 	//     has been updated.
// 	public event EventHandler<TextChangingEventArgs>? TextChanging {
// 		add {
// 			AddHandler(TextChangingEvent, value);
// 		}
// 		remove {
// 			RemoveHandler(TextChangingEvent, value);
// 		}
// 	}

// 	static TextBox() {
// 		IsInactiveSelectionHighlightEnabledProperty = AvaloniaProperty.Register<TextBox, bool>("IsInactiveSelectionHighlightEnabled", defaultValue: true);
// 		ClearSelectionOnLostFocusProperty = AvaloniaProperty.Register<TextBox, bool>("ClearSelectionOnLostFocus", defaultValue: true);
// 		AcceptsReturnProperty = AvaloniaProperty.Register<TextBox, bool>("AcceptsReturn", defaultValue: false);
// 		AcceptsTabProperty = AvaloniaProperty.Register<TextBox, bool>("AcceptsTab", defaultValue: false);
// 		CaretIndexProperty = AvaloniaProperty.Register<TextBox, int>("CaretIndex", 0, inherits: false, BindingMode.OneWay, null, CoerceCaretIndex);
// 		IsReadOnlyProperty = AvaloniaProperty.Register<TextBox, bool>("IsReadOnly", defaultValue: false);
// 		PasswordCharProperty = AvaloniaProperty.Register<TextBox, char>("PasswordChar", '\0');
// 		SelectionBrushProperty = AvaloniaProperty.Register<TextBox, IBrush>("SelectionBrush");
// 		SelectionForegroundBrushProperty = AvaloniaProperty.Register<TextBox, IBrush>("SelectionForegroundBrush");
// 		CaretBrushProperty = AvaloniaProperty.Register<TextBox, IBrush>("CaretBrush");
// 		CaretBlinkIntervalProperty = AvaloniaProperty.Register<TextBox, TimeSpan>("CaretBlinkInterval", TimeSpan.FromMilliseconds(500.0));
// 		SelectionStartProperty = AvaloniaProperty.Register<TextBox, int>("SelectionStart", 0, inherits: false, BindingMode.OneWay, null, CoerceCaretIndex);
// 		SelectionEndProperty = AvaloniaProperty.Register<TextBox, int>("SelectionEnd", 0, inherits: false, BindingMode.OneWay, null, CoerceCaretIndex);
// 		MaxLengthProperty = AvaloniaProperty.Register<TextBox, int>("MaxLength", 0);
// 		MaxLinesProperty = AvaloniaProperty.Register<TextBox, int>("MaxLines", 0);
// 		MinLinesProperty = AvaloniaProperty.Register<TextBox, int>("MinLines", 0);
// 		StyledProperty<string?> textProperty = TextBlock.TextProperty;
// 		Func<AvaloniaObject, string, string> coerce = CoerceText;
// 		TextProperty = textProperty.AddOwner<TextBox>(new StyledPropertyMetadata<string>(default(Optional<string>), BindingMode.TwoWay, coerce, enableDataValidation: true));
// 		TextAlignmentProperty = TextBlock.TextAlignmentProperty.AddOwner<TextBox>();
// 		HorizontalContentAlignmentProperty = ContentControl.HorizontalContentAlignmentProperty.AddOwner<TextBox>();
// 		VerticalContentAlignmentProperty = ContentControl.VerticalContentAlignmentProperty.AddOwner<TextBox>();
// 		TextWrappingProperty = TextBlock.TextWrappingProperty.AddOwner<TextBox>();
// 		LineHeightProperty = TextBlock.LineHeightProperty.AddOwner<TextBox>(new StyledPropertyMetadata<double>(double.NaN));
// 		LetterSpacingProperty = TextBlock.LetterSpacingProperty.AddOwner<TextBox>();
// 		WatermarkProperty = AvaloniaProperty.Register<TextBox, string>("Watermark");
// 		UseFloatingWatermarkProperty = AvaloniaProperty.Register<TextBox, bool>("UseFloatingWatermark", defaultValue: false);
// 		NewLineProperty = AvaloniaProperty.Register<TextBox, string>("NewLine", Environment.NewLine);
// 		InnerLeftContentProperty = AvaloniaProperty.Register<TextBox, object>("InnerLeftContent");
// 		InnerRightContentProperty = AvaloniaProperty.Register<TextBox, object>("InnerRightContent");
// 		RevealPasswordProperty = AvaloniaProperty.Register<TextBox, bool>("RevealPassword", defaultValue: false);
// 		CanCutProperty = AvaloniaProperty.RegisterDirect("CanCut", (TextBox o) => o.CanCut, null, unsetValue: false);
// 		CanCopyProperty = AvaloniaProperty.RegisterDirect("CanCopy", (TextBox o) => o.CanCopy, null, unsetValue: false);
// 		CanPasteProperty = AvaloniaProperty.RegisterDirect("CanPaste", (TextBox o) => o.CanPaste, null, unsetValue: false);
// 		IsUndoEnabledProperty = AvaloniaProperty.Register<TextBox, bool>("IsUndoEnabled", defaultValue: true);
// 		UndoLimitProperty = AvaloniaProperty.Register<TextBox, int>("UndoLimit", 10);
// 		CanUndoProperty = AvaloniaProperty.RegisterDirect("CanUndo", (TextBox x) => x.CanUndo, null, unsetValue: false);
// 		CanRedoProperty = AvaloniaProperty.RegisterDirect("CanRedo", (TextBox x) => x.CanRedo, null, unsetValue: false);
// 		CopyingToClipboard = RoutedEvent.Register<TextBox, RoutedEventArgs>("CopyingToClipboard", RoutingStrategies.Bubble);
// 		CuttingToClipboard = RoutedEvent.Register<TextBox, RoutedEventArgs>("CuttingToClipboard", RoutingStrategies.Bubble);
// 		PastingFromClipboard = RoutedEvent.Register<TextBox, RoutedEventArgs>("PastingFromClipboard", RoutingStrategies.Bubble);
// 		TextChanged = RoutedEvent.Register<TextBox, TextChangedEventArgs>("TextChanged", RoutingStrategies.Bubble);
// 		TextChanging = RoutedEvent.Register<TextBox, TextChangingEventArgs>("TextChanging", RoutingStrategies.Bubble);
// 		invalidCharacters = new string[1] { "\u007f" };
// 		InputElement.FocusableProperty.OverrideDefaultValue(typeof(TextBox), defaultValue: true);
// 		InputElement.TextInputMethodClientRequested.AddClassHandler(delegate (TextBox tb, TextInputMethodClientRequestedEventArgs e) {
// 			if (!tb.IsReadOnly) {
// 				e.Client = tb._imClient;
// 			}
// 		});
// 	}

// 	public TextBox() {
// 		IObservable<ScrollBarVisibility> source = this.GetObservable(AcceptsReturnProperty).CombineLatest(this.GetObservable(TextWrappingProperty), delegate (bool acceptsReturn, TextWrapping wrapping) {
// 			if (wrapping != 0) {
// 				return ScrollBarVisibility.Disabled;
// 			}

// 			return acceptsReturn ? ScrollBarVisibility.Auto : ScrollBarVisibility.Hidden;
// 		});
// 		Bind(ScrollViewer.HorizontalScrollBarVisibilityProperty, source, BindingPriority.Style);
// 		_undoRedoHelper = new UndoRedoHelper<UndoRedoState>(this);
// 		_selectedTextChangesMadeSinceLastUndoSnapshot = 0;
// 		_hasDoneSnapshotOnce = false;
// 		UpdatePseudoclasses();
// 	}

// 	private void OnCaretIndexChanged(AvaloniaPropertyChangedEventArgs e) {
// 		if (IsUndoEnabled && _undoRedoHelper.TryGetLastState(out var _state) && _state.Text == Text) {
// 			_undoRedoHelper.UpdateLastState();
// 		}

// 		using (_imClient.BeginChange()) {
// 			int newValue = e.GetNewValue<int>();
// 			SetCurrentValue(SelectionStartProperty, newValue);
// 			SetCurrentValue(SelectionEndProperty, newValue);
// 			_presenter?.SetCurrentValue(TextPresenter.CaretIndexProperty, newValue);
// 		}
// 	}

// 	private void OnSelectionStartChanged(AvaloniaPropertyChangedEventArgs e) {
// 		UpdateCommandStates();
// 		int newValue = e.GetNewValue<int>();
// 		if (SelectionEnd == newValue && CaretIndex != newValue) {
// 			SetCurrentValue(CaretIndexProperty, newValue);
// 		}
// 	}

// 	private void OnSelectionEndChanged(AvaloniaPropertyChangedEventArgs e) {
// 		UpdateCommandStates();
// 		int newValue = e.GetNewValue<int>();
// 		if (SelectionStart == newValue && CaretIndex != newValue) {
// 			SetCurrentValue(CaretIndexProperty, newValue);
// 		}
// 	}

// 	private static string? CoerceText(AvaloniaObject sender, string? value) {
// 		return ((TextBox)sender).CoerceText(value);
// 	}

// 	//
// 	// 摘要:
// 	//     Coerces the current text.
// 	//
// 	// 参数:
// 	//   value:
// 	//     The initial text.
// 	//
// 	// 返回结果:
// 	//     A coerced text.
// 	//
// 	// 言论：
// 	//     This method also manages the internal undo/redo state whenever the text changes:
// 	//     if overridden, ensure that the base is called or undo/redo won't work correctly.
// 	protected virtual string? CoerceText(string? value) {
// 		if (!_isUndoingRedoing) {
// 			SnapshotUndoRedo();
// 		}

// 		return value;
// 	}

// 	//
// 	// 摘要:
// 	//     Clears the current selection, maintaining the Avalonia.Controls.TextBox.CaretIndex
// 	public void ClearSelection() {
// 		SetCurrentValue(CaretIndexProperty, SelectionStart);
// 		SetCurrentValue(SelectionEndProperty, SelectionStart);
// 	}

// 	private void OnUndoLimitChanged(int newValue) {
// 		_undoRedoHelper.Limit = newValue;
// 		_undoRedoHelper.Clear();
// 		_selectedTextChangesMadeSinceLastUndoSnapshot = 0;
// 		_hasDoneSnapshotOnce = false;
// 	}

// 	//
// 	// 摘要:
// 	//     Get the number of lines in the TextBox.
// 	//
// 	// 值:
// 	//     number of lines in the TextBox, or -1 if no layout information is available
// 	//
// 	// 言论：
// 	//     If Wrap == true, changing the width of the TextBox may change this value. The
// 	//     value returned is the number of lines in the entire TextBox, regardless of how
// 	//     many are currently in view.
// 	public int GetLineCount() {
// 		return _presenter?.TextLayout.TextLines.Count ?? (-1);
// 	}

// 	protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
// 		_presenter = e.NameScope.Get<TextPresenter>("PART_TextPresenter");
// 		if (_scrollViewer != null) {
// 			_scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
// 		}

// 		_scrollViewer = e.NameScope.Find<ScrollViewer>("PART_ScrollViewer");
// 		if (_scrollViewer != null) {
// 			_scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
// 		}

// 		_imClient.SetPresenter(_presenter, this);
// 		if (base.IsFocused) {
// 			_presenter?.ShowCaret();
// 		}
// 	}

// 	private void ScrollViewer_ScrollChanged(object? sender, ScrollChangedEventArgs e) {
// 		_presenter?.TextSelectionHandleCanvas?.MoveHandlesToSelection();
// 	}

// 	protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
// 		base.OnAttachedToVisualTree(e);
// 		if (_presenter != null) {
// 			if (base.IsFocused) {
// 				_presenter.ShowCaret();
// 			} else if (IsInactiveSelectionHighlightEnabled) {
// 				_presenter.ShowSelectionHighlight = true;
// 			}

// 			_presenter.PropertyChanged += PresenterPropertyChanged;
// 		}
// 	}

// 	protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
// 		base.OnDetachedFromVisualTree(e);
// 		if (_presenter != null) {
// 			_presenter.HideCaret();
// 			_presenter.PropertyChanged -= PresenterPropertyChanged;
// 		}

// 		_imClient.SetPresenter(null, null);
// 	}

// 	private void PresenterPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e) {
// 		if (e.Property == TextPresenter.PreeditTextProperty && string.IsNullOrEmpty(e.OldValue as string) && !string.IsNullOrEmpty(e.NewValue as string)) {
// 			base.PseudoClasses.Set(":empty", value: false);
// 			DeleteSelection();
// 		}
// 	}

// 	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
// 		base.OnPropertyChanged(change);
// 		if (change.Property == TextProperty) {
// 			CoerceValue(CaretIndexProperty);
// 			CoerceValue(SelectionStartProperty);
// 			CoerceValue(SelectionEndProperty);
// 			RaiseTextChangeEvents();
// 			UpdatePseudoclasses();
// 			UpdateCommandStates();
// 		} else if (change.Property == CaretIndexProperty) {
// 			OnCaretIndexChanged(change);
// 		} else if (change.Property == SelectionStartProperty) {
// 			OnSelectionStartChanged(change);
// 		} else if (change.Property == SelectionEndProperty) {
// 			OnSelectionEndChanged(change);
// 		} else if (change.Property == MaxLinesProperty) {
// 			InvalidateMeasure();
// 		} else if (change.Property == MinLinesProperty) {
// 			InvalidateMeasure();
// 		} else if (change.Property == UndoLimitProperty) {
// 			OnUndoLimitChanged(change.GetNewValue<int>());
// 		} else if (change.Property == IsUndoEnabledProperty && !change.GetNewValue<bool>()) {
// 			_undoRedoHelper.Clear();
// 			_selectedTextChangesMadeSinceLastUndoSnapshot = 0;
// 			_hasDoneSnapshotOnce = false;
// 		}
// 	}

// 	private void UpdateCommandStates() {
// 		bool flag = string.IsNullOrEmpty(GetSelection());
// 		CanCopy = !IsPasswordBox && !flag;
// 		CanCut = !IsPasswordBox && !flag && !IsReadOnly;
// 		CanPaste = !IsReadOnly;
// 	}

// 	protected override void OnGotFocus(GotFocusEventArgs e) {
// 		base.OnGotFocus(e);
// 		if (_presenter != null) {
// 			_presenter.ShowSelectionHighlight = true;
// 		}

// 		if (e.NavigationMethod == NavigationMethod.Tab && !AcceptsReturn) {
// 			string? text = Text;
// 			if (text != null && text.Length > 0) {
// 				SelectAll();
// 			}
// 		}

// 		UpdateCommandStates();
// 		_imClient.SetPresenter(_presenter, this);
// 		_presenter?.ShowCaret();
// 	}

// 	protected override void OnLostFocus(RoutedEventArgs e) {
// 		base.OnLostFocus(e);
// 		if ((base.ContextFlyout == null || !base.ContextFlyout.IsOpen) && (base.ContextMenu == null || !base.ContextMenu.IsOpen)) {
// 			if (ClearSelectionOnLostFocus) {
// 				ClearSelection();
// 			}

// 			SetCurrentValue(RevealPasswordProperty, value: false);
// 		}

// 		UpdateCommandStates();
// 		_presenter?.HideCaret();
// 		_imClient.SetPresenter(null, null);
// 		if (_presenter != null && !IsInactiveSelectionHighlightEnabled) {
// 			_presenter.ShowSelectionHighlight = false;
// 		}
// 	}

// 	protected override void OnTextInput(TextInputEventArgs e) {
// 		if (!e.Handled) {
// 			HandleTextInput(e.Text);
// 			e.Handled = true;
// 		}
// 	}

// 	private void HandleTextInput(string? input) {
// 		if (IsReadOnly) {
// 			return;
// 		}

// 		input = SanitizeInputText(input);
// 		if (string.IsNullOrEmpty(input)) {
// 			return;
// 		}

// 		_selectedTextChangesMadeSinceLastUndoSnapshot++;
// 		SnapshotUndoRedo(ignoreChangeCount: false);
// 		string text = Text ?? string.Empty;
// 		int num = Math.Abs(SelectionStart - SelectionEnd);
// 		int num2 = input.Length + text.Length - num;
// 		if (MaxLength > 0 && num2 > MaxLength) {
// 			input = input.Remove(Math.Max(0, input.Length - (num2 - MaxLength)));
// 			num2 = MaxLength;
// 		}

// 		if (!string.IsNullOrEmpty(input)) {
// 			StringBuilder stringBuilder = StringBuilderCache.Acquire(Math.Max(text.Length, num2));
// 			stringBuilder.Append(text);
// 			int num3 = CaretIndex;
// 			if (num != 0) {
// 				int item = GetSelectionRange().start;
// 				stringBuilder.Remove(item, num);
// 				num3 = item;
// 			}

// 			stringBuilder.Insert(num3, input);
// 			string stringAndRelease = StringBuilderCache.GetStringAndRelease(stringBuilder);
// 			SetCurrentValue(TextProperty, stringAndRelease);
// 			ClearSelection();
// 			if (IsUndoEnabled) {
// 				_undoRedoHelper.DiscardRedo();
// 			}

// 			_presenter?.SetCurrentValue(TextPresenter.TextProperty, stringAndRelease);
// 			num3 += input.Length;
// 			_presenter?.MoveCaretToTextPosition(num3);
// 			SetCurrentValue(CaretIndexProperty, num3);
// 		}
// 	}

// 	private string? SanitizeInputText(string? text) {
// 		if (text == null) {
// 			return null;
// 		}

// 		if (!AcceptsReturn) {
// 			int num = 0;
// 			GraphemeEnumerator graphemeEnumerator = new GraphemeEnumerator(text.AsSpan());
// 			Grapheme grapheme;
// 			while (graphemeEnumerator.MoveNext(out grapheme) && !grapheme.FirstCodepoint.IsBreakChar) {
// 				num += grapheme.Length;
// 			}

// 			text = text.Substring(0, num);
// 		}

// 		for (int i = 0; i < invalidCharacters.Length; i++) {
// 			text = text.Replace(invalidCharacters[i], string.Empty);
// 		}

// 		return text;
// 	}

// 	//
// 	// 摘要:
// 	//     Cuts the current text onto the clipboard
// 	public async void Cut() {
// 		string selection = GetSelection();
// 		if (string.IsNullOrEmpty(selection)) {
// 			return;
// 		}

// 		RoutedEventArgs routedEventArgs = new RoutedEventArgs(CuttingToClipboard);
// 		RaiseEvent(routedEventArgs);
// 		if (!routedEventArgs.Handled) {
// 			SnapshotUndoRedo();
// 			IClipboard clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
// 			if (clipboard != null) {
// 				await clipboard.SetTextAsync(selection);
// 				DeleteSelection();
// 			}
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Copies the current text onto the clipboard
// 	public async void Copy() {
// 		string selection = GetSelection();
// 		if (string.IsNullOrEmpty(selection)) {
// 			return;
// 		}

// 		RoutedEventArgs routedEventArgs = new RoutedEventArgs(CopyingToClipboard);
// 		RaiseEvent(routedEventArgs);
// 		if (!routedEventArgs.Handled) {
// 			IClipboard clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
// 			if (clipboard != null) {
// 				await clipboard.SetTextAsync(selection);
// 			}
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Pastes the current clipboard text content into the TextBox
// 	public async void Paste() {
// 		RoutedEventArgs routedEventArgs = new RoutedEventArgs(PastingFromClipboard);
// 		RaiseEvent(routedEventArgs);
// 		if (routedEventArgs.Handled) {
// 			return;
// 		}

// 		string text = null;
// 		IClipboard clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
// 		if (clipboard != null) {
// 			try {
// 				text = await clipboard.TryGetTextAsync();
// 			} catch (TimeoutException) {
// 			}
// 		}

// 		if (!string.IsNullOrEmpty(text)) {
// 			SnapshotUndoRedo();
// 			HandleTextInput(text);
// 		}
// 	}

// 	protected override void OnKeyDown(KeyEventArgs e) {
// 		KeyEventArgs e2 = e;
// 		if (_presenter == null || !string.IsNullOrEmpty(_presenter.PreeditText)) {
// 			return;
// 		}

// 		string text = Text ?? string.Empty;
// 		int caretIndex = CaretIndex;
// 		bool flag = false;
// 		bool flag2 = false;
// 		bool flag3 = false;
// 		KeyModifiers keyModifiers = e2.KeyModifiers;
// 		PlatformHotkeyConfiguration keymap = Application.Current.PlatformSettings.HotkeyConfiguration;
// 		using (_imClient.BeginChange()) {
// 			if (Match(keymap.SelectAll)) {
// 				SelectAll();
// 				flag3 = true;
// 			} else if (Match(keymap.Copy)) {
// 				if (!IsPasswordBox) {
// 					Copy();
// 				}

// 				flag3 = true;
// 			} else if (Match(keymap.Cut)) {
// 				if (!IsPasswordBox) {
// 					Cut();
// 				}

// 				flag3 = true;
// 			} else if (Match(keymap.Paste)) {
// 				Paste();
// 				flag3 = true;
// 			} else if (Match(keymap.Undo) && IsUndoEnabled) {
// 				Undo();
// 				flag3 = true;
// 			} else if (Match(keymap.Redo) && IsUndoEnabled) {
// 				Redo();
// 				flag3 = true;
// 			} else if (Match(keymap.MoveCursorToTheStartOfDocument)) {
// 				MoveHome(document: true);
// 				flag = true;
// 				flag2 = false;
// 				flag3 = true;
// 				SetCurrentValue(CaretIndexProperty, _presenter.CaretIndex);
// 			} else if (Match(keymap.MoveCursorToTheEndOfDocument)) {
// 				MoveEnd(document: true);
// 				flag = true;
// 				flag2 = false;
// 				flag3 = true;
// 				SetCurrentValue(CaretIndexProperty, _presenter.CaretIndex);
// 			} else if (Match(keymap.MoveCursorToTheStartOfLine)) {
// 				MoveHome(document: false);
// 				flag = true;
// 				flag2 = false;
// 				flag3 = true;
// 				SetCurrentValue(CaretIndexProperty, _presenter.CaretIndex);
// 			} else if (Match(keymap.MoveCursorToTheEndOfLine)) {
// 				MoveEnd(document: false);
// 				flag = true;
// 				flag2 = false;
// 				flag3 = true;
// 				SetCurrentValue(CaretIndexProperty, _presenter.CaretIndex);
// 			} else if (Match(keymap.MoveCursorToTheStartOfDocumentWithSelection)) {
// 				SetCurrentValue(SelectionStartProperty, caretIndex);
// 				MoveHome(document: true);
// 				SetCurrentValue(SelectionEndProperty, _presenter.CaretIndex);
// 				flag = true;
// 				flag2 = true;
// 				flag3 = true;
// 			} else if (Match(keymap.MoveCursorToTheEndOfDocumentWithSelection)) {
// 				SetCurrentValue(SelectionStartProperty, caretIndex);
// 				MoveEnd(document: true);
// 				SetCurrentValue(SelectionEndProperty, _presenter.CaretIndex);
// 				flag = true;
// 				flag2 = true;
// 				flag3 = true;
// 			} else if (Match(keymap.MoveCursorToTheStartOfLineWithSelection)) {
// 				SetCurrentValue(SelectionStartProperty, caretIndex);
// 				MoveHome(document: false);
// 				SetCurrentValue(SelectionEndProperty, _presenter.CaretIndex);
// 				flag = true;
// 				flag2 = true;
// 				flag3 = true;
// 			} else if (Match(keymap.MoveCursorToTheEndOfLineWithSelection)) {
// 				SetCurrentValue(SelectionStartProperty, caretIndex);
// 				MoveEnd(document: false);
// 				SetCurrentValue(SelectionEndProperty, _presenter.CaretIndex);
// 				flag = true;
// 				flag2 = true;
// 				flag3 = true;
// 			} else if (Match(keymap.PageLeft)) {
// 				MovePageLeft();
// 				flag = true;
// 				flag2 = false;
// 				flag3 = true;
// 			} else if (Match(keymap.PageRight)) {
// 				MovePageRight();
// 				flag = true;
// 				flag2 = false;
// 				flag3 = true;
// 			} else if (Match(keymap.PageUp)) {
// 				MovePageUp();
// 				flag = true;
// 				flag2 = false;
// 				flag3 = true;
// 			} else if (Match(keymap.PageDown)) {
// 				MovePageDown();
// 				flag = true;
// 				flag2 = false;
// 				flag3 = true;
// 			} else {
// 				bool flag4 = keyModifiers.HasAllFlags(keymap.WholeWordTextActionModifiers) && !IsPasswordBox;
// 				switch (e2.Key) {
// 					case Key.Left:
// 						flag2 = DetectSelection();
// 						MoveHorizontal(-1, flag4, flag2, moveCaretPosition: true);
// 						if (caretIndex != _presenter.CaretIndex) {
// 							flag = true;
// 						}

// 						break;
// 					case Key.Right:
// 						flag2 = DetectSelection();
// 						MoveHorizontal(1, flag4, flag2, moveCaretPosition: true);
// 						if (caretIndex != _presenter.CaretIndex) {
// 							flag = true;
// 						}

// 						break;
// 					case Key.Up:
// 						flag2 = DetectSelection();
// 						MoveVertical(LogicalDirection.Backward, flag2);
// 						if (caretIndex != _presenter.CaretIndex) {
// 							flag = true;
// 						}

// 						break;
// 					case Key.Down:
// 						flag2 = DetectSelection();
// 						MoveVertical(LogicalDirection.Forward, flag2);
// 						if (caretIndex != _presenter.CaretIndex) {
// 							flag = true;
// 						}

// 						break;
// 					case Key.Back:
// 						SnapshotUndoRedo();
// 						if (flag4 && SelectionStart == SelectionEnd) {
// 							SetSelectionForControlBackspace();
// 						}

// 						if (!DeleteSelection()) {
// 							CharacterHit nextCharacterHit2 = _presenter.GetNextCharacterHit(LogicalDirection.Backward);
// 							int num4 = nextCharacterHit2.FirstCharacterIndex + nextCharacterHit2.TrailingLength;
// 							int lineIndexFromCharacterIndex = _presenter.TextLayout.GetLineIndexFromCharacterIndex(caretIndex, trailingEdge: true);
// 							CharacterHit backspaceCaretCharacterHit = _presenter.TextLayout.TextLines[lineIndexFromCharacterIndex].GetBackspaceCaretCharacterHit(new CharacterHit(caretIndex));
// 							if (backspaceCaretCharacterHit.FirstCharacterIndex > num4 && backspaceCaretCharacterHit.FirstCharacterIndex < caretIndex) {
// 								num4 = backspaceCaretCharacterHit.FirstCharacterIndex;
// 							}

// 							if (caretIndex != num4) {
// 								int num5 = Math.Min(num4, caretIndex);
// 								int num6 = Math.Max(num4, caretIndex);
// 								StringBuilder stringBuilder2 = StringBuilderCache.Acquire(text.Length);
// 								stringBuilder2.Append(text);
// 								stringBuilder2.Remove(num5, num6 - num5);
// 								SetCurrentValue(TextProperty, StringBuilderCache.GetStringAndRelease(stringBuilder2));
// 								SetCurrentValue(CaretIndexProperty, num5);
// 								_presenter.MoveCaretToTextPosition(num5);
// 							}
// 						}

// 						SnapshotUndoRedo();
// 						flag3 = true;
// 						break;
// 					case Key.Delete:
// 						SnapshotUndoRedo();
// 						if (flag4 && SelectionStart == SelectionEnd) {
// 							SetSelectionForControlDelete();
// 						}

// 						if (!DeleteSelection()) {
// 							CharacterHit nextCharacterHit = _presenter.GetNextCharacterHit();
// 							int num = nextCharacterHit.FirstCharacterIndex + nextCharacterHit.TrailingLength;
// 							if (num != caretIndex) {
// 								int num2 = Math.Min(num, caretIndex);
// 								int num3 = Math.Max(num, caretIndex);
// 								StringBuilder stringBuilder = StringBuilderCache.Acquire(text.Length);
// 								stringBuilder.Append(text);
// 								stringBuilder.Remove(num2, num3 - num2);
// 								SetCurrentValue(TextProperty, StringBuilderCache.GetStringAndRelease(stringBuilder));
// 							}
// 						}

// 						SnapshotUndoRedo();
// 						flag3 = true;
// 						break;
// 					case Key.Return:
// 						if (AcceptsReturn) {
// 							SnapshotUndoRedo();
// 							HandleTextInput(NewLine);
// 							flag3 = true;
// 						}

// 						break;
// 					case Key.Tab:
// 						if (AcceptsTab) {
// 							SnapshotUndoRedo();
// 							HandleTextInput("\t");
// 							flag3 = true;
// 						} else {
// 							base.OnKeyDown(e2);
// 						}

// 						break;
// 					case Key.Space:
// 						SnapshotUndoRedo();
// 						break;
// 					default:
// 						flag3 = false;
// 						break;
// 				}
// 			}

// 			if (flag && !flag2) {
// 				ClearSelection();
// 			}

// 			if (flag3 || flag) {
// 				e2.Handled = true;
// 			}
// 		}

// 		bool DetectSelection() {
// 			return e2.KeyModifiers.HasAllFlags(keymap.SelectionModifiers);
// 		}

// 		bool Match(List<KeyGesture> gestures) {
// 			return gestures.Any((KeyGesture g) => g.Matches(e2));
// 		}
// 	}

// 	protected override void OnPointerPressed(PointerPressedEventArgs e) {
// 		if (_presenter == null) {
// 			return;
// 		}

// 		string text = Text;
// 		PointerPoint currentPoint = e.GetCurrentPoint(this);
// 		using (_imClient.BeginChange()) {
// 			if (text != null && (e.Pointer.Type == PointerType.Mouse || e.ClickCount >= 2) && currentPoint.Properties.IsLeftButtonPressed && !(currentPoint.Pointer?.Captured is Border)) {
// 				_currentClickCount = e.ClickCount;
// 				Point position = e.GetPosition(_presenter);
// 				_presenter.MoveCaretToPoint(position);
// 				int caretIndex = _presenter.CaretIndex;
// 				bool flag = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
// 				int selectionStart = SelectionStart;
// 				int selectionEnd = SelectionEnd;
// 				switch (e.ClickCount) {
// 					case 1:
// 						if (flag) {
// 							if (_wordSelectionStart >= 0) {
// 								UpdateWordSelectionRange(caretIndex, ref selectionStart, ref selectionEnd);
// 								SetCurrentValue(SelectionStartProperty, selectionStart);
// 								SetCurrentValue(SelectionEndProperty, selectionEnd);
// 							} else {
// 								SetCurrentValue(SelectionEndProperty, caretIndex);
// 							}
// 						} else {
// 							SetCurrentValue(SelectionStartProperty, caretIndex);
// 							SetCurrentValue(SelectionEndProperty, caretIndex);
// 							_wordSelectionStart = -1;
// 						}

// 						break;
// 					case 2:
// 						if (!IsPasswordBox || RevealPassword) {
// 							if (!StringUtils.IsStartOfWord(text, caretIndex)) {
// 								selectionStart = StringUtils.PreviousWord(text, caretIndex);
// 							}

// 							if (!StringUtils.IsEndOfWord(text, caretIndex)) {
// 								selectionEnd = StringUtils.NextWord(text, caretIndex);
// 							}

// 							if (selectionStart != selectionEnd) {
// 								_wordSelectionStart = selectionStart;
// 							}

// 							SetCurrentValue(SelectionStartProperty, selectionStart);
// 							SetCurrentValue(SelectionEndProperty, selectionEnd);
// 							break;
// 						}

// 						goto case 3;
// 					case 3:
// 						_wordSelectionStart = -1;
// 						SelectAll();
// 						break;
// 				}
// 			}

// 			_isDoubleTapped = e.ClickCount == 2;
// 			e.Pointer.Capture(_presenter);
// 			e.Handled = true;
// 		}
// 	}

// 	protected override void OnPointerMoved(PointerEventArgs e) {
// 		if (_presenter == null || _isHolding) {
// 			return;
// 		}

// 		using (_imClient.BeginChange()) {
// 			if (e.Pointer.Captured != _presenter || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) {
// 				return;
// 			}

// 			Point point = e.GetPosition(_presenter);
// 			point = new Point(MathUtilities.Clamp(point.X, 0.0, Math.Max(_presenter.Bounds.Width - 1.0, 0.0)), MathUtilities.Clamp(point.Y, 0.0, Math.Max(_presenter.Bounds.Height - 1.0, 0.0)));
// 			int caretIndex = _presenter.CaretIndex;
// 			_presenter.MoveCaretToPoint(point);
// 			int caretIndex2 = _presenter.CaretIndex;
// 			if (Math.Abs(caretIndex2 - caretIndex) == 1) {
// 				e.PreventGestureRecognition();
// 			}

// 			if (e.Pointer.Type == PointerType.Mouse || _isDoubleTapped) {
// 				int selectionStart = SelectionStart;
// 				int selectionEnd = SelectionEnd;
// 				if (_wordSelectionStart >= 0) {
// 					UpdateWordSelectionRange(caretIndex2, ref selectionStart, ref selectionEnd);
// 					SetCurrentValue(SelectionStartProperty, selectionStart);
// 					SetCurrentValue(SelectionEndProperty, selectionEnd);
// 				} else {
// 					SetCurrentValue(SelectionEndProperty, caretIndex2);
// 				}
// 			} else {
// 				SetCurrentValue(SelectionStartProperty, caretIndex2);
// 				SetCurrentValue(SelectionEndProperty, caretIndex2);
// 			}
// 		}
// 	}

// 	private void UpdateWordSelectionRange(int caretIndex, ref int selectionStart, ref int selectionEnd) {
// 		string text = Text;
// 		if (!string.IsNullOrEmpty(text)) {
// 			if (caretIndex > _wordSelectionStart) {
// 				int num = StringUtils.NextWord(text, caretIndex);
// 				selectionEnd = num;
// 				selectionStart = _wordSelectionStart;
// 			} else {
// 				int num2 = StringUtils.PreviousWord(text, caretIndex);
// 				selectionStart = num2;
// 				selectionEnd = StringUtils.NextWord(text, _wordSelectionStart);
// 			}
// 		}
// 	}

// 	protected override void OnPointerReleased(PointerReleasedEventArgs e) {
// 		if (_presenter == null || e.Pointer.Captured != _presenter) {
// 			return;
// 		}

// 		using (_imClient.BeginChange()) {
// 			if (e.Pointer.Type != 0 && !_isDoubleTapped) {
// 				string? text = Text;
// 				PointerPoint currentPoint = e.GetCurrentPoint(this);
// 				if (text != null && !(currentPoint.Pointer?.Captured is Border)) {
// 					Point position = e.GetPosition(_presenter);
// 					_presenter.MoveCaretToPoint(position);
// 					int caretIndex = _presenter.CaretIndex;
// 					bool num = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
// 					int selectionStart = SelectionStart;
// 					int selectionEnd = SelectionEnd;
// 					if (num) {
// 						if (_wordSelectionStart >= 0) {
// 							UpdateWordSelectionRange(caretIndex, ref selectionStart, ref selectionEnd);
// 							SetCurrentValue(SelectionStartProperty, selectionStart);
// 							SetCurrentValue(SelectionEndProperty, selectionEnd);
// 						} else {
// 							SetCurrentValue(SelectionEndProperty, caretIndex);
// 						}
// 					} else {
// 						SetCurrentValue(SelectionStartProperty, caretIndex);
// 						SetCurrentValue(SelectionEndProperty, caretIndex);
// 						_wordSelectionStart = -1;
// 					}

// 					_presenter.TextSelectionHandleCanvas?.MoveHandlesToSelection();
// 				}
// 			}

// 			if (_isHolding) {
// 				_isHolding = false;
// 			} else if (e.InitialPressMouseButton == MouseButton.Right) {
// 				Point position2 = e.GetPosition(_presenter);
// 				_presenter.MoveCaretToPoint(position2);
// 				int caretIndex2 = _presenter.CaretIndex;
// 				int num2 = Math.Min(SelectionStart, SelectionEnd);
// 				int num3 = Math.Max(SelectionStart, SelectionEnd);
// 				if (SelectionStart == SelectionEnd || caretIndex2 < num2 || caretIndex2 > num3) {
// 					SetCurrentValue(CaretIndexProperty, caretIndex2);
// 					SetCurrentValue(SelectionEndProperty, caretIndex2);
// 					SetCurrentValue(SelectionStartProperty, caretIndex2);
// 				}
// 			} else if (e.Pointer.Type == PointerType.Touch) {
// 				if (_currentClickCount == 1) {
// 					Point position3 = e.GetPosition(_presenter);
// 					_presenter.MoveCaretToPoint(position3);
// 					int caretIndex3 = _presenter.CaretIndex;
// 					SetCurrentValue(SelectionStartProperty, caretIndex3);
// 					SetCurrentValue(SelectionEndProperty, caretIndex3);
// 				}

// 				if (SelectionStart != SelectionEnd) {
// 					_presenter.TextSelectionHandleCanvas?.ShowContextMenu();
// 				}
// 			}

// 			e.Pointer.Capture(null);
// 		}
// 	}

// 	protected override AutomationPeer OnCreateAutomationPeer() {
// 		return new TextBoxAutomationPeer(this);
// 	}

// 	protected override void UpdateDataValidation(AvaloniaProperty property, BindingValueType state, Exception? error) {
// 		if (property == TextProperty) {
// 			DataValidationErrors.SetError(this, error);
// 		}
// 	}

// 	internal static int CoerceCaretIndex(AvaloniaObject sender, int value) {
// 		string value2 = sender.GetValue(TextProperty);
// 		if (value2 == null) {
// 			return 0;
// 		}

// 		int length = value2.Length;
// 		if (value < 0) {
// 			return 0;
// 		}

// 		if (value > length) {
// 			return length;
// 		}

// 		if (value > 0 && value2[value - 1] == '\r' && value < length && value2[value] == '\n') {
// 			return value + 1;
// 		}

// 		return value;
// 	}

// 	//
// 	// 摘要:
// 	//     Clears the text in the TextBox
// 	public void Clear() {
// 		SetCurrentValue(TextProperty, string.Empty);
// 	}

// 	private void MoveHorizontal(int direction, bool wholeWord, bool isSelecting, bool moveCaretPosition) {
// 		if (_presenter == null) {
// 			return;
// 		}

// 		using (_imClient.BeginChange()) {
// 			string text = Text ?? string.Empty;
// 			int selectionStart = SelectionStart;
// 			int selectionEnd = SelectionEnd;
// 			if (!wholeWord) {
// 				if (isSelecting) {
// 					_presenter.MoveCaretToTextPosition(selectionEnd);
// 					_presenter.MoveCaretHorizontal((direction > 0) ? LogicalDirection.Forward : LogicalDirection.Backward);
// 					SetCurrentValue(SelectionEndProperty, _presenter.CaretIndex);
// 					return;
// 				}

// 				if (selectionStart != selectionEnd) {
// 					ClearSelectionAndMoveCaretToTextPosition((direction > 0) ? LogicalDirection.Forward : LogicalDirection.Backward);
// 				} else {
// 					_presenter.MoveCaretHorizontal((direction > 0) ? LogicalDirection.Forward : LogicalDirection.Backward);
// 				}

// 				SetCurrentValue(CaretIndexProperty, _presenter.CaretIndex);
// 			} else {
// 				int num = ((direction <= 0) ? (StringUtils.PreviousWord(text, selectionEnd) - selectionEnd) : (StringUtils.NextWord(text, selectionEnd) - selectionEnd));
// 				SetCurrentValue(SelectionEndProperty, SelectionEnd + num);
// 				if (moveCaretPosition) {
// 					_presenter.MoveCaretToTextPosition(SelectionEnd);
// 				}

// 				if (!isSelecting && moveCaretPosition) {
// 					SetCurrentValue(CaretIndexProperty, SelectionEnd);
// 				} else {
// 					SetCurrentValue(SelectionStartProperty, selectionStart);
// 				}
// 			}
// 		}
// 	}

// 	private void MoveVertical(LogicalDirection direction, bool isSelecting) {
// 		if (_presenter == null) {
// 			return;
// 		}

// 		if (isSelecting) {
// 			int caretIndex = _presenter.CaretIndex;
// 			_presenter.MoveCaretVertical(direction);
// 			int caretIndex2 = _presenter.CaretIndex;
// 			if (caretIndex == caretIndex2) {
// 				string text = Text ?? string.Empty;
// 				if (direction == LogicalDirection.Forward && caretIndex2 < text.Length) {
// 					_presenter.MoveCaretToTextPosition(text.Length);
// 				} else if (direction == LogicalDirection.Backward && caretIndex2 > 0) {
// 					_presenter.MoveCaretToTextPosition(0);
// 				}
// 			}

// 			SetCurrentValue(SelectionEndProperty, _presenter.CaretIndex);
// 		} else {
// 			if (SelectionStart != SelectionEnd) {
// 				ClearSelectionAndMoveCaretToTextPosition(direction);
// 			}

// 			_presenter.MoveCaretVertical(direction);
// 			SetCurrentValue(CaretIndexProperty, _presenter.CaretIndex);
// 		}
// 	}

// 	private void MoveHome(bool document) {
// 		if (_presenter != null) {
// 			int caretIndex = CaretIndex;
// 			if (document) {
// 				_presenter.MoveCaretToTextPosition(0);
// 				return;
// 			}

// 			IReadOnlyList<TextLine> textLines = _presenter.TextLayout.TextLines;
// 			int lineIndexFromCharacterIndex = _presenter.TextLayout.GetLineIndexFromCharacterIndex(caretIndex, trailingEdge: false);
// 			TextLine textLine = textLines[lineIndexFromCharacterIndex];
// 			_presenter.MoveCaretToTextPosition(textLine.FirstTextSourceIndex);
// 		}
// 	}

// 	private void MoveEnd(bool document) {
// 		if (_presenter != null) {
// 			string text = Text ?? string.Empty;
// 			int caretIndex = CaretIndex;
// 			if (document) {
// 				_presenter.MoveCaretToTextPosition(text.Length, trailingEdge: true);
// 				return;
// 			}

// 			IReadOnlyList<TextLine> textLines = _presenter.TextLayout.TextLines;
// 			int lineIndexFromCharacterIndex = _presenter.TextLayout.GetLineIndexFromCharacterIndex(caretIndex, trailingEdge: false);
// 			TextLine textLine = textLines[lineIndexFromCharacterIndex];
// 			int textPosition = textLine.FirstTextSourceIndex + textLine.Length - textLine.NewLineLength;
// 			_presenter.MoveCaretToTextPosition(textPosition, trailingEdge: true);
// 		}
// 	}

// 	private void MovePageRight() {
// 		_scrollViewer?.PageRight();
// 	}

// 	private void MovePageLeft() {
// 		_scrollViewer?.PageLeft();
// 	}

// 	private void MovePageUp() {
// 		_scrollViewer?.PageUp();
// 	}

// 	private void MovePageDown() {
// 		_scrollViewer?.PageDown();
// 	}

// 	private void ClearSelectionAndMoveCaretToTextPosition(LogicalDirection direction) {
// 		int num = ((direction == LogicalDirection.Forward) ? Math.Max(SelectionStart, SelectionEnd) : Math.Min(SelectionStart, SelectionEnd));
// 		SetCurrentValue(SelectionStartProperty, num);
// 		SetCurrentValue(SelectionEndProperty, num);
// 		_presenter?.MoveCaretToTextPosition(num);
// 	}

// 	//
// 	// 摘要:
// 	//     Scroll the Avalonia.Controls.TextBox to the specified line index.
// 	//
// 	// 参数:
// 	//   lineIndex:
// 	//     The line index to scroll to.
// 	//
// 	// 异常:
// 	//   T:System.ArgumentOutOfRangeException:
// 	//     lineIndex is less than zero. -or - lineIndex is larger than or equal to the line
// 	//     count.
// 	public void ScrollToLine(int lineIndex) {
// 		if (_presenter != null) {
// 			if (lineIndex < 0 || lineIndex >= _presenter.TextLayout.TextLines.Count) {
// 				throw new ArgumentOutOfRangeException("lineIndex");
// 			}

// 			TextLine textLine = _presenter.TextLayout.TextLines[lineIndex];
// 			_presenter.MoveCaretToTextPosition(textLine.FirstTextSourceIndex);
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Select all text in the TextBox
// 	public void SelectAll() {
// 		using (_imClient.BeginChange()) {
// 			SetCurrentValue(SelectionStartProperty, 0);
// 			SetCurrentValue(SelectionEndProperty, Text?.Length ?? 0);
// 		}
// 	}

// 	private (int start, int end) GetSelectionRange() {
// 		int selectionStart = SelectionStart;
// 		int selectionEnd = SelectionEnd;
// 		return (Math.Min(selectionStart, selectionEnd), Math.Max(selectionStart, selectionEnd));
// 	}

// 	internal bool DeleteSelection() {
// 		if (IsReadOnly) {
// 			return true;
// 		}

// 		using (_imClient.BeginChange()) {
// 			var (num, num2) = GetSelectionRange();
// 			if (num != num2) {
// 				string text = Text;
// 				StringBuilder stringBuilder = StringBuilderCache.Acquire(text.Length);
// 				stringBuilder.Append(text);
// 				stringBuilder.Remove(num, num2 - num);
// 				SetCurrentValue(TextProperty, StringBuilderCache.GetStringAndRelease(stringBuilder));
// 				_presenter?.MoveCaretToTextPosition(num);
// 				SetCurrentValue(SelectionStartProperty, num);
// 				ClearSelection();
// 				return true;
// 			}

// 			SetCurrentValue(CaretIndexProperty, SelectionStart);
// 			return false;
// 		}
// 	}

// 	private string GetSelection() {
// 		string text = Text;
// 		if (string.IsNullOrEmpty(text)) {
// 			return "";
// 		}

// 		int selectionStart = SelectionStart;
// 		int selectionEnd = SelectionEnd;
// 		int num = Math.Min(selectionStart, selectionEnd);
// 		int num2 = Math.Max(selectionStart, selectionEnd);
// 		if (num == num2 || (Text?.Length ?? 0) < num2) {
// 			return "";
// 		}

// 		return text.Substring(num, num2 - num);
// 	}

// 	//
// 	// 摘要:
// 	//     Returns the sum of any vertical whitespace added between the Avalonia.Controls.ScrollViewer
// 	//     and Avalonia.Controls.Presenters.TextPresenter in the control template.
// 	//
// 	// 返回结果:
// 	//     The total vertical whitespace.
// 	private double GetVerticalSpaceBetweenScrollViewerAndPresenter() {
// 		double num = 0.0;
// 		if (_presenter != null) {
// 			Visual visual = _presenter;
// 			while (visual != null && visual != this) {
// 				if (visual == _scrollViewer) {
// 					num += _scrollViewer.Padding.Top + _scrollViewer.Padding.Bottom;
// 					break;
// 				}

// 				Thickness value = visual.GetValue(Layoutable.MarginProperty);
// 				Thickness value2 = visual.GetValue(Decorator.PaddingProperty);
// 				num += value.Top + value2.Top + value2.Bottom + value.Bottom;
// 				visual = visual.VisualParent;
// 			}
// 		}

// 		return num;
// 	}

// 	//
// 	// 摘要:
// 	//     Raises both the Avalonia.Controls.TextBox.TextChanging and Avalonia.Controls.TextBox.TextChanged
// 	//     events.
// 	//
// 	// 言论：
// 	//     This must be called after the Avalonia.Controls.TextBox.Text property is set.
// 	private void RaiseTextChangeEvents() {
// 		TextChangingEventArgs e = new TextChangingEventArgs(TextChanging);
// 		RaiseEvent(e);
// 		Dispatcher.UIThread.Post(delegate {
// 			TextChangedEventArgs e2 = new TextChangedEventArgs(TextChanged);
// 			RaiseEvent(e2);
// 		}, DispatcherPriority.Normal);
// 	}

// 	private void SetSelectionForControlBackspace() {
// 		string text = Text ?? string.Empty;
// 		int caretIndex = CaretIndex;
// 		using (_imClient.BeginChange()) {
// 			MoveHorizontal(-1, wholeWord: true, isSelecting: false, moveCaretPosition: false);
// 			if (SelectionEnd > 0 && caretIndex < text.Length && text[caretIndex] == ' ') {
// 				SetCurrentValue(SelectionEndProperty, SelectionEnd - 1);
// 			}

// 			SetCurrentValue(SelectionStartProperty, caretIndex);
// 		}
// 	}

// 	private void SetSelectionForControlDelete() {
// 		int num = Text?.Length ?? 0;
// 		if (_presenter == null || num == 0) {
// 			return;
// 		}

// 		using (_imClient.BeginChange()) {
// 			SetCurrentValue(SelectionStartProperty, CaretIndex);
// 			MoveHorizontal(1, wholeWord: true, isSelecting: true, moveCaretPosition: false);
// 			if (SelectionEnd < num && Text[SelectionEnd] == ' ') {
// 				SetCurrentValue(SelectionEndProperty, SelectionEnd + 1);
// 			}
// 		}
// 	}

// 	private void UpdatePseudoclasses() {
// 		base.PseudoClasses.Set(":empty", string.IsNullOrEmpty(Text));
// 	}

// 	private void SnapshotUndoRedo(bool ignoreChangeCount = true) {
// 		if (IsUndoEnabled && (ignoreChangeCount || !_hasDoneSnapshotOnce || (!ignoreChangeCount && _selectedTextChangesMadeSinceLastUndoSnapshot >= 7))) {
// 			_undoRedoHelper.Snapshot();
// 			_selectedTextChangesMadeSinceLastUndoSnapshot = 0;
// 			_hasDoneSnapshotOnce = true;
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Undoes the first action in the undo stack
// 	public void Undo() {
// 		if (IsUndoEnabled && CanUndo) {
// 			try {
// 				SnapshotUndoRedo();
// 				_isUndoingRedoing = true;
// 				_undoRedoHelper.Undo();
// 			} finally {
// 				_isUndoingRedoing = false;
// 			}
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Reapplies the first item on the redo stack
// 	public void Redo() {
// 		if (IsUndoEnabled && CanRedo) {
// 			try {
// 				_isUndoingRedoing = true;
// 				_undoRedoHelper.Redo();
// 			} finally {
// 				_isUndoingRedoing = false;
// 			}
// 		}
// 	}

// 	//
// 	// 摘要:
// 	//     Called from the UndoRedoHelper when the undo stack is modified
// 	void UndoRedoHelper<UndoRedoState>.IUndoRedoHost.OnUndoStackChanged() {
// 		CanUndo = _undoRedoHelper.CanUndo;
// 	}

// 	//
// 	// 摘要:
// 	//     Called from the UndoRedoHelper when the redo stack is modified
// 	void UndoRedoHelper<UndoRedoState>.IUndoRedoHost.OnRedoStackChanged() {
// 		CanRedo = _undoRedoHelper.CanRedo;
// 	}

// 	protected override Size MeasureOverride(Size availableSize) {
// 		if (_scrollViewer != null) {
// 			double value = double.PositiveInfinity;
// 			if (MaxLines > 0 && double.IsNaN(base.Height)) {
// 				double fontSize = base.FontSize;
// 				TextParagraphProperties paragraphProperties = TextLayout.CreateTextParagraphProperties(new Typeface(base.FontFamily, base.FontStyle, base.FontWeight, base.FontStretch), fontSize, null, TextAlignment.Left, TextWrapping.NoWrap, null, FlowDirection.LeftToRight, LineHeight, 0.0, base.FontFeatures);
// 				TextLayout textLayout = new TextLayout(new LineTextSource(MaxLines), paragraphProperties);
// 				double verticalSpaceBetweenScrollViewerAndPresenter = GetVerticalSpaceBetweenScrollViewerAndPresenter();
// 				value = Math.Ceiling(textLayout.Height + verticalSpaceBetweenScrollViewerAndPresenter);
// 			}

// 			_scrollViewer.SetCurrentValue(Layoutable.MaxHeightProperty, value);
// 			double value2 = 0.0;
// 			if (MinLines > 0 && double.IsNaN(base.Height)) {
// 				double fontSize2 = base.FontSize;
// 				TextParagraphProperties paragraphProperties2 = TextLayout.CreateTextParagraphProperties(new Typeface(base.FontFamily, base.FontStyle, base.FontWeight, base.FontStretch), fontSize2, null, TextAlignment.Left, TextWrapping.NoWrap, null, FlowDirection.LeftToRight, LineHeight, 0.0, base.FontFeatures);
// 				TextLayout textLayout2 = new TextLayout(new LineTextSource(MinLines), paragraphProperties2);
// 				double verticalSpaceBetweenScrollViewerAndPresenter2 = GetVerticalSpaceBetweenScrollViewerAndPresenter();
// 				value2 = Math.Ceiling(textLayout2.Height + verticalSpaceBetweenScrollViewerAndPresenter2);
// 			}

// 			_scrollViewer.SetCurrentValue(Layoutable.MinHeightProperty, value2);
// 		}

// 		return base.MeasureOverride(availableSize);
// 	}
// }

