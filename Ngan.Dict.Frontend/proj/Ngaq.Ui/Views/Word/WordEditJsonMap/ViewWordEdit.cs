namespace Ngaq.Ui.Views.Word.WordEditJsonMap;

using System.Linq.Expressions;
using Avalonia.Controls;
using DynamicData.Binding;
using Ngaq.Core.Infra;
using Ngaq.Core.Tools.JsonMap;
using Ngaq.Ui;
using Ngaq.Ui.Components.KvMap.JsonMap;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.Ctrls;
using Ngaq.Ui.Tools;
using Ngaq.Ui.Views.Word.WordEditV2;
using Ngaq.Ui.Views.Word.WordManage.EditWord;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsI18n;
using Tsinswreng.Srefl;
using Ctx = VmWordEdit;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;
public partial class ViewWordEdit
	:AppViewBase<Ctx>
	,I_MkTitleMenu
{

	public ViewWordEdit(){
		Ctx = App.DiOrMk<Ctx>();
		Style();
		Render();
	}

	public partial class Cls{

	}

	public Control MkTitleMenu(){
		var R = new ContextMenu();
		R.Styles.Add(new Style().NoMargin().NoPadding());
		R.Items.A(new MenuItem(), o=>{
			o.Header = "To Json View";
			o.Click += (s,e)=>{
				var vj = new ViewEditJsonWord();
				if(AnyNull(vj.Ctx, Ctx?.JnWord)){
					Ctx?.ShowDialog(I[K.NoWordOrCtx]);
					return;
				}
				vj.Ctx.FromJnWord(Ctx.JnWord);

				ViewNavi?.GoTo(ToolView.WithTitle(Ctx?.JnWord.Word.Head??"", vj));
			};
		});
		R.Items.A(new MenuItem(), o=>{
			o.Header = "To New Form View";
			o.Click += (s,e)=>{
				var v2 = new ViewWordEditV2();
				if(AnyNull(v2.Ctx, Ctx?.JnWord)){
					Ctx?.ShowDialog("No Word or Ctx");
					return;
				}
				v2.Ctx.FromJnWord(Ctx.JnWord);
				ViewNavi?.GoTo(ToolView.WithTitle(Ctx?.JnWord.Word.Head??"", v2));
			};
		});
		return R;
	}

	protected nil Style(){
		return NIL;
	}

	Expander _Expander(){
		var R = new Expander();
		R.Styles.Add(new Style().NoMargin().NoPadding());
		R.HorizontalAlignment = HAlign.Stretch;
		return R;
	}

	GridStack Root = new(IsRow: true);
	protected nil Render(){
		this.Content = Root.Grid;
		Root.SetRowDefs([
			new(9999, GUT.Star),
			new(1, GUT.Auto),
		]);

		Root.A(new ScrollViewer(), Sv=>{
			Sv.SetContent(new StackPanel(), root2=>{
				root2.A(_Expander(), Ex=>{
					Ex.Header = I[K.WordCore];
					Ex.IsExpanded = true;
					Ex.SetContent(new ScrollViewer(), Sv=>{
						Sv.SetContent(new ViewUiJsonMap(), jm=>{
							Ctx?.WhenPropertyChanged(x=>x.JnWord).Subscribe(propValue=>{
								var currentJnWord = propValue.Value;
								if (currentJnWord == null) return;
								var poWord = JnWordToUiJsonMap.MkPoWord(
									CoreDictMapper.Inst.PropAccessorReg.ToPropDict(currentJnWord.Word)
								);
								jm.Ctx!.FromBo(poWord);
							});
						});
					});
				});
			});
		});

		Root.A(new OpBtn(), o=>{
			o.BtnContent = I[K.Save];
			o._Button.StretchCenter();
		});

		return NIL;
	}


}

