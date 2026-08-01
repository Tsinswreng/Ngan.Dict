#define Impl
namespace Ngaq.Ui.Infra;

using System.Collections.Generic;
using Avalonia.Controls;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using Tsinswreng.CsTools;
using Tsinswreng.CsCore;
using Ngaq.Ui.Views;
using Ngaq.Core.Infra.Errors;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using Tsinswreng.CsErr;
using Tsinswreng.CsI18n;
using Ngaq.Ui.Infra.I18n;
using Tsinswreng.Avln.Navi;

public class EvtArgMsg:EventArgs{

}

public interface IMk<T>{
	public static abstract T Mk();
}

public partial class ViewModelBase
	:ObservableObject
	,IMk<ViewModelBase>
	,IViewModel
	,IViewNaviHolder
	,I_Arg
	,I_ForceSetProp
{

	static ViewModelBase IMk<ViewModelBase>.Mk(){
		return new ViewModelBase();
	}

	public ViewModelBase(){
		ViewNavi = MgrViewNavi.Inst.ViewNavi;
	}

	public void Init(){
		IsInited = true;
	}

	public bool IsInited{get;set;} = false;
	public void CheckInit(){
		if(!IsInited){
			throw new InvalidOperationException(Todo.I18n("ViewModel Not Inited"));
		}
	}

	[Impl(typeof(IViewNaviHolder))]
	[Obsolete(@$"用 {nameof(AppViewBase.ViewNavi)}。
	不應該在ViewModel層做視圖跳轉
	")]
	public IViewNavi? ViewNavi{get;set;}
	//跳轉傳參
	public ITypedObj? Arg{get;set;}

	public nil LogInfo(str? Msg = null){
		App.Logger?.LogInformation(Msg??"");
		return NIL;
	}

	public nil LogDebug(str? Msg = null){
		App.Logger?.LogDebug(Msg??"");
		return NIL;
	}

	public nil LogWarn(str? Msg=null){
		App.Logger?.LogWarning(Msg??"");
		return NIL;
	}

	public nil LogError(str? Msg = null){
		App.Logger?.LogError(Msg??"");
		return NIL;
	}

	/// 彈窗ʹ抽象。調用方宜無需知其內ʹ叶
	public nil ShowDialog(str Msg){
		LogInfo(nameof(ShowDialog)+": "+Msg);
		Dispatcher.UIThread.Post(()=>{
			MainView.Inst.ShowDialog(Msg);
		});
		return NIL;
	}

	/// 彈窗ʹ抽象(帶操作按鈕)。調用方傳入操作列表即可、彈窗內負責綁定按鈕並執行。
	public nil ShowDialog(str Msg, IList<Button> Operations){
		LogInfo(nameof(ShowDialog)+": "+Msg);
		Dispatcher.UIThread.Post(()=>{
			MainView.Inst.ShowDialog(Msg, Operations);
		});
		return NIL;
	}

	public nil ShowToast(str Msg, u64 DurationMs = 3000){
		return MainView.Inst.ShowToast(Msg, DurationMs);
	}

	public II18n I18n{get;set;} = AppI18n.Inst;
	public nil HandleErr(obj? Ex){
		return MainView.Inst.HandleErr(Ex);
	}

	public nil HandleErr(Task T){
		if(T.IsFaulted){
			if(T.Exception.InnerException is not null){
				HandleErr(T.Exception.InnerException);
			}else{
				HandleErr(T.Exception);
			}
		}
		return NIL;
	}

	/// 地址未變但內容ˋ變旹 適用此
	[Impl]
	public bool ForceSetProp<T>(
		[NotNullIfNotNull(nameof(newValue))] ref T Field
		,T newValue
		,[CallerMemberName] string? propertyName = null
	)
#if Impl
	{
		OnPropertyChanging(propertyName);
		Field = newValue;
		OnPropertyChanged(propertyName);
		return true;
	}
#else
	;
#endif

}
