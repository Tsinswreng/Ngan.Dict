namespace Ngaq.Ui.Views.Word.WordManage.StudyPlan.StudyPlanRow;
using System.Collections.ObjectModel;
using Ngaq.Ui.Infra;

using Ctx = VmStudyPlanRow;
public partial class VmStudyPlanRow: ViewModelBase, IMk<Ctx>{
	//蔿從構造函數依賴注入、故以靜態工廠代無參構造器
	protected VmStudyPlanRow(){}
	public static Ctx Mk(){
		return new Ctx();
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmStudyPlanRow(){
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
