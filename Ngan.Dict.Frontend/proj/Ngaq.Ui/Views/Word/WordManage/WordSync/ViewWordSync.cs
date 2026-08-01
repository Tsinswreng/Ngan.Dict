namespace Ngaq.Ui.Views.Word.WordManage.WordSync;

using Avalonia.Controls;
using Avalonia.Threading;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.Ctrls;
using Ngaq.Ui.Tools;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsI18n;
using Ctx = VmWordSync;
using K = Infra.I18n.KeysUiI18nCommon;

public partial class ViewWordSync
	:AppViewBase<Ctx>
{

	public ViewWordSync(){
		Ctx = App.GetRSvc<Ctx>();
		Style();
		Render();
	}

	public partial class Cls_{

	}
	public Cls_ Cls{get;set;} = new Cls_();

	protected nil Style(){
		return NIL;
	}

	public GridStack Root = new(IsRow:true);
	protected nil Render(){
		this.SetContent(Root.Grid, o=>{

		});
		Root.A(new StackPanel(), Sp=>{

			Sp.A(new OpBtn(), op=>{
				var o = op._Button;
				op.SetExe((Ct)=>Ctx?.PushAsy(Ct)!);
				//o.Content = I[K.Push];
				o.Content = ToolIcon.IconWithTitle(
					Icons.CloudUpload().ToIcon()
					,I[K.Push]
				);
			})
			.A(new OpBtn(), o=>{
				o.SetExe((Ct)=>Ctx?.PullAsy(Ct));
				o.BtnContent = ToolIcon.IconWithTitle(
					Icons.CloudDownload().ToIcon()
					,I[K.Pull]
				);
			})
			.A(new TextBlock(), o=>{
				o.Text = I[K.ExportPath];
			})
			.A(new TextBox(), o=>{
				o.CBind<Ctx>(o.PropText, x=>x.PathExport);
			})
			.A(new OpBtn(), o=>{
				o.BtnContent = ToolIcon.IconWithTitle(
					Icons.DatabaseExport().ToIcon()
					,I[K.Export]
				);
				o.SetExe((Ct)=>Ctx?.ExportAsy(Ct));
			})

			.A(new Border(), o=>{
				o.Height = 10;
			})

			.A(new TextBlock(), o=>{
				o.Text = I[K.ImportPath];
			})
			.A(new TextBox(), o=>{
				o.CBind<Ctx>(o.PropText, x=>x.PathImport);
			})
			.A(new OpBtn(), o=>{
				o.BtnContent = ToolIcon.IconWithTitle(
					Icons.DatabaseImport().ToIcon()
					,I[K.Import]
				);
				o.SetExe((Ct)=>Ctx?.ImportAsy(Ct));
			})
			;
			// .AddInit(_Button(), o=>{
			// 	o.Content = "Test";
			// 	o.Click+=(s,e)=>{
			// 		Ctx?.ShowMsg("Test");
			// 	};
			// })
			// .AddInit(_Button(), o=>{
			// 	o.Content = "TestLong";
			// 	o.Click+=(s,e)=>{
			// 		Ctx?.ShowMsg(LongText);
			// 	};
			// })

			;
		});
		return NIL;
	}

}

