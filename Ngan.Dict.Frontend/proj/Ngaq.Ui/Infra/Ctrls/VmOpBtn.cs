namespace Ngaq.Ui.Infra.Ctrls;
using System.Collections.ObjectModel;
using Ngaq.Ui.Infra;

using Ctx = VmOpBtn;
public partial class VmOpBtn: ViewModelBase{
	//蔿從構造函數依賴注入、故以靜態工廠代無參構造器
	protected VmOpBtn(){}
	public static Ctx Mk(){
		return new Ctx();
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmOpBtn(){
		#if DEBUG
		{
			var o = new Ctx();
			Samples.Add(o);
		}
		#endif
	}

	public CancellationTokenSource Cts = new();

/*
	protected str _YYY = "";
	public str YYY{
		get{return _YYY;}
		set{SetProperty(ref _YYY, value);}
	}
 */

}
