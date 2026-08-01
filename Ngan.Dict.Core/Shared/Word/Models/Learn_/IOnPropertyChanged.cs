namespace Ngaq.Core.Shared.Word.Models.Learn_;
using System.ComponentModel;
using System.Runtime.CompilerServices;



public partial interface I_OnPropertyChanged: INotifyPropertyChanged{
	void OnPropertyChanged(str? PropName);
	// public virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null){
	// 	PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	// }
}

