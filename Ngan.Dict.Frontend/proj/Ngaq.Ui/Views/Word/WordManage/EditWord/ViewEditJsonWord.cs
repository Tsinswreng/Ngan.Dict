namespace Ngaq.Ui.Views.Word.WordManage.EditWord;

using Avalonia.Controls;
using AvaloniaEdit;
using Ngaq.Ui.Infra;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Ctx = VmEditJsonWord;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

#if false //TODO 寫到Ui㕥示用戶

改詞?需改聚合根ʹ BizUpdatedAt ?所改ʹ條目ʹBizUpdatedAt?
務必使新BizUpdatedAt值足夠大、任設一值芝大於原者即可。後端自動取當前ʹ時㕥潙其?
于新增ʹ條??Props或Learns、設其Id?0"即可、後端自動替換成新生成ʹId

#endif

public partial class ViewEditJsonWord
	:AppViewBase<Ctx>
{

	public ViewEditJsonWord(){
		Ctx = App.DiOrMk<Ctx>();
		Style();
		Render();
	}

	public partial class Cls_{

	}
	public Cls_ Cls{get;set;} = new Cls_();

	protected nil Style(){
		return NIL;
	}

	GridStack Root = new(IsRow: true);
	protected nil Render(){
		this.SetContent(Root.Grid, o=>{
			o.SetRowDefs([
				new(1, GUT.Star),
				new(1, GUT.Auto),
			]);
		});
		Root.A(new TextBox(), o=>{
			o.AcceptsReturn = true;
			o.CBind<Ctx>(
				o.PropText
				,x=>x.Json);
		});
		var BottomBtnGrid = new GridStack(IsRow: false);
		Root.Add(BottomBtnGrid.Grid);
		{
			BottomBtnGrid.Grid.SetColDefs([
				new(1,GUT.Star),
				new(1,GUT.Star),
			]);
		}
		{{
			BottomBtnGrid
			.A(new Button(), o=>{
				o.HorizontalContentAlignment = HAlign.Center;
				o.Content = I[K.Save];
				o.Click += (s,e)=>{
					Ctx?.Save();
				};
			})
			.A(new Button(), o=>{
				o.HorizontalContentAlignment = HAlign.Center;
				o.Content = I[K.Delete];
				o.Click += (s,e)=>{
					Ctx?.Delete();
				};
			})
			;
		}}

		return NIL;
	}
}


