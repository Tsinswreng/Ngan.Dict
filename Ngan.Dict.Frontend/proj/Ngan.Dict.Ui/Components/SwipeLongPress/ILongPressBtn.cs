// 從 Tsinswreng.AvlnTools（0.3 系）原樣搬入本專案：滑動／長按按鈕族。
// 該庫尚未遷入 TswgLib，而本項目只用到 SwipeLongPressBtn 這一個控件；
// 為避免整包 AvlnTools 的依賴，只搬它與其基底（OpenButton／SwipeBtn／LongPressBtn）與介面，行為不變。
using Ngan.Dict.Ui.Components.SwipeLongPress;
using System;

namespace Ngan.Dict.Ui.Components.SwipeLongPress.IF;

public  partial interface ILongPressBtn{
	public event EventHandler? OnLongPressed;
	public i64 LongPressDurationMs{get;set;}
}
