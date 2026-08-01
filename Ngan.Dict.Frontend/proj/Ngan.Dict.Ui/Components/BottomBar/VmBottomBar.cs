using System.Collections.ObjectModel;
using Ngan.Dict.Ui.Infra;

namespace Ngan.Dict.Ui.Views.BottomBar;
using Ctx = VmBottomBar;
public partial class VmBottomBar: ViewModelBase{

	public static ObservableCollection<Ctx> Samples = [];
	static VmBottomBar(){
		{
			var o = new Ctx();
			Samples.Add(o);

		}
	}

}
