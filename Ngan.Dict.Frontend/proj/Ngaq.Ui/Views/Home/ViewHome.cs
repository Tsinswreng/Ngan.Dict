namespace Ngaq.Ui.Views.Home;

using Avalonia.Controls;
using Avalonia.Media;
using Ngaq.Ui.Views.BottomBar;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.AvlnTools.Dsl;
using Ctx = VmHome;
using Ngaq.Ui.Views.Dictionary;
using Ngaq.Ui.Views.User.AboutMe;

using Ngaq.Ui.Icons;
using Avalonia;
using Ngaq.Ui.Views.Word.Learn;
using Tsinswreng.CsI18n;
using Ngaq.Ui.Infra;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;
using Tsinswreng.Avln.Grid;

public partial class ViewHome
	:AppViewBase<Ctx>
{

	public ViewHome(){
		Ctx = new Ctx();
		Style();
		Render();
		//Ctx.ViewNavi = MgrViewNavi.ViewNavi;
	}

	public  partial class Cls_{

	}
	public Cls_ Cls{get;set;} = new Cls_();

	protected nil Style(){
		return NIL;
	}

	GridStack Root = new(IsRow:true);
	readonly ViewDictionary DictionaryView = new();
	ViewBottomBar? BottomBar;

	protected nil Render(){
		this.SetContent(Root.Grid, o=>{
			o.SetRowDefs([
				new(20, GUT.Star),
			]);
		});

		var BarItem = (str Title, Control Icon)=>{
			var R = StrBarItem.Inst.BarItem(Title, Icon);
			//不效
			// if (R.Content is Grid grid && grid.Children.Count >= 2 && grid.Children[1] is Control ctrl){
			// 	ctrl.Margin = new(0, 0, 0, 10); // 👈 關鍵：往下留少許空間，視覺上往上移
			// }
			return R;
		};

		Root.A(new ViewBottomBar(), ViewBottomBar=>{
			BottomBar = ViewBottomBar;
			ViewBottomBar.Items.A(new Btn_Control(
					BarItem(I[K.Learn], Icons.Learn().ToIcon())//📖
					,()=>new ViewLearnWords()
				),
				o=>{
					ViewBottomBar.Cur.Content = o.GetOrCreateControl();
					o.Button.Background = Brushes.Transparent;
				}
			).A(
				new Btn_Control(
					BarItem(I[K.Dictionary], Icons.Dictionary().ToIcon())//📚
					,DictionaryView
				)
			).A(
				new Btn_Control(
					BarItem(I[K.Me], Icons.User().ToIcon())//👤
					,()=>new ViewAboutMe()
				)
			)
			;

			// ViewBottomBar.Items.AddInitT(
			// 	new Btn_Control(
			// 		BarItem(I[K.Learn], Svgs.BookOpenTextFill().ToIcon())//📖
			// 		,ViewWordQuery
			// 	),
			// 	o=>{
			// 		ViewBottomBar.Cur.Content = o.Control;
			// 		o.Button.Background = Brushes.Transparent;
			// 	}
			// ).AddInitT(new Btn_Control(
			// 	BarItem(I[K.Library], Svgs.BookBookmarkFill().ToIcon())//📚
			// 	,new ViewWordManage()
			// ))
			// .AddInitT(new Btn_Control(
			// 	BarItem(I[K.Me], Svgs.UserCircleFill().ToIcon())//👤
			// 	,new ViewAboutMe()
			// ));
		});

		return NIL;
	}

	/// 切換到「字典」底欄頁簽，並可選擇直接查詞。
	/// <param name="Term">待查詞條；空則僅切換頁簽。</param>
	/// <returns>空值。</returns>
	public nil OpenDictionaryTab(str? Term = null){
		BottomBar?.SelectControl(DictionaryView);
		if(!str.IsNullOrWhiteSpace(Term)){
			DictionaryView.ClickLookupBtn(Term);
		}
		return NIL;
	}


}
