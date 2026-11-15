// 從 Tsinswreng.AvlnTools 原樣搬入（該庫尚未遷入 TswgLib）。
namespace Ngan.Dict.Ui.Components;


using Ngan.Dict.Ui.Components.SwipeLongPress;
using Avalonia.Controls;
using Avalonia.Media;
using Tsinswreng.Avln.Dsl;
using Tsinswreng.Avln.Grid;
using Ngan.Dict.Ui.Infra.AvlnSugar;
using Ngan.Dict.Ui.Infra.AvlnSugar;



public partial class MsgBox : UserControl{
	// public Ctx? Ctx{
	// 	get{return DataContext as Ctx;}
	// 	set{DataContext = value;}
	// }

	public MsgBox(){
		//Ctx = new Ctx();
		Render();
		_Style();
	}

	public partial class Cls_{

	}
	public Cls_ Cls{get;set;} = new Cls_();

	public GridStack Root {get;protected set;} = new(IsRow: true);

	// public Func<nil> OnLeftBtn = ()=>Nil;
	// public Func<nil> OnRightBtn = ()=>Nil;

	public Border _Border{get;protected set;} = new ();
	public Border _BdrTitle{get; protected set;} = new ();
	public ContentControl _Title{get;} = new();
	
	/// 不蔿此設高度則恐 ScrollViewer不效
	
	public Border _BdrBody{get; protected set;} = new ();
	public ContentControl _Body{get;} = new();
	public SwipeLongPressBtn _CloseBtn{get; protected set;}
	public Border _BdrBottomView{get; protected set;} = new();
	public ContentControl _BottomView{get;} = new();



	protected nil _Style(){

		return NIL;
	}

	protected nil Render(){
		//Content = Root.Grid;
		// Content = _Border;
		// _Border.Child = Root.Grid;
		this.SetContent(_Border, o=>{
			o.Child = Root.Grid;
		});
		{var o = Root.Grid;

			o.RowDefinitions.AddRange([
				new(1, GUT.Auto),//Title
				new(1, GUT.Auto),//Body
				new(1, GUT.Auto),//Bottom
			]);
		}
		{{
			var TitleRow = new GridStack(IsRow: false);
			_BdrTitle.Child = TitleRow.Grid;
			Root.Add(_BdrTitle);
			{var o = TitleRow.Grid;
				o.ColumnDefinitions.AddRange([
					new(999, GUT.Star),
					new(1, GUT.Auto),
				]);
			}
			{{
				TitleRow.A(_Title, o=>{
					o.VerticalAlignment = VAlign.Center;
				});

				_CloseBtn = new SwipeLongPressBtn{};
				TitleRow.A(_CloseBtn, o=>{
					o.Content = "×";
					o.HorizontalAlignment = HAlign.Right;
					o.Background = Brushes.Red;
				});
			}}//~TitleLine


			Root.A(_BdrBody, o=>{
				var Scr = new ScrollViewer();
				o.Child = Scr;
				{{
					Scr.Content = _Body;
				}}//~Scr
			});
			Root.A(_BdrBottomView);
			_BdrBottomView.Child = _BottomView;
		}}//~Root
		return NIL;
	}

	// protected Style _Sty2Btn(){
	// 	// o.VerticalAlignment = VertAlign.Stretch;
	// 	// o.HorizontalAlignment = HoriAlign.Stretch;
	// 	// var o = new Style(x=>
	// 	// 	x.Is<Button>()
	// 	// );
	// 	// o.Set(
	// 	// 	Button.VerticalContentAlignmentProperty
	// 	// 	,VAlign.Center
	// 	// );
	// 	// o.Set(
	// 	// 	Button.HorizontalContentAlignmentProperty
	// 	// 	,HoriAlign.Center
	// 	// );
	// 	// o.Set(
	// 	// 	Button.VerticalAlignmentProperty
	// 	// 	,VertAlign.Stretch
	// 	// );
	// 	// o.Set(
	// 	// 	Button.HorizontalAlignmentProperty
	// 	// 	,HoriAlign.Stretch
	// 	// );
	// 	// return o;
	// }


}
