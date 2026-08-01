namespace Ngaq.Ui.Views.Word.WordManage.StudyPlan;

using Avalonia.Controls;
using Avalonia.Input;
using Ngaq.Ui;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views.About;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.PreFilterPage;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.SetCurStudyPlan;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.StudyPlanPage;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.WeightArgPage;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.WeightCalculatorPage;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsI18n;
using Ctx = VmStudyPlan;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;
public partial class ViewStudyPlan
	:AppViewBase<Ctx>, I_MkTitleMenu
{

	public ViewStudyPlan(){
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
		Root.A(_S());
		return NIL;
	}

	StackPanel _S(){
		var o = new StackPanel();
		o
		.A(
			MainView.Inst.MkBtnToView(()=>new ViewSetCurStudyPlan(),I[K.SetCurrentStudyPlan])
		).A(
			MainView.Inst.MkBtnToView(()=>new ViewStudyPlanPage(),I[K.StudyPlan])
		).A(
			MainView.Inst.MkBtnToView(()=>new ViewPreFilterPage(),I[K.PreFilter])
		).A(
			MainView.Inst.MkBtnToView(()=>new ViewWeightCalculatorPage(),I[K.WeightCalculator])
		).A(
			MainView.Inst.MkBtnToView(()=>new ViewWeightArgPage(),I[K.WeightArgWithSpace])
		)
		;
		return o;
	}

	public Control MkTitleMenu() {
		var r = new ContextMenu();
		var items = r.Items;
		items.A(new MenuItem(), o=>{
			o.Header = I[K.Help];
			o.Click += (s,e)=>{
				ViewNavi?.GoTo(ToolView.WithTitle(I[K.StudyPlanHelpTitle_], MkHelpView()));
			};
		});
		return r;
	}

	Control MkHelpView(){
		var o = new TextBox();
		o.Classes.Add(App.Cls.RoTextBox);
		//InputMethod.SetIsInputMethodEnabled(o, false);
		o.Text = I[K.StudyPlanHelpText_] + "\n" + ViewAbout.WeightAlgorithmPluginDocUrl;
		return o;
	}
}



