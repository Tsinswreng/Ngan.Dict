using Tsinswreng.CsTools;

namespace Ngaq.Ui.Infra;

public partial interface I_Arg{
	public ITypedObj? Arg{get;set;}
}





// public static class ExtnIViewModel{
// 	public static nil Bind(
// 		this IViewModel z
// 		,Control Ctrl
// 		,AvaloniaProperty Property
// 		,Expression<Func<IViewModel, object?>> propertySelector

// 	){
// 		Ctrl.Bind(
// 			Property
// 			,new CBE(CBE.Pth<IViewModel>(propertySelector)){
// 				Mode = BindingMode.TwoWay
// 			}
// 		);
// 		return NIL;
// 	}
// }
