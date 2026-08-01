namespace Ngaq.Ui.Views.Word.WordManage.StudyPlan;
using System.Collections.ObjectModel;
using Ngaq.Ui.Infra;

using Ctx = VmStudyPlan;
public partial class VmStudyPlan: ViewModelBase, IMk<Ctx>{
	//蔿從構造函數依賴注入、故以靜態工廠代無參構造器
	protected VmStudyPlan(){}
	public static Ctx Mk(){
		return new Ctx();
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmStudyPlan(){
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
