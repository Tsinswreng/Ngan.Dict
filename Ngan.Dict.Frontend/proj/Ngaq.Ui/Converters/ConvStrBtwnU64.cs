namespace Ngaq.Ui.Converters;
using System.Globalization;
using Avalonia.Data.Converters;
public partial class ConvU64ToStr : IValueConverter {
	protected static ConvU64ToStr? _Inst = null;
	public static ConvU64ToStr Inst => _Inst??= new ConvU64ToStr();

	//Vm->View
	public obj? Convert(obj? value, Type targetType, obj? parameter, CultureInfo culture) {
		return value?.ToString();
	}

	public obj? ConvertBack(obj? value, Type targetType, obj? parameter, CultureInfo culture) {
		if (value is string str) {
			if (u64.TryParse(str, out u64 result)) {
				return result;
			}
		}
		return null;
	}
}
