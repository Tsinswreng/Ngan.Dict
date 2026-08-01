using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using Ngaq.Ui.Infra.I18n;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsCore;
using Tsinswreng.CsI18n;

namespace Ngaq.Ui.Views;

public partial class MainView : UserControl {
	public static MainView Inst{get;protected set;} =new();

	[Doc(@$"造按鈕、點後跳到目標視圖")]
	public partial Button MkBtnToView(
		Control Target
		,str? Title = null
	);

	[Doc(@$"造按鈕、點後跳到目標視圖（延遲建構）")]
	public partial Button MkBtnToView(
		Func<Control> MkTarget
		,str? Title = null
	);
	public II18n I18n{get;set;} = AppI18n.Inst;
	public SvcPopup SvcPopup{get;set;}
	public GridStack GridStack = new (IsRow: true);
	public Grid Root{get{return GridStack.Grid;}}
	public ViewNaviBase ViewNaviBase{get;} = new ();
	public ILogger? Logger{get=>App.Logger;set{}}

	[Doc(@$"前端拿到異常後處理之")]
	public partial nil HandleErr(obj? Ex);

	[Doc(@$"可關閉彈窗")]
	public partial nil ShowDialog(str Msg);

	[Doc(@$"可關閉彈窗、執行操作。
	不要在ViewModel層調用此函數、因爲這違反Mvvm規範。
	#Params([],[縱向生成按鈕列])

	")]
	public partial nil ShowDialog(str Msg, IList<Button> Operations);

	[Doc(@$"處理返回動作。
	優先關閉彈窗；若無彈窗則返回上一級導航。
	#Returns([是否已消費此次返回事件])
	")]
	public partial bool TryHandleBackNavigation();

	[Doc(@$"")]
	public partial nil ShowToast(str Msg, u64 DurationMs = 3000);


}
