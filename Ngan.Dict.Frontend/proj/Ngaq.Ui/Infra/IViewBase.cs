using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Ngaq.Ui.Infra;

public interface IViewBase
	:INotifyPropertyChanged
	,INotifyPropertyChanging
{

}


public interface IBtn{
	public obj? Content{get;}
	public Task<nil> Click(CT Ct);
	public event EventHandler? Done;
}


public class Btn : IBtn{
	public obj? InnerBtn{get;set;}
	public obj? Content{get;set;}
	public Func<Task<nil>>? FuncClick{get;set;}
	public async Task<nil> Click(CT Ct){
		if(FuncClick is not null){
			return FuncClick.Invoke();
		}
		return NIL;
	}
	public event EventHandler? Done;
}
