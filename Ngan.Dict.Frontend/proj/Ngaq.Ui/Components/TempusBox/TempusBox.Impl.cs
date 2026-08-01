namespace Ngaq.Ui.Components.TempusBox;
using Tsinswreng.CsTempus;
using Avalonia;
using Avalonia.Data;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using Ngaq.Core.Infra;
using Tsinswreng.AvlnTools.Dsl;
using Ngaq.Ui.Icons;

public partial class TempusBox: ContentControl{
	UnixMs? _Tempus = null;
	bool _IsReadOnly = false;
	i32 _SelectedFormatIndex = 0;
	f64 _ControlHeight = 34;
 	ITempusFormatItem _SelectedFormat = TempusFormatItem.Iso8601Full;
 	ICommand? _InvalidInputCommand = null;
 	ICommand? _DefaultInvalidInputCommand = null;

	readonly IList<ITempusFormatItem> _FormatItems = [
		TempusFormatItem.Iso8601Full,
		TempusFormatItem.UnixMs,
	];
	bool _LastParseOk = true;

	public partial UnixMs? Tempus{
		get{return _Tempus;}
		set{
			SetAndRaise(TempusProperty, ref _Tempus, value);
			SyncUiFromTempus();
		}
	}

	public ITempusFormatItem SelectedFormat{
		get{return _SelectedFormat;}
		set{
			var next = value ?? (FormatItems.Count > 0 ? FormatItems[0] : TempusFormatItem.Iso8601Full);
			SetAndRaise(SelectedFormatProperty, ref _SelectedFormat, next);
			var idx = FormatItems.IndexOf(next);
			if(idx >= 0){
				_SelectedFormatIndex = idx;
			}
			SyncUiFromTempus();
		}
	}

	/*
	o.InvalidInputCommand = new RelayCommand<InvalidCommandParam?>((param)=>{
			var badText = param?.Text ?? "";
			var self = param?.Self ?? o;
			ToolTip.SetTip(self, $"Time format is invalid: {badText}");
			self._Input.BorderBrush = Brushes.Goldenrod;
			self._Input.Foreground = Brushes.Goldenrod;
		});
	*/
	public ICommand? InvalidInputCommand{
		get{return _InvalidInputCommand;}
		set{
			var next = value ?? _DefaultInvalidInputCommand;
			SetAndRaise(InvalidInputCommandProperty, ref _InvalidInputCommand, next);
		}
	}

	public partial bool IsReadOnly{
		get{return _IsReadOnly;}
		set{
			_IsReadOnly = value;
			ApplyReadOnlyState();
		}
	}

	public partial IList<ITempusFormatItem> FormatItems{
		get{return _FormatItems;}
	}

	public partial i32 SelectedFormatIndex{
		get{return _SelectedFormatIndex;}
		set{
			var max = FormatItems.Count - 1;
			var next = max < 0 ? 0 : Math.Clamp(value, 0, max);
			_SelectedFormatIndex = next;
			if(FormatItems.Count > 0){
				SetAndRaise(SelectedFormatProperty, ref _SelectedFormat, FormatItems[next]);
			}
			SyncUiFromTempus();
		}
	}

	public partial f64 ControlHeight{
		get{return _ControlHeight;}
		set{
			_ControlHeight = value;
			ApplyControlSize();
		}
	}

	public partial bool LastParseOk{
		get{return _LastParseOk;}
		protected set{_LastParseOk = value;}
	}

	Flyout? _MenuFlyout;
	bool _SyncingUi = false;

	partial void Init(){
		_DefaultInvalidInputCommand = new DelegateCommand(_=>{
			ApplyInvalidInputVisual();
		});
		_InvalidInputCommand = _DefaultInvalidInputCommand;
		Render();
		ApplyControlSize();
		ApplyReadOnlyState();
		SyncUiFromTempus();
	}

	public partial void RefreshFormatOptions(){
		_SelectedFormatIndex = Math.Clamp(_SelectedFormatIndex, 0, Math.Max(0, FormatItems.Count-1));
		if(FormatItems.Count > 0){
			SetAndRaise(SelectedFormatProperty, ref _SelectedFormat, FormatItems[_SelectedFormatIndex]);
		}
		SyncUiFromTempus();
	}

	void Render(){
		this.Content = Root.Grid;
		Root.Grid.SetColDefs([
			new(1, GUT.Auto),
			new(1, GUT.Star),
		]);
		Root
		.A(MkMenuBtn())
		.A(MkInputBox());
	}

	TextBox MkInputBox(){
		_Input = new TextBox{};
		var o = _Input;
		o.MinWidth = 0;
		o.VerticalContentAlignment = VAlign.Center;
		o.Margin = new(0);
		o.BorderThickness = new(1);
		//o.Padding = new(8, 0, 8, 0);
		o.TextChanged += (s, e)=>{
			// 由用戶輸入觸發時，按當前格式解析；失敗則只更新標記，不覆蓋既有 Tempus。
			if(_SyncingUi){
				return;
			}
			if(string.IsNullOrWhiteSpace(o.Text)){
				LastParseOk = true;
				RestoreInputVisual();
				Tempus = null;
				return;
			}
			if(TryParseBySelectedFormat(o.Text ?? "", out var parsed)){
				LastParseOk = true;
				RestoreInputVisual();
				Tempus = parsed;
			}else{
				LastParseOk = false;
				TriggerInvalidInputCommand(o.Text ?? "");
			}
		};

		return o;
	}

	Button MkMenuBtn(){
		var btn = new Button{
			Content = Icons.Calendar().ToIcon()
		};
		btn.HorizontalAlignment = HAlign.Stretch;
		btn.VerticalAlignment = VAlign.Stretch;
		btn.Margin = new(0);
		btn.BorderThickness = new(1);
		btn.Padding = new(8, 0, 8, 0);
		var flyout = new Flyout{
			Placement = PlacementMode.Bottom,
		};
		var panel = new StackPanel();
		var cbFormat = new ComboBox{
			ItemsSource = FormatItems.Select(x=>x.FmtDisplayName).ToArray(),
		};
		cbFormat.SelectionChanged += (s, e)=>{
			if(_SyncingUi){
				return;
			}
			if(cbFormat.SelectedIndex is >= 0 && cbFormat.SelectedIndex < FormatItems.Count){
				SelectedFormatIndex = cbFormat.SelectedIndex;
			}
		};
		panel.A(cbFormat);
		var wrap = new Border{
			Padding = new(6),
		};
		var calendar = new Avalonia.Controls.Calendar{
			HorizontalAlignment = HAlign.Stretch,
			VerticalAlignment = VAlign.Stretch,
		};
		calendar.SelectedDatesChanged += (s, e)=>{
			// 日曆僅改日期部分，保留原時間的時分秒毫秒。
			if(_SyncingUi || IsReadOnly || calendar.SelectedDate is null){
				return;
			}
			var selectedDate = calendar.SelectedDate.Value;
			var currentLocal = Tempus is null
				? DateTime.Now
				: DateTimeOffset.FromUnixTimeMilliseconds(Tempus.Value.Value).ToLocalTime().DateTime;
			var merged = new DateTime(
				selectedDate.Year,
				selectedDate.Month,
				selectedDate.Day,
				currentLocal.Hour,
				currentLocal.Minute,
				currentLocal.Second,
				currentLocal.Millisecond,
				DateTimeKind.Local
			);
			Tempus = UnixMs.FromDateTime(merged);
		};
		wrap.Child = calendar;
		panel.A(wrap);
		flyout.Content = panel;
		FlyoutBase.SetAttachedFlyout(btn, flyout);
		btn.Click += (s, e)=>{
			if(IsReadOnly){
				return;
			}
			cbFormat.ItemsSource = FormatItems.Select(x=>x.FmtDisplayName).ToArray();
			cbFormat.SelectedIndex = Math.Clamp(SelectedFormatIndex, 0, Math.Max(0, FormatItems.Count-1));
			FlyoutBase.ShowAttachedFlyout(btn);
		};
		_BtnMenu = btn;
		_ComboBoxFormat = cbFormat;
		_Calendar = calendar;
		_MenuFlyout = flyout;
		return btn;
	}

	void SyncUiFromTempus(){
		if(_Input is null || _Calendar is null || _BtnMenu is null){
			return;
		}
		_SyncingUi = true;
		_Input.Text = FormatBySelectedFormat(Tempus);
		if(_ComboBoxFormat is not null){
			_ComboBoxFormat.ItemsSource = FormatItems.Select(x=>x.FmtDisplayName).ToArray();
			_ComboBoxFormat.SelectedIndex = Math.Clamp(SelectedFormatIndex, 0, Math.Max(0, FormatItems.Count-1));
		}
		if(Tempus is null){
			_Calendar.SelectedDate = null;
		}else{
			var localDate = DateTimeOffset.FromUnixTimeMilliseconds(Tempus.Value.Value).ToLocalTime().Date;
			_Calendar.SelectedDate = localDate;
		}
		_SyncingUi = false;
		LastParseOk = true;
		RestoreInputVisual();
	}

	void ApplyReadOnlyState(){
		if(_Input is not null){
			_Input.IsReadOnly = IsReadOnly;
		}
		if(_BtnMenu is not null){
			_BtnMenu.IsEnabled = !IsReadOnly;
		}
		if(_ComboBoxFormat is not null){
			_ComboBoxFormat.IsEnabled = !IsReadOnly;
		}
		if(_Calendar is not null){
			_Calendar.IsEnabled = !IsReadOnly;
		}
		if(IsReadOnly){
			_MenuFlyout?.Hide();
		}
	}

	void ApplyControlSize(){
		if(_Input is not null){
			_Input.Height = ControlHeight;
		}
		if(_BtnMenu is not null){
			_BtnMenu.Height = ControlHeight;
		}
	}

	str FormatBySelectedFormat(UnixMs? Value){
		if(Value is null){
			return "";
		}
		var item = SelectedFormat;
		var converted = item.Converter.Convert(Value.Value, typeof(str), null, CultureInfo.InvariantCulture);
		if(converted is str s){
			return s;
		}
		return Value.Value.Value.ToString(CultureInfo.InvariantCulture);
	}

	bool TryParseBySelectedFormat(str Text, out UnixMs Result){
		Result = default;
		if(string.IsNullOrWhiteSpace(Text)){
			return false;
		}
		var item = SelectedFormat;
		var converted = item.Converter.ConvertBack(Text, typeof(UnixMs), null, CultureInfo.InvariantCulture);
		if(converted is UnixMs t){
			Result = t;
			return true;
		}
		return false;
	}

	void TriggerInvalidInputCommand(str text){
		if(InvalidInputCommand is null){
			return;
		}
		var param = new InvalidCommandParam{
			Self = this,
			Text = text,
		};
		if(InvalidInputCommand.CanExecute(param)){
			InvalidInputCommand.Execute(param);
		}
	}

	void ApplyInvalidInputVisual(){
		if(_Input is null){
			return;
		}
		_Input.BorderBrush = Brushes.Red;
		_Input.Foreground = Brushes.Red;
	}

	void RestoreInputVisual(){
		if(_Input is null){
			return;
		}
		// 清除本地值，回退到主題/樣式原本外觀
		_Input.ClearValue(TextBox.BorderBrushProperty);
		_Input.ClearValue(TextBox.ForegroundProperty);
	}

	sealed class DelegateCommand(Action<object?> execute, Func<object?, bool>? canExecute = null): ICommand{
		readonly Action<object?> _Execute = execute;
		readonly Func<object?, bool>? _CanExecute = canExecute;
		public event EventHandler? CanExecuteChanged;
		public bool CanExecute(object? parameter){
			return _CanExecute?.Invoke(parameter) ?? true;
		}
		public void Execute(object? parameter){
			_Execute(parameter);
		}
		public void RaiseCanExecuteChanged(){
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
