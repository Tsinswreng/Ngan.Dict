// 從 Tsinswreng.AvlnTools（0.3 系）原樣搬入本專案：滑動／長按按鈕族。
// 該庫尚未遷入 TswgLib，而本項目只用到 SwipeLongPressBtn 這一個控件；
// 為避免整包 AvlnTools 的依賴，只搬它與其基底（OpenButton／SwipeBtn／LongPressBtn）與介面，行為不變。
/*
Func<$1, zero>
 */
using Ngan.Dict.Ui.Components.SwipeLongPress;
using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace Ngan.Dict.Ui.Components.SwipeLongPress;
public  partial class OpenButton:Button{

	public OpenButton():base(){
		// Avalonia 12 不再提供可覆寫的 OnLostFocus，改由路由事件保留原回調能力。
		LostFocus += (s,e)=>{
			FnOnLostFocus?.Invoke(e);
		};
	}

	protected override Type StyleKeyOverride => typeof(Button);

	#region override
	public Func<KeyEventArgs, nil>? FnOnKeyDown{get;set;}

	public void BOnKeyDown(KeyEventArgs e){
		base.OnKeyDown(e);
	}

	protected override void OnKeyDown(KeyEventArgs e){
		if(FnOnKeyDown == null){
			base.OnKeyDown(e);
			return;
		}
		FnOnKeyDown.Invoke(e);
	}
	//-

	public Func<KeyEventArgs, nil>? FnOnKeyUp{get;set;}
	public void BOnKeyUp(KeyEventArgs e){
		base.OnKeyUp(e);
	}

	protected override void OnKeyUp(KeyEventArgs e){
		if(FnOnKeyUp == null){
			base.OnKeyUp(e);
			return;
		}
		FnOnKeyUp.Invoke(e);
	}
	//-

	public Func<PointerPressedEventArgs, nil>? FnOnPointerPressed{get;set;}
	public void BOnPointerPressed(PointerPressedEventArgs e){
		base.OnPointerPressed(e);
	}

	protected override void OnPointerPressed(PointerPressedEventArgs e){
		if(FnOnPointerPressed == null){
			base.OnPointerPressed(e);
			return;
		}
		FnOnPointerPressed.Invoke(e);
	}
	//-

	public Func<PointerReleasedEventArgs, nil>? FnOnPointerReleased{get;set;}
	public void BOnPointerReleased(PointerReleasedEventArgs e){
		base.OnPointerReleased(e);
	}

	protected override void OnPointerReleased(PointerReleasedEventArgs e){
		if(FnOnPointerReleased == null){
			base.OnPointerReleased(e);
			return;
		}
		FnOnPointerReleased.Invoke(e);
	}
	//-

	public Func<PointerCaptureLostEventArgs, nil>? FnOnPointerCaptureLost{get;set;}
	public void BOnPointerCaptureLost(PointerCaptureLostEventArgs e){
		base.OnPointerCaptureLost(e);
	}

	protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e){
		if(FnOnPointerCaptureLost == null){
			base.OnPointerCaptureLost(e);
			return;
		}
		FnOnPointerCaptureLost.Invoke(e);
	}
	//-

	public Func<RoutedEventArgs, nil>? FnOnLostFocus{get;set;}
	public void BOnLostFocus(RoutedEventArgs e){
		// Avalonia 12 的失焦流程由事件系統負責；此入口保留舊 API 的回調語義。
		FnOnLostFocus?.Invoke(e);
	}
	//-
	public Func<TemplateAppliedEventArgs, nil>? FnOnApplyTemplate{get;set;}
	public void BOnApplyTemplate(TemplateAppliedEventArgs e){
		base.OnApplyTemplate(e);
	}
	protected override void OnApplyTemplate(TemplateAppliedEventArgs e){
		if(FnOnApplyTemplate == null){
			base.OnApplyTemplate(e);
			return;
		}
		FnOnApplyTemplate.Invoke(e);
	}
	//-
	public Func<AvaloniaPropertyChangedEventArgs, nil>? FnOnPropertyChanged{get;set;}
	public void BOnPropertyChanged(AvaloniaPropertyChangedEventArgs change){
		base.OnPropertyChanged(change);
	}
	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change){
		if(FnOnPropertyChanged == null){
			base.OnPropertyChanged(change);
			return;
		}
		FnOnPropertyChanged.Invoke(change);
	}

	#endregion override

	#region virtualOfParent
	//-
	public Action? FnOnClick{get;set;}
	public void BOnClick(){
		base.OnClick();
	}

	protected override void OnClick(){
		if(FnOnClick == null){
			base.OnClick();
			return;
		}
		FnOnClick.Invoke();
	}
	#endregion virtual
}
