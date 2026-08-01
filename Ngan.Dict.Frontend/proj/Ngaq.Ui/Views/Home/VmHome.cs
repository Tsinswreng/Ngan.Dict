using System.Collections.ObjectModel;
using Ngaq.Ui.Infra;

namespace Ngaq.Ui.Views.Home;
using Ctx = VmHome;
public partial class VmHome: ViewModelBase{

	public static ObservableCollection<Ctx> Samples = [];
	static VmHome(){
		{
			var o = new Ctx();
			Samples.Add(o);
		}
	}

}
