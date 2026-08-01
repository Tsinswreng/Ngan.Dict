using Avalonia.Controls;
using System;

namespace Ngaq.Ui.Views.BottomBar;

public partial class Btn_Control{

	public Btn_Control(Button Button, Control Control){
		this.Button = Button;
		this.Control = Control;
		this.MkControl = ()=>Control;
	}

	public Btn_Control(Button Button, Func<Control> MkControl){
		this.Button = Button;
		this.MkControl = MkControl;
	}

	public Button Button{get;set;}
	public Control? Control{get;protected set;}
	public Func<Control> MkControl{get;protected set;}

	public Control GetOrCreateControl(){
		Control ??= MkControl();
		return Control;
	}

}
