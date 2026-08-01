namespace Ngaq.Ui.Infra.Ctrls;
using Avalonia.Threading;
using Ngaq.Ui.Views;

public static class ExtnAppOpBtn{
	public static OpBtn SetExe(
		this OpBtn z
		,Func<CT, Task<nil>?>? FnExeAsy
	){
		var op = z;
		// FnExeAsy中恐有ˌ改UI、故不包于Task
		// if(FnExeAsy is not null){
		// 	op.FnExeAsy = async(Ct)=>{
		// 		await Task.Run(async ()=>{
		// 			var R = FnExeAsy(Ct);
		// 			if(R is not null){
		// 				await R;
		// 			}
		// 		});
		// 		return NIL;
		// 	};
		// }
		if(FnExeAsy is not null){
			op.FnExeAsy = FnExeAsy;
		}

		op.FnOk = ()=>{
			// Dispatcher.UIThread.Post(()=>{
			// 	MainView.Inst.ShowMsg("Ok");
			// });
			return NIL;
		};
		op.FnFail = (err)=>{
			Dispatcher.UIThread.Post(()=>{
				MainView.Inst.HandleErr(err);
			});
			return NIL;
		};
		return z;
	}
}
