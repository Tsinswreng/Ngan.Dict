using System.ComponentModel;
using System.Linq.Expressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using Ngaq.Ui.Infra.I18n;
using Ngaq.Ui.Views;
using Tsinswreng.Avln.Navi;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.CsI18n;
namespace Ngaq.Ui.Infra;

public partial class AppViewBase
	:UserControl
	,IViewBase
	,IViewNaviHolder
{
	public IViewNavi? ViewNavi{get;set;} = MgrViewNavi.Inst.ViewNavi;
	public II18n I = AppI18n.Inst;

	public event PropertyChangedEventHandler? PropertyChanged;
	public event PropertyChangingEventHandler? PropertyChanging;

	protected nil RaisePropertyChanged(str PropertyName){
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
		return NIL;
	}

	protected nil RaisePropertyChanging(str PropertyName){
		PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(PropertyName));
		return NIL;
	}
}


public partial class AppViewBase<TCtx>
	:UserControl
	,IViewBase
	,IView<TCtx>
	,IViewNaviHolder
	where TCtx:class
{
	public IViewNavi? ViewNavi{get;set;} = MgrViewNavi.Inst.ViewNavi;
	public II18n I = AppI18n.Inst;
	public event PropertyChangedEventHandler? PropertyChanged;
	public event PropertyChangingEventHandler? PropertyChanging;
	public TCtx? Ctx{
		get{return DataContext as TCtx;}
		set{DataContext = value;}
	}

	protected nil RaisePropertyChanged(str PropertyName){
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
		return NIL;
	}

	protected nil RaisePropertyChanging(str PropertyName){
		PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(PropertyName));
		return NIL;
	}

}


