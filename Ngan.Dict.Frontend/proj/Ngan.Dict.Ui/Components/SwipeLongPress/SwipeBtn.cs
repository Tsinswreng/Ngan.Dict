// 從 Tsinswreng.AvlnTools（0.3 系）原樣搬入本專案：滑動／長按按鈕族。
// 該庫尚未遷入 TswgLib，而本項目只用到 SwipeLongPressBtn 這一個控件；
// 為避免整包 AvlnTools 的依賴，只搬它與其基底（OpenButton／SwipeBtn／LongPressBtn）與介面，行為不變。
using Ngan.Dict.Ui.Components.SwipeLongPress;
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;


namespace Ngan.Dict.Ui.Components.SwipeLongPress;

using Ngan.Dict.Ui.Components.SwipeLongPress.IF;
using SwipeEventArgs = Ngan.Dict.Ui.Components.SwipeLongPress.IF.ISwipeBtn.SwipeEventArgs;
using SwipeDirection = Ngan.Dict.Ui.Components.SwipeLongPress.IF.ISwipeBtn.SwipeDirection;

public  partial class SwipeBtn
	:OpenButton
	,ISwipeBtn
{
	public f64 SwipeThreshold{
		get{return _SwipeBtnFn.SwipeThreshold;}
		set{_SwipeBtnFn.SwipeThreshold=value;}
	}

	public event EventHandler<SwipeEventArgs>? OnSwipe;
	public SwipeBtnFn _SwipeBtnFn{get;set;}=new SwipeBtnFn();

	public SwipeBtn():base(){
		//_swipeBtnFn.init();
		_SwipeBtnFn.OnSwipe = (e)=>{
			OnSwipe?.Invoke(this,e);
			return NIL;
		};
	}

	protected override void OnPointerPressed(PointerPressedEventArgs e) {
		base.OnPointerPressed(e);
		_SwipeBtnFn._OnPointerPressed(this,e);
	}

	protected override void OnPointerReleased(PointerReleasedEventArgs e) {
		base.OnPointerReleased(e);
		_SwipeBtnFn._OnPointerReleased(this,e);
	}


#region Fn
public  partial class SwipeBtnFn{
	public Point StartPoint{get;set;}
	public bool IsSwiping{get;set;}
	public f64 SwipeThreshold{get;set;}=50.0;

	// 定义自定义滑动事件
	//public event EventHandler<SwipeEventArgs>? Swipe;
	public Func<SwipeEventArgs, nil>? OnSwipe;

	// public Func<PointerPressedEventArgs, zero> fn_OnPointerPressed(Button z){
	// 	return (PointerPressedEventArgs e)=>{
	// 		_startPoint = e.GetPosition(z); // 记录初始触摸点
	// 		_isSwiping = true;
	// 		e.Handled = true; // 阻止事件冒泡
	// 		return Nil;
	// 	};
	// }

	public nil _OnPointerPressed(Button z, PointerPressedEventArgs e){
		StartPoint = e.GetPosition(z); // 记录初始触摸点
		IsSwiping = true;
		e.Handled = true; // 阻止事件冒泡
		return NIL;
	}

	public nil _OnPointerReleased(Button z, PointerReleasedEventArgs e){
		if (!IsSwiping){return NIL;}
		var currentPoint = e.GetPosition(z);
		var delta = currentPoint - StartPoint;
		// 判断滑动方向（可调整阈值）
		if (Math.Abs(delta.X) > SwipeThreshold || Math.Abs(delta.Y) > SwipeThreshold) {
			var direction = _getSwipeDirection(delta);
			//Swipe?.Invoke(this, new SwipeEventArgs(direction));//TODO
			OnSwipe?.Invoke(new SwipeEventArgs(direction));
			IsSwiping = false; // 触发后结束滑动
		}
		return NIL;
	}

	// public Func<PointerEventArgs, zero> fn_OnPointerMoved(Button z){
	// 	return (e)=>{
	// 		if (!_isSwiping){return Nil;}
	// 		var currentPoint = e.GetPosition(z);
	// 		var delta = currentPoint - _startPoint;
	// 		// 判断滑动方向（可调整阈值）
	// 		if (Math.Abs(delta.X) > swipeThreshold || Math.Abs(delta.Y) > swipeThreshold) {
	// 			var direction = _getSwipeDirection(delta);
	// 			Swipe?.Invoke(this, new SwipeEventArgs(direction));
	// 			_isSwiping = false; // 触发后结束滑动
	// 		}
	// 		return Nil;
	// 	};
	// }

	public static SwipeDirection _getSwipeDirection(Vector delta) {
		if (Math.Abs(delta.X) > Math.Abs(delta.Y)) {
			return delta.X > 0 ? SwipeDirection.Right : SwipeDirection.Left;
		} else {
			return delta.Y > 0 ? SwipeDirection.Down : SwipeDirection.Up;
		}
	}
}

// public  partial class SwipeEventArgs : EventArgs {
// 	public SwipeDirection Direction { get; }

// 	public SwipeEventArgs(SwipeDirection direction) {
// 		Direction = direction;
// 	}
// }


// public enum SwipeDirection {
// 	Up,
// 	Down,
// 	Left,
// 	Right
// }
#endregion


}
