using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Ngaq.Ui.Infra;

public interface I_ForceSetProp{

	/// 地址未變但內容ˋ變旹 適用此

	/// <typeparam name="T"></typeparam>
	/// <param name="field"></param>
	/// <param name="newValue"></param>
	/// <param name="propertyName"></param>
	/// <returns></returns>
	public bool ForceSetProp<T>(
		[NotNullIfNotNull(nameof(newValue))] ref T field
		,T newValue
		,[CallerMemberName] string? propertyName = null
	)
#if Impl
	{
		OnPropertyChanging(propertyName);
		field = newValue;
		OnPropertyChanged(propertyName);
		return true;
	}
#else
	;
#endif
}
