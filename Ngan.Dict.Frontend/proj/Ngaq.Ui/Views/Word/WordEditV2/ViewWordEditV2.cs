namespace Ngaq.Ui.Views.Word.WordEditV2;

using Avalonia.Controls;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Views.Word.WordEditV2.PoWordEdit;
using Ngaq.Ui.Views.Word.WordEditV2.WordLearnPage;
using Ngaq.Ui.Views.Word.WordEditV2.WordPropPage;
using Tsinswreng.Avln.Grid;
using Ctx = VmWordEditV2;

[Doc(@$"
單詞編輯主頁：承載三個分頁，並提供全局保存與刪除入口。
操作儘量原子化、一個操作對應一個後端接口、
不要在前端做「先編輯 再保存」、否則會有大量複雜邏輯判斷。
")]
public partial class ViewWordEditV2: AppViewBase<Ctx>{
	/// 基本資料分頁的抽象介面，供宿主存取表單資料。
	public ViewPoWordEdit? PoWordEdit{get;set;}
	/// 單詞屬性分頁，供宿主存取屬性列表與新增入口。
	public ViewWordPropPage? WordPropPage{get;set;}
	/// 單詞學習記錄分頁，供宿主存取學習列表與新增入口。
	public ViewWordLearnPage? WordLearnPage{get;set;}
	/// 固定承載分頁與錯誤列的兩列根版面。
	public GridStack Root = new(IsRow: true);
	/// 建立頁面、取得 ViewModel，並掛接子頁編輯事件。
	public partial ViewWordEditV2();

	partial void Render();
	/// 將列表子頁的單行編輯請求導向對應編輯 View。
	partial void HookSubPageEvents();
	/// 建立基本資料、屬性與學習記錄分頁。
	private partial Control MkTabs();

	/// 建立與 ViewModel 錯誤狀態同步的提示列。
	private partial Control MkErrBar();
}
