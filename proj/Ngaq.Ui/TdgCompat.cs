namespace Avalonia.Controls.Models.TreeDataGrid;

using System.Collections;
using System.Linq.Expressions;
using Avalonia.Controls;
using Avalonia.Interactivity;

/// 免費 DataGrid 兼容源的非泛型入口，供控件在不使用反射的情況下讀取行與列。
public interface IFlatTreeDataGridSource{
	/// DataGrid 顯示的平面行集合。
	IEnumerable Rows{get;}

	/// DataGrid 顯示的列集合。
	IEnumerable<DataGridColumn> Columns{get;}
}

/// 保留原 FlatTreeDataGridSource API 形狀的免費平面資料源。
public sealed class FlatTreeDataGridSource<TModel>: IFlatTreeDataGridSource{
	/// 建立直接引用既有集合的資料源，保留集合變更通知。
	public FlatTreeDataGridSource(IEnumerable<TModel> Rows){
		this.Rows = Rows;
	}

	/// 現有頁面提供的行集合。
	public IEnumerable<TModel> Rows{get;}

	/// 現有頁面以集合初始化器建立的文字列。
	public IList<DataGridColumn> Columns{get;} = [];

	/// 將泛型集合暴露給免費 DataGrid，不複製資料。
	IEnumerable IFlatTreeDataGridSource.Rows{
		get{return Rows;}
	}

	/// 將列集合暴露給免費 DataGrid。
	IEnumerable<DataGridColumn> IFlatTreeDataGridSource.Columns{
		get{return Columns;}
	}
}

/// 保留原 TextColumn 建構方式，以委託產生只讀文字儲存格。
public sealed partial class TextColumn<TModel, TValue>: DataGridColumn{
	/// 從行模型取得顯示值；使用解譯式 expression 避免 AOT 動態程式碼生成。
	readonly Func<TModel, TValue> Getter;

	/// 建立相容於原 TreeDataGrid 的文字列。
	public partial TextColumn(
		object? Header
		,Expression<Func<TModel, TValue>> Getter
		,GridLength? width = null
	);

	/// 建立顯示儲存格。
	protected override partial Control GenerateElement(DataGridCell Cell, object DataItem);

	/// DataGrid 抽象 API 要求的編輯儲存格；本兼容列始終只讀。
	protected override partial Control GenerateEditingElement(
		DataGridCell Cell
		,object DataItem
		,out Avalonia.Data.BindingExpressionBase Binding
	);

	/// DataGrid 編輯流程要求的入口；只讀列不保留待提交值。
	protected override partial object? PrepareCellForEdit(
		Control EditingElement
		,RoutedEventArgs EditingEventArgs
	);

	/// 格式化單元格值，類型不匹配時顯示空字串。
	private partial str FormatValue(object DataItem);

	/// 把原 GridLength 轉爲 DataGridLength。
	private static partial DataGridLength ToDataGridLength(GridLength Width);
}
