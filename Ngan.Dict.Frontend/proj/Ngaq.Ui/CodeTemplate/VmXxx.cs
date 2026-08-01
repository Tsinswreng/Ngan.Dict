namespace Xxx;
using System.Collections.ObjectModel;
using Ngaq.Ui.Infra;

using Ctx = VmXxx;
public partial class VmXxx: ViewModelBase, IMk<Ctx>{
	//蔿從構造函數依賴注入、故以靜態工廠代無參構造器
	protected VmXxx(){}
	public static Ctx Mk(){
		return new Ctx();
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmXxx(){
		#if DEBUG
		{
			var o = new Ctx();
			Samples.Add(o);
		}
		#endif
	}


	public str YYY{
		get;
		set{SetProperty(ref field, value);}
	}="";


}
