namespace Ngaq.Ui.Views.User.Profile;

using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Media;
using Ngaq.Ui.Components;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.Ctrls;
using Ngaq.Ui.Tools;
using Ngaq.Ui.User;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsI18n;
using Ursa.Controls;
using Ctx = VmUserProfile;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;
using Ngaq.Ui.Icons;
using Tsinswreng.Avln.Grid;

public partial class ViewUserProfile
	:AppViewBase
{

	public Ctx? Ctx{
		get{return DataContext as Ctx;}
		set{
			if(ReferenceEquals(DataContext, value)){
				return;
			}
			BindVm(DataContext as Ctx, false);
			DataContext = value;
			BindVm(value, true);
		}
	}

	public ViewUserProfile(){
		Ctx = App.GetRSvc<Ctx>();
		Style();
		Render();
		DetachedFromVisualTree += (s,e)=>{
			BindVm(Ctx, false);
		};
	}

	protected nil BindVm(Ctx? Vm, bool Enable){
		if(Vm is null){
			return NIL;
		}
		if(Enable){
			Vm.OnLogoutSucceeded += HandleLogoutSucceeded;
		}else{
			Vm.OnLogoutSucceeded -= HandleLogoutSucceeded;
		}
		return NIL;
	}

	protected void HandleLogoutSucceeded(object? Sender, EvtArgMsg Evt){
		Dispatcher.UIThread.Post(()=>{
			ViewNavi?.GoTo(ToolView.WithTitle(I[K.Login], new ViewLoginRegister()));
		});
	}

	public partial class Cls{

	}
	public GridStack Root = new(IsRow: true);

	protected nil Style(){
		return NIL;
	}

	protected TextBox TxtBox(){
		return new TextBox();
	}

	protected nil Render(){
		this.SetContent(Root.Grid, o=>{
			o.SetRowDefs([
				new(1, GUT.Auto),
				new(1, GUT.Auto),
			]);
		});
		Root.A(new ScrollViewer(), sv=>{
			var ContentGrid = new GridStack(IsRow: true);
			sv.SetContent(ContentGrid.Grid, o=>{
				o.SetRowDefs([
					new(4, GUT.Star),
					new(1, GUT.Star),
					new(1, GUT.Auto),
					new(1, GUT.Auto),
					new(1, GUT.Auto),
				]);

				ContentGrid
				.A(new CircleAvatar(), o=>{
					try{
						o.Source = DfltAvatar.Img;//TODO
						o.VerticalAlignment = VAlign.Center;
						o.HorizontalAlignment = HAlign.Center;
						o.Width = 150;
						o.Height = 150;
					}catch{}
				})
				.A(new Border())
				.A(new TextBlock(), o=>{
					o.Text = I[K.UserId];
				})
				.A(new SelectableTextBlock(), o=>{
					o.Bind(o.PropText,CBE.Mk<Ctx>(x=>x.UserIdRepr));
				})
				.A(new StackPanel(), Sp=>{
					Sp
						.A(new Button(), o=>{
						o.StretchCenter();
						o.Content = Icons.UserSwitch().ToIcon().WithText(I[K.ChangeAccount]);
						o.Click += (s,e)=>{
							ViewNavi?.GoTo(
								ToolView.WithTitle(I[K.ChangeAccount], new ViewLoginRegister())
							);
						};
					})
					.A(new OpBtn(), o=>{
						o._Button.StretchCenter();
						o._Button.Background = Brushes.Red;
						o.BtnContent = Icons.RegularLogOutCircle().ToIcon().WithText(I[K.Logout]);;
						o.SetExe((Ct)=>Ctx?.LogoutAsy(Ct));
					});
				});
			});
		});
		return NIL;
	}
}
