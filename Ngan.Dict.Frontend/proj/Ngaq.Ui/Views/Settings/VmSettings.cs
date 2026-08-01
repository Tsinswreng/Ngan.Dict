namespace Ngaq.Ui.Views.Settings;

using System.Collections.ObjectModel;
using Ngaq.Ui.Infra;

using Ctx = VmSettings;

public partial class VmSettings: ViewModelBase, IMk<Ctx>{
	protected VmSettings(){}

	public static Ctx Mk(){
		return new Ctx();
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmSettings(){
		#if DEBUG
		{
			var o = new Ctx();
			Samples.Add(o);
		}
		#endif
	}
}

