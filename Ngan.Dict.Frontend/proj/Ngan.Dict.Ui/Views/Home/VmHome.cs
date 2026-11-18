using System.Collections.ObjectModel;
using Ngan.Dict.Ui.Infra;

namespace Ngan.Dict.Ui.Views.Home;
using Ctx = VmHome;
public partial class VmHome: AppVmBase{

	public static ObservableCollection<Ctx> Samples = [];
	static VmHome(){
		{
			var o = new Ctx();
			Samples.Add(o);
		}
	}

}
