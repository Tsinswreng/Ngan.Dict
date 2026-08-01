namespace Ngaq.Ui.Views.Settings.LearnWord;

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.Ctrls;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsI18n;
using Ngaq.Ui.Infra.I18n;
using Ctx = VmCfgLearnWord;
using Tsinswreng.AvlnTools.Controls;
using Tsinswreng.Avln.Grid;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.SetCurStudyPlan;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.StudyPlanPage;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

public partial class ViewCfgLearnWord
	:AppViewBase<Ctx>
{


	public ViewCfgLearnWord(){
		Ctx = App.DiOrMk<Ctx>();
		Style();
		Render();
	}

	public partial class Cls{

	}


	protected nil Style(){
		this.Classes.A(App.Cls.ViewPadding);
		return NIL;
	}

	GridStack Root = new GridStack(IsRow:true);
	protected nil Render(){
		this.SetContent(Root.Grid, o=>{
			o.SetRowDefs([
				new(1, GUT.Star),
				new(1, GUT.Star),
			]);
		});
		Root.A(new ScrollViewer(), sc=>{
			sc.SetContent(new StackPanel(), sp=>{
				sp.Classes.A(App.Cls.SpacedStackPanel);
				sp
				.A(new SwipeLongPressBtn(), o=>{
					o.Content = I[K.StudyPlan];
					o.Click += (s,e)=>{
						ViewNavi?.GoTo(
							ToolView.WithTitle(I[K.StudyPlan], new ViewStudyPlan())
						);
					};
				}).A(new SwipeLongPressBtn(), o=>{
					o.Content = I[K.ChooseAnotherStudyPlan];
					o.Click += (s,e)=>{
						_ = ChooseAndApplyStudyPlan(default);
					};
				}).A(new CheckBox(), o=>{
					o.Content = I[K.EnableRandomBackground];
					Ctx.Bind(o, o=>o.IsChecked,x=>x.EnableRandomBackground);
				}).A(new CheckBox(), o=>{
					o.Content = I[K.EnableAutoPronounce];
					Ctx.Bind(o, o=>o.IsChecked,x=>x.EnableAutoPronounce);
				}).A(new TextBlock(), o=>{
					o.Text = I[K.MaxDisplayedWordCount];
				}).A(new TextBox(), o=>{
					Ctx.Bind(o, o=>o.Text, x=>x.MaxDisplayedWordCount);
				});
			});
		}).A(new OpBtn(), o=>{
			DockPanel.SetDock(o, Dock.Bottom);
			o._Button.StretchCenter();
			o.VAlign(x=>x.Bottom);
			o.BtnContent = I[K.Save];
			o._Button.Background = UiCfg.Inst.MainColor;
			o.SetExe((Ct)=>Ctx?.Save(Ct));
		});
		return NIL;
	}

	async System.Threading.Tasks.Task<nil> ChooseAndApplyStudyPlan(CT Ct){
		var tcs = new System.Threading.Tasks.TaskCompletionSource<
			Ngaq.Core.Shared.StudyPlan.Models.Po.StudyPlan.PoStudyPlan?
		>();
		var view = new ViewStudyPlanPage();
		view.Ctx?.SetSelectMode(po=>{
			tcs.TrySetResult(po);
			view.ViewNavi?.Back();
		});
		ViewNavi?.GoTo(ToolView.WithTitle(I[K.SelectStudyPlan], view));
		try{
			var selected = await tcs.Task;
			if(selected is null){
				return NIL;
			}
			var setter = App.DiOrMk<VmSetCurStudyPlan>();
			await setter.ApplySelectedStudyPlan(selected, Ct);
		}catch{
		}
		return NIL;
	}


}

