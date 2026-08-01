
using Tsinswreng.CsErr;
using Tsinswreng.CsI18n;

namespace Ngaq.Ui.Infra.I18n;
using I18n = Tsinswreng.CsI18n.I18n;
public class AppI18n{
	public static I18n Inst{get;set;} = new();
}

public static class AppExtnErrItem {
	extension(I18n z){
		public static I18n Inst{
			get => AppI18n.Inst;
			set => AppI18n.Inst = value;
		}
	}
	public static I18nKey ToI18nKey(this IErrNode z) {
		var Parent = ErrNode.MkB(null, ["Error"]);
		z.Parent?.Parent = Parent;
		var r = new I18nKey {
			RelaPathSegs = z.RelaPathSegs,
			DfltValueObj = z.DfltValueObj,
			Parent = z.Parent,
			Children = z.Children,
		};
		return r;
	}
}
