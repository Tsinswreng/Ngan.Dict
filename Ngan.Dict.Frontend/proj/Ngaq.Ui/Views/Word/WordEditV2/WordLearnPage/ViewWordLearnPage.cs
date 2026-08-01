namespace Ngaq.Ui.Views.Word.WordEditV2.WordLearnPage;

using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.Media;
using Ngaq.Core.Infra;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;
using Avalonia;
using Ngaq.Ui.Tools;
using Avalonia.Interactivity;
using Tsinswreng.Avln.Grid;
using Tsinswreng.Avln.Dsl;

/// 學習記錄分頁：列表 + 新增，點行進入編輯頁。
public partial class ViewWordLearnPage: AppViewBase{
	public VmWordLearnPage? Ctx{
		get{return DataContext as VmWordLearnPage;}
		set{DataContext = value;}
	}

	TreeDataGrid? Grid;
	INotifyCollectionChanged? RowsNotifier;
	VmWordLearnPage? SubscribedCtx;

	public partial ViewWordLearnPage();

	/// 依既定版面與綁定狀態建立此頁的控制項樹。
	partial void Render();

	/// 建立 BtnAdd 所需的 UI 組件，供頁面組裝時重用。
	private partial Button MkBtnAdd();

	/// 建立 Grid 所需的 UI 組件，供頁面組裝時重用。
	private partial Control MkGrid();

	/// 依目前 ViewModel 狀態重建呈現資料，確保列表與編輯狀態一致。
	partial void RebuildGrid();

	/// `Ctx` 後置注入後才有真正的行數據，這裏補建表格源。
	partial void OnCtxChanged();

	/// `LoadFromPoLearns` 會直接替換 `Rows`，需重新綁定集合事件。
	partial void OnCtxPropertyChanged(object? Sender, PropertyChangedEventArgs E);

	/// 掛接必要的資料或事件監聽，讓後續狀態變更能反映到畫面。
	partial void HookRowsChanged();

	/// 處理 RowsChanged 事件，維持頁面狀態與資料來源同步。
	partial void OnRowsChanged(object? Sender, NotifyCollectionChangedEventArgs E);

	/// 根據目前狀態取得對應的顯示或轉換結果。
	private partial str GetIdxText(VmWordLearnRow Row);

	/// 處理 GridTapped 事件，維持頁面狀態與資料來源同步。
	partial void OnGridTapped(object? Sender, TappedEventArgs E);
}
