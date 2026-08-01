namespace Ngaq.Ui.Infra.Ctrls;

using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using Avalonia.Threading;
using Ngaq.Ui.Infra;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.CsCore;
using Ctx = VmOpBtn;

[Doc(@$"Operation Button.
when processing, underline will have rolling bar.
設置背景要用 _Button.Background、不能直接.Backround
")]
public partial class OpBtn : ContentControl {

	public enum EState {
		Ready,
		Working,
		Disabled
	}
	public Button _Button { get; set; } = new();
	public CancellationTokenSource Cts { get; set; } = new();
	public Func<CT, Task<nil>?>? FnExeAsy { get; set; } = async (Ct) => NIL;
	public Func<obj?>? FnOk { get; set; }
	public Func<Exception?, obj?>? FnFail { get; set; }
	public Func<obj?>? FnCancel { get; set; }
	public EState State { get; set; }

	public Grid Grid = new();

	public Control Overlay;
	void Start() {
		Cts = new();
		State = EState.Working;
		SetupOverlay();
	}

	void End() {
		Dispatcher.UIThread.Post(() => {
			State = EState.Ready;
			SetdownOverlay();
		});
	}

	void Cancel() {
		Cts.Cancel();
		SetdownOverlay();
	}

	void SetdownOverlay() {
		// if(Grid.Children.Count > 1){
		// 	Grid.Children.RemoveAt(Grid.Children.Count-1);
		// }
		if (Grid.Children.Count > 1) {
			var overlay = Grid.Children[1];
			overlay.IsVisible = false;
		}
	}

	void SetupOverlay() {
		//
		if (Grid.Children.Count > 1) {
			var overlay = Grid.Children[1];
			overlay.IsVisible = true;
		} else {
			Overlay.IsVisible = true;
			Grid.Children.Add(Overlay);
			//Grid.SetRow(Overlay, 1);
		}
	}

	public obj? BtnContent { get => _Button?.Content; set => _Button.Content = value; }

	// 在 OpBtn 類內部加一行
	public void PerformClick() {
		// 把按鈕的 Click 事件手動拋出去
		_Button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
	}

	public OpBtn() {
		base.Content = Grid;
		Grid.SetRowDefs([
			new(1, GUT.Star),
			//RowDef(1, GUT.Auto),
		]);
		Grid.A(_Button, b=>{
			b.VAlign(x=>x.Stretch);
			b.HAlign(x=>x.Stretch);
		});

		var FnMkOverlay = () => {
			return MkOverlay();
		};
		Overlay = FnMkOverlay();

		_Button.Click += (s, e) => {
			if (State == EState.Working) {
				Cancel();
				return;
			}

			Start();
			var R = FnExeAsy(Cts.Token);
			if (R is null) {
				End();
				return;
			}
			R.ContinueWith(t => {
				if (t.IsCanceled) {
					FnCancel?.Invoke();
				} else if (t.IsFaulted) {
					FnFail?.Invoke(t.Exception.InnerException);
				} else {
					FnOk?.Invoke();
				}
				End();
			});
		};
	}

	public Control MkOverlay() {
		// 1. 還是你原來的 ProgressBar，啥也不改
		var bar = new ProgressBar {
			IsIndeterminate = true,
			VerticalAlignment = VAlign.Bottom,
			HorizontalAlignment = HAlign.Stretch,
			Height = 4,
			Margin = new(0),
			IsHitTestVisible = false
		};

		// 2. 包一層「零尺寸」殼，讓 Grid 量不到
		return new ZeroDecorator { Child = bar };
	}

}

class ZeroDecorator : Decorator {
	protected override Size MeasureOverride(Size availableSize)
		=> new Size(0, 0);   // 永遠報 0×0，父級就不會被撐開
}
