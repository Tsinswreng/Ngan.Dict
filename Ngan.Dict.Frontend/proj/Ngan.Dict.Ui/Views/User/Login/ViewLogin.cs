namespace Ngan.Dict.Ui.Views.User.Login;

using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using Avalonia.Media;
using Avalonia.Styling;
using Ngan.Dict.Ui;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using K = Ngan.Dict.Ui.Infra.I18n.KeysUiI18nCommon;


using Ctx = VmLoginRegister;
using Ngan.Dict.Ui.Infra.I18n;
using Ngan.Dict.Ui.Infra.Ctrls;
using Ngan.Dict.Ui.Infra;
using Tsinswreng.Avln.Grid;

public partial class ViewLogin
	:AppViewBase<Ctx>
	//,IHasViewNavigator
{

	//public IViewNavigator? ViewNavigator{get;set;}

	public ViewLogin(){
		// Ctx由父容器ViewLoginRegister注入，避免登錄與註冊頁分別持有不同Vm實例。
		Ctx = Ctx.Mk();//前置兜底
		_Style();
		_Render();
	}

public partial class Cls_{
		public str InputBox = nameof(InputBox);
		public str Logo = nameof(Logo);
	}
	public Cls_ Cls{get;set;} = new Cls_();


	protected nil _Style(){
		var cls_inputBox = Sty.Is<ContentControl>(x=>
			x.Class(Cls.InputBox)
		);
		Styles.Add(cls_inputBox);
		{
			var o = cls_inputBox;
			o.Set(
				x=>x.Background
				,new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20))
			);
		}

		return NIL;
	}

	protected nil _Render(){
		this.SetContent(Root.Grid, o=>{
			o.SetRowDefs([
				new(1, GUT.Star),
				new(1, GUT.Auto),
				new(1, GUT.Star),
			]);
		});

		// 上下空白等比，內容集中在中間區域。
		Root.A(new Border(), _=>{});
		Root.A(new StackPanel(), Stk=>{
			Stk.Spacing = 8.0;

			var formItem = FnAddLabelBox(Stk);
			formItem(I[K.Email], CBE.Mk<Ctx>(x=>x.Email, Mode:BindingMode.TwoWay), false);
			formItem(I[K.Password], CBE.Mk<Ctx>(x=>x.Password, Mode:BindingMode.TwoWay), true);
			var errMsgSclv = new ScrollViewer{};
			Stk.A(errMsgSclv);
			{
			}
			errMsgSclv.Content = _ErrorList();
			Stk.A(new OpBtn(), (o)=>{
				var b = o._Button;
				b.SetContent(new TextBlock(), t=>{
					t.Text = I[K.Login];
					t.FontSize = UiCfg.Inst.BaseFontSize*1.2;
					t.TextAlignment = TextAlignment.Center;
					t.HorizontalAlignment = HAlign.Center;
				});
				b.HorizontalAlignment = HAlign.Stretch;
				b.Background = UiCfg.Inst.MainColor;
				o.SetExe((Ct)=>Ctx?.LoginAsy(Ct));
			});
		});
		Root.A(new Border(), _=>{});
		return NIL;
	}

	GridStack Root = new(IsRow: true);



	protected Control _ErrorList(){
		var ans = new ItemsControl{};
		ans.Bind(
			ItemsControl.ItemsSourceProperty
			,CBE.Mk<Ctx>(x=>x.Msgs)
		);
		ans.ItemTemplate = new FuncDataTemplate<str>((err,b)=>{
			var ua = new SelectableTextBlock{};
			ua.Text = err;
			ua.Foreground = Brushes.Red;
			ua.TextWrapping = TextWrapping.Wrap;
			return ua;
		});
		return ans;
	}


	protected Func<str, IBinding, bool, nil?> FnAddLabelBox(Panel Container){
		return (LabelText, TextBinding, IsPassword)=>{
			var label = new Label{};
			Container.A(label);
			{
				var o = label;
				o.Content = LabelText;
			}

			if(IsPassword){
				var box = new TextBox{};
				Container.A(box);
				{
					var o = box;
					o.Classes.Add(Cls.InputBox);
					o.PasswordChar = '*';
					label.Target = o;
					o.Bind(
						o.PropText
						,TextBinding
					);
				}
			}else{
				var box = new TextBox{};
				Container.A(box);
				{
					var o = box;
					o.Classes.Add(Cls.InputBox);
					label.Target = o;
					o.Bind(
						o.PropText
						,TextBinding
					);
				}
			}

			return null;
		};
	}


}

