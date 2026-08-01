namespace Ngaq.Ui.Views.Word.WordManage.StudyPlan.SetCurStudyPlan;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Ngaq.Ui;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.Ctrls;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.StudyPlanEdit;
using Ngaq.Ui.Views.Word.WordManage.StudyPlan.StudyPlanPage;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsI18n;
using Ctx = VmSetCurStudyPlan;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

/// 設置當前學習方案頁。
/// 頁面打開後即讀取後端當前學習方案，僅展示關鍵字段並支持重新選擇。
public partial class ViewSetCurStudyPlan
	:AppViewBase<Ctx>
	,I_MkTitleMenu
{

	public ViewSetCurStudyPlan(){
		Ctx = App.DiOrMk<Ctx>();
		Style();
		Render();
		Loaded += async(s,e)=>{
			if(HasAutoLoaded){
				return;
			}
			HasAutoLoaded = true;
			_ = Ctx?.LoadCurStudyPlan(default);
		};
	}


	public partial class Cls{
		public static str FullStretch = nameof(FullStretch);
	}

	protected nil Style(){
		Styles.A(
			Sty.Is<Control>(
				x=>x.Class(Cls.FullStretch)
			)
			.Set(x=>x.HorizontalAlignment, HAlign.Stretch)
			.Set(x=>x.VerticalAlignment, VAlign.Stretch)
		);
		return NIL;
	}

	GridStack Root = new(IsRow: true);
	bool HasAutoLoaded = false;

	protected nil Render(){
		this.Content = Root.Grid;
		Root.SetRowDefs([
			new(1, GUT.Star),
		]);
		Root.A(MkBody());
		return NIL;
	}

	Control MkBody(){
		var host = new GridStack(IsRow:true);
		host.Grid.SetRowDefs([new(1, GUT.Star)]);
		host.A(MkFieldsPanel());
		return host.Grid;
	}


	/// 當前方案字段展示區。

	Control MkFieldsPanel(){
		var sv = new ScrollViewer();
		var root = new StackPanel{
			Spacing = 10,
			Margin = new(10),
		};
		sv.Content = root;

		var bdr = new Border{
			BorderBrush = Brushes.DimGray,
			BorderThickness = new(1),
			Padding = new(10),
		};
		root.Children.Add(bdr);

		var sp = new StackPanel{
			Spacing = 8,
		};
		bdr.Child = sp;

		sp.A(MkIdRow(I[K.Id], CBE.Mk<Ctx>(x=>x.CurId, Mode: BindingMode.OneWay)))
		.A(MkInputRow(I[K.UniqName], CBE.Mk<Ctx>(x=>x.CurUniqName, Mode: BindingMode.OneWay), ReadOnly: true))
		.A(MkInputRow(I[K.Descr], CBE.Mk<Ctx>(x=>x.CurDescr, Mode: BindingMode.OneWay), ReadOnly: true, AcceptsReturn: true))
		.A(new OpBtn(), o=>{
			o._Button.HorizontalContentAlignment = HAlign.Center;
			o.BtnContent = Icons.ListSelect().WithText(I[K.ChooseAnotherStudyPlan]);
			o.SetExe((Ct)=>ChooseAndApplyStudyPlan(Ct));
		})
		.A(new Button(), o=>{
			o.HorizontalContentAlignment = HAlign.Center;
			o.Content = Icons.Edit().ToIcon().WithText(I[K.Edit]);
			o.Click += (s,e)=>{
				var po = Ctx?.CurPoStudyPlan;
				if(po is null){
					Ctx?.ShowDialog(I[K.NoEditableStudyPlan]);
					return;
				}
				var view = new ViewStudyPlanEdit();
				view.Ctx?.SetCreateMode(false);
				view.Ctx?.FromPoStudyPlan(po);
				ViewNavi?.GoTo(ToolView.WithTitle(po.UniqName ?? I[K.EditStudyPlan], view));
			};
		});

		return sv;
	}

	/// 展示頁正文使用只讀 TextBox 呈現，並禁用聚焦，避免移動端彈出輸入法。
	Control MkInputRow(str Label, IBinding Binding, bool ReadOnly = false, bool AcceptsReturn = false){
		var sp = new StackPanel{
			Spacing = 3,
		};
		sp.A(new TextBlock(), o=>{
			o.Text = Label;
		})
		.A(new TextBox(), o=>{
			o.IsReadOnly = ReadOnly;
			o.AcceptsReturn = AcceptsReturn;
			o.TextWrapping = AcceptsReturn ? TextWrapping.Wrap : TextWrapping.NoWrap;
			o.MaxHeight = AcceptsReturn ? 140 : double.PositiveInfinity;
			o.Focusable = false;
			o.IsTabStop = false;
			o.Bind(TextBox.TextProperty, Binding);
		});
		return sp;
	}

	/// Id 只用於辨識與複製，弱化視覺權重並保持可選中。
	Control MkIdRow(str Label, IBinding Binding){
		var row = new StackPanel{
			Spacing = 6,
			Orientation = Orientation.Horizontal,
		};
		row.A(new TextBlock(), o=>{
			o.Text = Label + ":";
			o.FontSize = UiCfg.Inst.BaseFontSize * 0.8;
		})
		.A(new SelectableTextBlock(), o=>{
			o.FontSize = UiCfg.Inst.BaseFontSize * 0.8;
			o.Bind(TextBlock.TextProperty, Binding);
		});
		return row;
	}

	async System.Threading.Tasks.Task<nil> ChooseAndApplyStudyPlan(CT Ct){
		if(Ctx is null){
			return NIL;
		}
		var tcs = new System.Threading.Tasks.TaskCompletionSource<
			Ngaq.Core.Shared.StudyPlan.Models.Po.StudyPlan.PoStudyPlan?
		>();
		var view = new ViewStudyPlanPage();
		view.DetachedFromVisualTree += (s,e)=>{
			tcs.TrySetResult(null);
		};
		view.Ctx?.SetSelectMode(po=>{
			Ctx.SelectCandidateStudyPlan(po);
			tcs.TrySetResult(po);
			view.ViewNavi?.Back();
		});
		ViewNavi?.GoTo(ToolView.WithTitle(I[K.SelectStudyPlan], view));
		Ngaq.Core.Shared.StudyPlan.Models.Po.StudyPlan.PoStudyPlan? selected = null;
		try{
			using var reg = Ct.Register(()=>tcs.TrySetCanceled(Ct));
			selected = await tcs.Task;
			if(selected is null){
				return NIL;
			}
			return await Ctx.CommitSelectedStudyPlan(Ct);
		}catch(System.OperationCanceledException){
			return NIL;
		}catch(System.Exception e){
			Ctx.HandleErr(e);
			return NIL;
		}
	}

	public Control MkTitleMenu(){
		var menu = new ContextMenu();
		menu.Items.A(new MenuItem(), o=>{
			o.Header = I[K.Reload];
			o.Click += (s, e)=>{
				_ = Ctx?.LoadCurStudyPlan(default);
			};
		});
		menu.Items.A(new MenuItem(), o=>{
			o.Header = I[K.RestoreBuiltin];
			o.Click += (s, e)=>{
				_ = Ctx?.RestoreBuiltin(default);
			};
		});
		return menu;
	}
}


