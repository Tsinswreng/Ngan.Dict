// 從 Tsinswreng.AvlnTools（0.3 系）原樣搬入本專案：滑動／長按按鈕族。
// 該庫尚未遷入 TswgLib，而本項目只用到 SwipeLongPressBtn 這一個控件；
// 為避免整包 AvlnTools 的依賴，只搬它與其基底（OpenButton／SwipeBtn／LongPressBtn）與介面，行為不變。

using Ngan.Dict.Ui.Components.SwipeLongPress;
using System;
using Avalonia.Controls;
using Avalonia.Input;

namespace Ngan.Dict.Ui.Components.SwipeLongPress;

using Ngan.Dict.Ui.Components.SwipeLongPress.IF;
using SwipeEventArgs = Ngan.Dict.Ui.Components.SwipeLongPress.IF.ISwipeBtn.SwipeEventArgs;
using SwipeDirection = Ngan.Dict.Ui.Components.SwipeLongPress.IF.ISwipeBtn.SwipeDirection;

public  partial class SwipeLongPressBtn
	:OpenButton
	,ISwipeBtn
	,ILongPressBtn
{

	public f64 SwipeThreshold{
		get{return _SwipeBtnFn.SwipeThreshold;}
		set{_SwipeBtnFn.SwipeThreshold=value;}
	}

	public event EventHandler<SwipeEventArgs>? OnSwipe;
	public SwipeBtn.SwipeBtnFn _SwipeBtnFn{get;set;}=new ();


	public long LongPressDurationMs{
		get{return _LongPressBtnFn.LongPressDurationMs;}
		set{_LongPressBtnFn.LongPressDurationMs = value;}
	}
	public event EventHandler? OnLongPressed;

	public LongPressBtn.LongPressBtnFn _LongPressBtnFn{get;set;} = new();

	protected override Type StyleKeyOverride => typeof(Button);
	public SwipeLongPressBtn():base(){
		_SwipeBtnFn.OnSwipe = (e)=>{
			OnSwipe?.Invoke(this,e);
			return NIL;
		};
		_LongPressBtnFn.Init();
		//_longPressBtnFn.onClick = ()=>{OnClick();return Nil;};
		_LongPressBtnFn.OnLongPress = ()=>{
			OnLongPressed?.Invoke(this, EventArgs.Empty);
			return NIL;
		};
	}

	protected override void OnPointerPressed(PointerPressedEventArgs e) {
		_SwipeBtnFn._OnPointerPressed(this,e);
		_LongPressBtnFn._OnPointerPressed(e.GetPosition(this));
		base.OnPointerPressed(e);
	}

	protected override void OnPointerMoved(PointerEventArgs e){
		_LongPressBtnFn._OnPointerMoved(e.GetPosition(this));
		base.OnPointerMoved(e);
	}

	protected override void OnPointerReleased(PointerReleasedEventArgs e) {
		_SwipeBtnFn._OnPointerReleased(this,e);
		_LongPressBtnFn._OnPointerReleased(e);
		base.OnPointerReleased(e);
	}

	protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e){
		_LongPressBtnFn._OnPointerCaptureLost();
		base.OnPointerCaptureLost(e);
	}


	protected override void OnClick(){
		if(!_LongPressBtnFn._OnClick()){
			return;
		}
		base.OnClick();
	}

}
