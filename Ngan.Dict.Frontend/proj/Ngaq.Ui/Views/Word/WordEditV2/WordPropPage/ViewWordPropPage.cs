namespace Ngaq.Ui.Views.Word.WordEditV2.WordPropPage;

using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.Layout;
using Avalonia.Media;
using Ngaq.Core.Infra;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;
using Avalonia;
using Avalonia.Interactivity;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Infra.Ctrls;
using Tsinswreng.Avln.Grid;
using Tsinswreng.Avln.Dsl;

/// 屬性分頁：列表 + 新增，點行進入編輯頁。
public partial class ViewWordPropPage
	: AppViewBase
{
	public VmWordPropPage? Ctx{
		get{return DataContext as VmWordPropPage;}
		set{DataContext = value;}
	}

	[Impl]
	public IBtn BtnAddProp{get;set;} = new Btn();

	[Impl]
	public TreeDataGrid? Rows{
		get{return Grid;}
	}

	TreeDataGrid? Grid;
	INotifyCollectionChanged? RowsNotifier;
	VmWordPropPage? SubscribedCtx;

	public partial ViewWordPropPage();

	/// 依既定版面與綁定狀態建立此頁的控制項樹。
	partial void Render();

	/// 建立 BtnAdd 所需的 UI 組件，供頁面組裝時重用。
	private partial OpBtn MkBtnAdd();

	/// 建立 Grid 所需的 UI 組件，供頁面組裝時重用。
	private partial Control MkGrid();

	/// 依目前 ViewModel 狀態重建呈現資料，確保列表與編輯狀態一致。
	partial void RebuildGrid();

	/// `Ctx` 是在構造後才灌入的，列表頁需在此時補掛監聽並首次建表格源。
	partial void OnCtxChanged();

	/// `LoadFromPoProps` 會整體替換 `Rows`，需重新掛上集合變更監聽。
	partial void OnCtxPropertyChanged(object? Sender, PropertyChangedEventArgs E);

	/// 掛接必要的資料或事件監聽，讓後續狀態變更能反映到畫面。
	partial void HookRowsChanged();

	/// 處理 RowsChanged 事件，維持頁面狀態與資料來源同步。
	partial void OnRowsChanged(object? Sender, NotifyCollectionChangedEventArgs E);

	/// 根據目前狀態取得對應的顯示或轉換結果。
	private partial str GetIdxText(VmWordPropRow Row);

	/// 處理 GridTapped 事件，維持頁面狀態與資料來源同步。
	partial void OnGridTapped(object? Sender, TappedEventArgs E);
}
