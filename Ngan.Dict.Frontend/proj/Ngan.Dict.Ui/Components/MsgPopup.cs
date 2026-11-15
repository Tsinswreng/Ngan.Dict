// 從 Tsinswreng.AvlnTools 原樣搬入（該庫尚未遷入 TswgLib）。
namespace Ngan.Dict.Ui.Components;
using Ngan.Dict.Ui.Components.SwipeLongPress;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Tsinswreng.Avln.Dsl;


public partial class MsgPopup : UserControl{
	public MsgBox _MsgBox = new();
	public Border _Border{get{
		return _MsgBox._Border;
	}}
	public Border _BdrTitle{get{
		return _MsgBox._BdrTitle;
	}}
	public ContentControl _Title{get{
		return _MsgBox._Title;
	}}
	public Border _BdrBody{get{
		return _MsgBox._BdrBody;
	}}
	public ContentControl _Body{get{
		return _MsgBox._Body;
	}}
	public SwipeLongPressBtn _CloseBtn{get{
		return _MsgBox._CloseBtn;
	}}
	public Border _BdrBottomView{get{
		return _MsgBox._BdrBottomView;
	}}
	public ContentControl _BottomView{get{
		return _MsgBox._BottomView;
	}}

	public Popup _Popup;

	public MsgPopup(){
		_Style();
		Render();
		_MsgBox._CloseBtn.Click += (s,e)=>{
			if(_Popup != null){
				_Popup.IsOpen= false;
			}
		};
	}

	protected nil _Style(){
		//Styles.Add(SugarStyle.GridShowLines());
		return NIL;
	}

	protected nil Render(){
		var Top = TopLevel.GetTopLevel(this);
		_Popup = new Popup{};
		this.SetContent(_Popup, o=>{
			o.Child = _MsgBox;
			//o.HorizontalOffset = TopLevel.GetTopLevel
			o.PlacementTarget = Top;
			o.Placement = PlacementMode.Center;
			//o.Placement = PlacementMode.Top;
			o.IsOpen = true;
			o.IsHitTestVisible = true;
			//o.StaysOpen = false // 点击其它地方自动关闭
		});
		return NIL;
	}
}
