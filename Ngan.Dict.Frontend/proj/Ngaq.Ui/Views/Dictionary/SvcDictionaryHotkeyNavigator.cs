namespace Ngaq.Ui.Views.Dictionary;

using System.Linq;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Views.Home;
using Tsinswreng.Avln.Navi;

/// 熱鍵用字典導航器：
/// 1) 回到 Home 頁；
/// 2) 切到 Home 底欄的字典標簽。
public class SvcDictionaryHotkeyNavigator : ISvcDictionaryHotkeyNavigator{
	/// 回到 Home 並切到字典底欄，並可選擇直接查詢詞條。
	/// <param name="Term">待查詞條。</param>
	/// <returns>空值。</returns>
	public nil OpenDictionary(str? Term = null){
		var navi = MgrViewNavi.Inst.GetViewNavi();
		if(navi is not ViewNavi concrete){
			return NIL;
		}

		var home = concrete.Stack.OfType<ViewHome>().LastOrDefault();
		if(home is null){
			home = new ViewHome();
			navi.GoTo(home);
			home.OpenDictionaryTab(Term);
			return NIL;
		}

		if(!object.ReferenceEquals(concrete.Peek, home)){
			while(!object.ReferenceEquals(concrete.Peek, home) && concrete.Back()){
			}
			if(!object.ReferenceEquals(concrete.Peek, home)){
				navi.GoTo(home);
			}
		}

		home.OpenDictionaryTab(Term);
		return NIL;
	}
}
