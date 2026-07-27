namespace Avalonia.Controls.Models.TreeDataGrid;

using System.Linq.Expressions;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;

/// 免費 DataGrid 文字列的實現。
public sealed partial class TextColumn<TModel, TValue>{
	/// 建立只讀文字列並保留原 API 的表頭與寬度語義。
	public partial TextColumn(
		object? Header
		,Expression<Func<TModel, TValue>> Getter
		,GridLength? width
	){
		ArgumentNullException.ThrowIfNull(Getter);
		this.Getter = Getter.Compile(preferInterpretation: true);
		this.Header = Header;
		Width = ToDataGridLength(width ?? new GridLength(1, GridUnitType.Auto));
		IsReadOnly = true;
	}

	/// 用輕量 TextBlock 顯示值，保持表格爲只讀用途。
	protected override partial Control GenerateElement(DataGridCell Cell, object DataItem){
		return new TextBlock{
			Text = FormatValue(DataItem),
			TextTrimming = TextTrimming.CharacterEllipsis,
			VerticalAlignment = VAlign.Center,
			Margin = new Thickness(8, 0),
		};
	}

	/// 編輯入口返回同一只讀視圖；兼容層不提供內聯編輯。
	protected override partial Control GenerateEditingElement(
		DataGridCell Cell
		,object DataItem
		,out BindingExpressionBase Binding
	){
		Binding = null!;
		return GenerateElement(Cell, DataItem);
	}

	/// 本列永不進入可編輯狀態，因此沒有需要保存的原值。
	protected override partial object? PrepareCellForEdit(
		Control EditingElement
		,RoutedEventArgs EditingEventArgs
	){
		return null;
	}

	/// 僅接受該列宣告的行模型，避免無效轉型影響 UI。
	private partial str FormatValue(object DataItem){
		if(DataItem is not TModel model){
			return "";
		}
		var value = Getter(model);
		return value?.ToString() ?? "";
	}

	/// 保留 Auto、Star、Pixel 三種現有列寬配置。
	private static partial DataGridLength ToDataGridLength(GridLength Width){
		if(Width.IsAuto){
			return DataGridLength.Auto;
		}
		if(Width.IsStar){
			return new DataGridLength(Width.Value, DataGridLengthUnitType.Star);
		}
		return new DataGridLength(Width.Value, DataGridLengthUnitType.Pixel);
	}
}
