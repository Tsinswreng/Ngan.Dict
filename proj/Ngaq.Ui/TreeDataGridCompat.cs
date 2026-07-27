namespace Avalonia.Controls;

using Avalonia.Controls.Models.TreeDataGrid;

/// 以免費 DataGrid 保留現有 TreeDataGrid 控件名稱與 Source 賦值方式。
public sealed class TreeDataGrid: DataGrid{
	/// 複用 DataGrid 的 ControlTheme，否則自定義子類在 Avalonia 12 中沒有可視模板。
	protected override Type StyleKeyOverride{
		get{return typeof(DataGrid);}
	}

	/// 當前兼容資料源。
	IFlatTreeDataGridSource? _Source;

	/// 設置資料源時同步更新 DataGrid 的行與列。
	public IFlatTreeDataGridSource? Source{
		get{return _Source;}
		set{
			_Source = value;
			ApplySource();
		}
	}

	/// 建立符合現有頁面只讀、單選行爲的免費表格。
	public TreeDataGrid(){
		AutoGenerateColumns = false;
		IsReadOnly = true;
		CanUserResizeColumns = false;
		CanUserSortColumns = false;
		CanUserReorderColumns = false;
		GridLinesVisibility = DataGridGridLinesVisibility.Horizontal;
		HeadersVisibility = DataGridHeadersVisibility.Column;
		SelectionMode = DataGridSelectionMode.Single;
	}

	/// 不使用反射地把兼容源投影到 DataGrid 原生 API。
	void ApplySource(){
		Columns.Clear();
		ItemsSource = _Source?.Rows;
		if(_Source is null){
			return;
		}
		foreach(var column in _Source.Columns){
			Columns.Add(column);
		}
	}
}
