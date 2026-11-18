using System.ComponentModel;
using Avalonia.Controls;
using Ngan.Dict.Ui.Infra.I18n;
using Tsinswreng.Avln.Navi;
using Tsinswreng.CsI18n;

namespace Ngan.Dict.Ui.Infra;

/// 舊寫法的視圖基類：`Ctx` 直接映射到 `DataContext`，控件樹由子類在構造器裏 `Render()` 自建。
///
/// *為何還在*：全倉庫仍有 60 餘個視圖是這個形態。新的視圖一律用 `LegacyAppViewBase<TVm>`
/// （Declarative 的 `ViewBase<TVm>`，控件樹由 `Build(TVm)` 描述）；
/// 這批視圖會逐步遷移，全部遷完後本檔連同 `IView<>` 一起刪。
public partial class LegacyAppViewBase<TCtx>
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

/// 不帶 ViewModel 的舊寫法視圖基類。
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
