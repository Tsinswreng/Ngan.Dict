using Avalonia.Markup.Declarative;
using Microsoft.Extensions.Logging;
using Ngan.Dict.Core.Infra.Log;
using Ngan.Dict.Ui.Infra.I18n;
using Tsinswreng.Avln.Navi;
using Tsinswreng.CsI18n;

namespace Ngan.Dict.Ui.Infra;

/// 所有新寫法視圖的基類。
///
/// *為何繼承 Declarative 的 `ViewBase<TVm>`*：控件樹改由 `Build(TVm)` 的回傳值描述，
/// 初始化時機、`BuildStyles()` 與 `OnAfterInitialized()` 的呼叫順序都由庫負責；
/// 子類不必在構造器裏自己 new 控件樹。
///
/// *為何 `ViewNavi`／`I` 是欄位而不是屬性*：本專案的 Declarative 生成器
/// （`MarkupDeclarative.Roslyn4`）在泛型視圖基類上遇到可寫屬性時，
/// 會把型別參數 `TVm` 原樣寫進非泛型的擴展類而編不過。
/// 欄位不進生成器，故這兩樣用欄位；`vm` 用 protected 屬性也不觸發。
///
/// *為何不實作 `IView<TVm>`*：它要求公開的 `Ctx` 屬性，而公開的泛型屬性同樣會觸發上述問題。
/// 舊寫法的視圖仍用 `LegacyAppViewBase<TCtx>`。
public abstract partial class AppViewBase<TVm>
	: ViewBase<TVm>
	where TVm : class
{
	/// 視圖導航器；導航職責留在 View 層，不進 ViewModel。
	public IViewNavi? ViewNavi = MgrViewNavi.Inst.ViewNavi;

	/// 本視圖使用的 i18n 門面。
	public II18n I = AppI18n.Inst;

	/// 本視圖的 ViewModel。由子類的構造器設好之後就不再變，故不判空。
	protected TVm vm{
		get{
			return ViewModel!;
		}
		set{
			ViewModel = value;
		}
	}

public void SetVm(TVm Vm){
		vm = Vm;
	}

	/// 啟動非同步工作：UI 事件與生命週期回調都是 void，不能 await。
	protected async void Fire(Task<nil> Op){
		try{
			await Op;
		}catch(Exception Ex){
			AppLog.Inst.Log(Microsoft.Extensions.Logging.LogLevel.Error, Ex, "Fire failed in " + GetType().Name);
		}
	}

/// 本視圖的 ViewModel（對外唯讀）。
	public TVm Vm{
		get{ return vm; }
	}

	/// 默認控件樹：不覆寫時什麼都不建，方便逐步遷移。
	/// 新寫法的視圖一律覆寫它、由回傳值描述控件樹。
	protected override object Build(TVm vm){
		return Child!;
	}
}
