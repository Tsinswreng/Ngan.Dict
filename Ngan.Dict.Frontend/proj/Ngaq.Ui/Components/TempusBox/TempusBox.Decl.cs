namespace Ngaq.Ui.Components.TempusBox;

using System.Collections.Generic;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Ngaq.Core.Infra;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsCore;
using Tsinswreng.CsTempus;

public class InvalidCommandParam{
	public TempusBox Self{get;set;} = null!;
	public str Text{get;set;} = "";
}

public partial class TempusBox: ContentControl{
	/// `Tempus` 的 Avalonia 可绑定属性。默认双向绑定。
	/// `null` 表示无时间值，输入框应显示为空。
	public static readonly DirectProperty<TempusBox, UnixMs?> TempusProperty =
		AvaloniaProperty.RegisterDirect<TempusBox, UnixMs?>(
			nameof(Tempus),
			o => o.Tempus,
			(o, v) => o.Tempus = v,
			defaultBindingMode: Avalonia.Data.BindingMode.TwoWay
		);

	/// 当前 `Tempus` 主值。文本输入和日历选择都会回写到此值。
	public partial UnixMs? Tempus{get;set;}

	/// 当前选中的格式对象，可直接绑定。
	public static readonly DirectProperty<TempusBox, ITempusFormatItem> SelectedFormatProperty =
		AvaloniaProperty.RegisterDirect<TempusBox, ITempusFormatItem>(
			nameof(SelectedFormat),
			o => o.SelectedFormat,
			(o, v) => o.SelectedFormat = v,
			defaultBindingMode: Avalonia.Data.BindingMode.TwoWay
		);

	/// 输入无法按当前格式解析时触发。
		/// 输入无法按当前格式解析时触发。
		/// `Execute(parameter)` 的参数类型为 `InvalidCommandParam`。
		public static readonly DirectProperty<TempusBox, ICommand?> InvalidInputCommandProperty =
		AvaloniaProperty.RegisterDirect<TempusBox, ICommand?>(
			nameof(InvalidInputCommand),
			o => o.InvalidInputCommand,
			(o, v) => o.InvalidInputCommand = v
		);

	/// 是否只读。`true` 时禁用格式切换和日历，文本框进入只读。
	public partial bool IsReadOnly{get;set;}

	/// 全部格式来源。下拉框只使用此列表。
	public partial IList<ITempusFormatItem> FormatItems{get;}

	/// 当前选中的格式下标。
	public partial i32 SelectedFormatIndex{get;set;}

	/// 控件统一高度，让左按钮与输入框外框对齐。
	public partial f64 ControlHeight{get;set;}

	/// 最近一次文本解析是否成功，外层可读取作提示。
	public partial bool LastParseOk{get;protected set;}

	public readonly GridStack Root = new(IsRow: false);
	public TextBox _Input{get;set;} = null!;
	public Button _BtnMenu{get;set;} = null!;
	public ComboBox _ComboBoxFormat{get;set;} = null!;
	public Avalonia.Controls.Calendar _Calendar{get;set;} = null!;

	partial void Init();

	public TempusBox(){
		Init();
	}

	/// 外层修改 `FormatItems` 后调用，刷新下拉格式源。
	public partial void RefreshFormatOptions();
}
