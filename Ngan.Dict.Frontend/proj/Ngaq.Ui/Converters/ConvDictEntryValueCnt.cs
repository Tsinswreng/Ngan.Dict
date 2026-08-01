namespace Ngaq.Ui.Converters;
using System.Globalization;
using Avalonia.Data.Converters;
public  partial class ConvMultiDictValueCnt<K,V> : IValueConverter {

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		if(value is not IDictionary<K,IList<V>> dict){
			throw new ArgumentException("Value must be an IDictionary<K,V>");
		}
		if(parameter is not K key){
			throw new ArgumentException("Parameter must be a string key");
		}
		if(!dict.TryGetValue(key, out IList<V>? List)){
			return 0;
		}
		return List.Count;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
		throw new NotSupportedException();
	}
}
