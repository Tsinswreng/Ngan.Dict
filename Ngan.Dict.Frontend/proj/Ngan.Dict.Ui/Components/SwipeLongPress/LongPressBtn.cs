// 從 Tsinswreng.AvlnTools（0.3 系）原樣搬入本專案：滑動／長按按鈕族。
// 該庫尚未遷入 TswgLib，而本項目只用到 SwipeLongPressBtn 這一個控件；
// 為避免整包 AvlnTools 的依賴，只搬它與其基底（OpenButton／SwipeBtn／LongPressBtn）與介面，行為不變。
using Ngan.Dict.Ui.Components.SwipeLongPress;
using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia;
using Avalonia.Threading;
using Tsinswreng.CsCore;
//TOFIX 觸屏時邊移動邊長按(如scrollView中上下滑 亦祘)
namespace Ngan.Dict.Ui.Components.SwipeLongPress;

using Ngan.Dict.Ui.Components.SwipeLongPress.IF;

//TODO 區分ʃᶤ長按ʹ鍵 左鍵抑右鍵
public partial class LongPressBtn
	:OpenButton
	,ILongPressBtn
{
	public long LongPressDurationMs{
		get{return FnLongPressBtn.LongPressDurationMs;}
		set{FnLongPressBtn.LongPressDurationMs = value;}
	}
	public event EventHandler? OnLongPressed;

	public LongPressBtnFn FnLongPressBtn{get;set;} = new LongPressBtnFn();
	public LongPressBtn():base(){
		FnLongPressBtn.Init();
		//_longPressBtnFn.onClick = ()=>{OnClick();return Nil;};
		FnLongPressBtn.OnLongPress = ()=>{
			OnLongPressed?.Invoke(this, EventArgs.Empty);
			return NIL;
		};
	}

	protected override void OnPointerPressed(PointerPressedEventArgs e){
		base.OnPointerPressed(e);
		FnLongPressBtn._OnPointerPressed(e.GetPosition(this));
	}

	protected override void OnPointerReleased(PointerReleasedEventArgs e){
		base.OnPointerReleased(e);
		FnLongPressBtn._OnPointerReleased(e);
	}

	protected override void OnPointerMoved(PointerEventArgs e){
		base.OnPointerMoved(e);
		FnLongPressBtn._OnPointerMoved(e.GetPosition(this));
	}

	protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e){
		base.OnPointerCaptureLost(e);
		FnLongPressBtn._OnPointerCaptureLost();
	}

	protected override void OnClick(){
		if(!FnLongPressBtn._OnClick()){
			return;
		}
		base.OnClick();
	}

	#region Fn


/// Call Init() after construct

public  partial class LongPressBtnFn
	:I_Init
{
	protected DispatcherTimer _PressTimer;
	protected bool _IsLongPressTriggered;
	protected bool _IsPointerPressed;
	protected Point _PressStartPoint;
	protected f64 _MoveCancelThresholdPx = 8;

	protected bool _HasLongPressed = false;
	protected i64 _LongPressDurationMs = 500;
	public i64 LongPressDurationMs{
		get{return _LongPressDurationMs;}
		set{
			_LongPressDurationMs = value;
			if(_PressTimer != null){
				_PressTimer.Interval = TimeSpan.FromMilliseconds(value);
			}
		}
	}
	//public Func<zero> onClick{get;set;} = ()=>0; // 点击事件
	public Func<nil> OnLongPress{get;set;} = ()=>0; // 长按事件

	bool _Inited = false;
	public nil Init(){
		if(_Inited){return NIL;}
		_PressTimer = new DispatcherTimer(){
			Interval = TimeSpan.FromMilliseconds(LongPressDurationMs)
		};
		_PressTimer.Tick += OnTimerElapsed;
		_Inited = true;
		return NIL;
	}

	public nil _OnPointerPressed(Point pressStartPoint){
		_IsLongPressTriggered = false;
		_HasLongPressed = false;
		_IsPointerPressed = true;
		_PressStartPoint = pressStartPoint;
		_PressTimer.Start();
		return NIL;
	}

	public nil _OnPointerReleased(PointerReleasedEventArgs e){
		_IsPointerPressed = false;
		_PressTimer.Stop(); // 松开时停止计时器
		if (!_IsLongPressTriggered) {
			// 未触发长按时，执行点击逻辑
			//OnClick();
			//onClick?.Invoke();
		}
		_IsLongPressTriggered = false;
		return NIL;
	}

	public nil _OnPointerMoved(Point p){
		if(!_IsPointerPressed || _IsLongPressTriggered){
			return NIL;
		}
		var dx = p.X - _PressStartPoint.X;
		var dy = p.Y - _PressStartPoint.Y;
		var moved2 = dx*dx + dy*dy;
		var threshold2 = _MoveCancelThresholdPx * _MoveCancelThresholdPx;
		if(moved2 > threshold2){
			_PressTimer.Stop();
			_IsPointerPressed = false;
		}
		return NIL;
	}

	public nil _OnPointerCaptureLost(){
		_IsPointerPressed = false;
		_PressTimer.Stop();
		_IsLongPressTriggered = false;
		return NIL;
	}

	private void OnTimerElapsed(object? sender, EventArgs e){
		_IsLongPressTriggered = true;
		_PressTimer.Stop();
		// 触发长按事件
		OnLongPress?.Invoke();
		_HasLongPressed = true;
	}


	public bool _OnClick(){
		if (_HasLongPressed) {
			_HasLongPressed = false;
			return false;
		}
		return true;
	}

	// public event EventHandler? LongPressed;
	// public virtual void OnLongPress(){
	// 	LongPressed?.Invoke(this, EventArgs.Empty);
	// }

}


	#endregion



}
