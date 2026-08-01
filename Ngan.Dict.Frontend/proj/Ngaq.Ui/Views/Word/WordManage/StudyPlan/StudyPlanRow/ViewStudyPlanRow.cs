namespace Ngaq.Ui.Views.Word.WordManage.StudyPlan.StudyPlanRow;

using Ngaq.Ui;
using Ngaq.Ui.Infra;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsI18n;
using Ctx = VmStudyPlanRow;
public partial class ViewStudyPlanRow
	:AppViewBase<Ctx>
{

	public ViewStudyPlanRow(){
		Ctx = App.DiOrMk<Ctx>();
		Style();
		Render();
	}

	public partial class Cls{

	}


	protected nil Style(){
		return NIL;
	}


	GridStack Root = new(IsRow: true);
	protected nil Render(){
		this.Content = Root.Grid;
		Root.SetRowDefs([
			new(1, GUT.Star),
		]);
		return NIL;
	}
}


