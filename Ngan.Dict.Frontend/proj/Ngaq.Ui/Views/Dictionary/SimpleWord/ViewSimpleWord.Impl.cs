namespace Ngaq.Ui.Views.Dictionary.SimpleWord;

using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Ui;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.Ctrls;
using Tsinswreng.Avln.Grid;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsI18n;
using Ctx = VmSimpleWord;

/// 此文件只保存 ViewSimpleWord 的函數實現；公開聲明位於同名 .cs 文件。
public partial class ViewSimpleWord{
	public partial ViewSimpleWord(){
			Ctx = App.DiOrMk<Ctx>();
			Style();
			Render();
		}

	protected partial nil Style(){
			return NIL;
		}

	private partial SelectableTextBlock Txt(){
			var R = new SelectableTextBlock();
			R.TextWrapping = TextWrapping.Wrap;
			return R;
		}

	protected partial nil Render(){
			Content = Root.Grid;
			Root.SetRowDefs([
				new(1, GUT.Auto),
				new(1, GUT.Auto),
				new(1, GUT.Auto),
			]);
			Root
			.A(Txt(), o=>{
				o.FontSize = UiCfg.Inst.BaseFontSize*1.5;
				Ctx.Bind(o, o=>o.Text,x=>x.Head);
			})
			.A(PronunciationList())
			.A(Txt(), o=>{
				Ctx.Bind(o,o=>o.Text,x=>x.Description);
			})
			;
			return NIL;
		}

	private partial Control PronunciationList(){
			var R = new ItemsControl();
			R.Bind(
				R.PropItemsSource, CBE.Mk<Ctx>(x=>x.Pronunciations)
			);
	
			R.SetItemTemplate<Pronunciation>((p,b)=>{
				var Row = new GridStack(IsRow: false);
				Row.SetColDefs([
					new(1, GUT.Auto),
					new(1, GUT.Auto),
				]);
				Row.A(new OpBtn(), o=>{
					var Icon = Icons.VolHigh().ToIcon();
					Icon.Height = UiCfg.Inst.BaseFontSize*0.8;
					Icon.Width = UiCfg.Inst.BaseFontSize*0.8;
					o.BtnContent = Icon;
					// 按鈕保持方形尺寸，且靠左不拉伸。
					o._Button.Width = UiCfg.Inst.BaseFontSize*1.6;
					o._Button.Height = UiCfg.Inst.BaseFontSize*1.6;
					o._Button.HorizontalAlignment = HAlign.Left;
					o.HorizontalAlignment = HAlign.Left;
					o.SetExe(Ct=>Ctx?.PlayHead(Ct));
				})
				.A(Txt(), o=>{
					o.Text = p.Text;
					o.VerticalAlignment = VAlign.Center;
					o.Margin = new Avalonia.Thickness(UiCfg.Inst.BaseFontSize*0.2, 0, 0, 0);
				});
				return Row.Grid;
			});
			R.SetItemsPanel(()=>{
				return new StackPanel{
					Orientation = Orientation.Vertical,
					HorizontalAlignment = HAlign.Left,
				};
			});
	
			return R;
		}
}

