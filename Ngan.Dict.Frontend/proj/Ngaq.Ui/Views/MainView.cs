namespace Ngaq.Ui.Views;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using Ngaq.Core.Infra;
using Ngaq.Core.Infra.Errors;
using Ngaq.Ui.Components.TempusBox;
using Ngaq.Ui.Icons;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.I18n;
using Ngaq.Ui.Tools;
using Ngaq.Ui.ViewModels;
using Ngaq.Ui.Views.Home;
using Tsinswreng.Avln.Navi;
using Tsinswreng.AvlnTools.Controls;
using Tsinswreng.AvlnTools.Dsl;
using Tsinswreng.AvlnTools.Tools;
using Tsinswreng.CsErr;


public class ViewNaviBase:UserControl{

}


public partial class MainView : UserControl {
	/// 當前顯示中的 Toast 容器。每次新顯示前會先關閉舊實例，避免重疊。
	protected Border? _ActiveToast = null;
	/// 用于取消舊 Toast 的自動關閉任務。
	protected CancellationTokenSource? _ToastCts = null;

	public partial Button MkBtnToView(
		Control Target
		,str? Title = null
	){
		var RealTarget = Target;
		var btn = new Button();
		if(Title is not null){
			RealTarget = ToolView.WithTitle(Title, Target);
			btn.Content = Title;
		}

		btn.Click += (s,e)=>{
			MgrViewNavi.Inst.ViewNavi?.GoTo(RealTarget);
		};
		return btn;
	}

	public partial Button MkBtnToView(
		Func<Control> MkTarget
		,str? Title = null
	){
		Control? RealTarget = null;
		var btn = new Button();
		if(Title is not null){
			btn.Content = Title;
		}

		btn.Click += (s,e)=>{
			RealTarget ??= Title is not null
				? ToolView.WithTitle(Title, MkTarget())
				: MkTarget()
			;
			MgrViewNavi.Inst.ViewNavi?.GoTo(RealTarget);
		};
		return btn;
	}

	public partial nil ShowDialog(str Msg){
		Dispatcher.UIThread.Post((Action)(()=>{
			var SvcPopup = this.SvcPopup;
			var msgBox = new MsgBox();
			{var o = msgBox;
				o._CloseBtn.Background = null;
				o._CloseBtn.SetContent(Icons.CloseX(), o=>{
					o.Fill = Brushes.Red;
				});
				o.MinHeight = UiCfg.Inst.WindowHeight*0.2;
				o.MinWidth = UiCfg.Inst.WindowWidth*0.5;
				o._Border.BorderThickness = new Avalonia.Thickness(1);
				o._Border.BorderBrush = UiCfg.Inst.MainColor;
				o._BdrTitle.Background = new SolidColorBrush(new Color(255, 40,40,40));
				o._BdrBody.MaxHeight = UiCfg.Inst.WindowHeight*0.8;

				o._CloseBtn.Click+=(object? s,global::Avalonia.Interactivity.RoutedEventArgs e)=>{
					SvcPopup.ClosePopup();
				};
				o.HorizontalAlignment = HAlign.Center;
				o.VerticalAlignment = VAlign.Center;
				// msgBox._Title.Content = new TextBlock{
				// 	Text = "TestTitle"
				// 	,FontSize = 26
				// };
				o._Body.Content = new SelectableTextBlock {
					Text= Msg,
					HorizontalAlignment = HAlign.Center,
					VerticalAlignment = VAlign.Center,
					TextWrapping = TextWrapping.Wrap,
				};
				o.Background = Brushes.Black;
				var Bdr = new Border();
				Bdr.Child = o;
				Bdr.Padding = new Avalonia.Thickness(40);
				SvcPopup.ShowPopup(Bdr);
			}
		}));
		return NIL;
	}

	public partial nil ShowDialog(str Msg, IList<Button> Operations){
		Dispatcher.UIThread.Post((()=>{
			var SvcPopup = this.SvcPopup;
			var msgBox = new MsgBox();
			{var o = msgBox;
				o._CloseBtn.Background = null;
				o._CloseBtn.SetContent(Icons.CloseX(), o=>{
					o.Fill = Brushes.Red;
				});
				o.MinHeight = UiCfg.Inst.WindowHeight*0.2;
				o.MinWidth = UiCfg.Inst.WindowWidth*0.5;
				o._Border.BorderThickness = new Avalonia.Thickness(1);
				o._Border.BorderBrush = Brushes.White;
				o._BdrTitle.Background = new SolidColorBrush(new Color(255, 40,40,40));
				o._BdrBody.MaxHeight = UiCfg.Inst.WindowHeight*0.8;

				o._CloseBtn.Click+=(object? s,global::Avalonia.Interactivity.RoutedEventArgs e)=>{
					SvcPopup.ClosePopup();
				};
				o.HorizontalAlignment = HAlign.Center;
				o.VerticalAlignment = VAlign.Center;

				// 先展示提示文本，再縱向渲染操作按鈕列。
				var Body = new StackPanel{
					Orientation = Orientation.Vertical,
					Spacing = UiCfg.Inst.BaseFontSize*0.35,
					Margin = new Avalonia.Thickness(
						UiCfg.Inst.BaseFontSize*0.7,
						UiCfg.Inst.BaseFontSize*0.45,
						UiCfg.Inst.BaseFontSize*0.7,
						UiCfg.Inst.BaseFontSize*0.45
					),
					HorizontalAlignment = HAlign.Stretch,
					VerticalAlignment = VAlign.Center,
				};
				Body.A(new SelectableTextBlock{
					Text = Msg,
					TextWrapping = TextWrapping.Wrap,
					HorizontalAlignment = HAlign.Center,
				});

				for(i32 Idx = 0; Idx < Operations.Count; Idx++){
					var Btn = Operations[Idx];
					Btn.HorizontalAlignment = HAlign.Stretch;
					// 點擊操作按鈕時統一關閉彈窗；其他業務行爲由調用方在按鈕自身事件中處理。
					Btn.Click += (s,e)=>{
						SvcPopup.ClosePopup();
					};
					Body.A(Btn);
				}

				o._Body.Content = Body;
				o.Background = Brushes.Black;
				var Bdr = new Border();
				Bdr.Child = o;
				Bdr.Padding = new Avalonia.Thickness(40);
				SvcPopup.ShowPopup(Bdr);
			}
		}));
		return NIL;
	}

	/// <summary>
	/// 統一處理桌面 Escape 與 Android 實體返回鍵。
	/// 先關閉最上層彈窗；沒有彈窗時再嘗試返回上一級視圖。
	/// </summary>
	/// <returns>返回事件是否已被當前 UI 消費。</returns>
	public partial bool TryHandleBackNavigation(){
		if(SvcPopup.Popups.Count > 0){
			SvcPopup.ClosePopup();
			return true;
		}
		return MgrViewNavi.Inst.GetViewNavi().Back();
	}

	/// 在主界面底部顯示可手動關閉的提示條；到期自動關閉。
	public partial nil ShowToast(str Msg, u64 DurationMs = 3000){
		Dispatcher.UIThread.Post((Action)(()=>{
			CloseActiveToast();
			var Toast = MkToastControl(Msg);
			_ActiveToast = Toast;
			Root.A(Toast);
			ScheduleCloseToast(DurationMs, Toast);
		}));
		return NIL;
	}

	/// 構造 Toast 控件。右上角提供關閉按鈕，內容區展示文案。
	protected Border MkToastControl(str Msg){
		var FontSize = UiCfg.Inst.BaseFontSize;
		var Container = new Grid{
			HorizontalAlignment = HAlign.Stretch,
			VerticalAlignment = VAlign.Bottom,
			Margin = new Avalonia.Thickness(FontSize*0.8, 0, FontSize*0.8, FontSize*0.8),
		};
		var ToastBody = new Border{
			Background = new SolidColorBrush(new Color(255, 40, 40, 40)),
			BorderBrush = UiCfg.Inst.MainColor,
			BorderThickness = new Avalonia.Thickness(1),
			Padding = new Avalonia.Thickness(FontSize*0.55, FontSize*0.45, FontSize*0.45, FontSize*0.45),
			HorizontalAlignment = HAlign.Stretch,
			VerticalAlignment = VAlign.Bottom,
		};
		var Layout = new Grid{
			ColumnDefinitions = new ColumnDefinitions("*,Auto"),
			HorizontalAlignment = HAlign.Stretch,
			VerticalAlignment = VAlign.Top,
		};
		var Text = new SelectableTextBlock{
			Text = Msg,
			TextWrapping = TextWrapping.Wrap,
			HorizontalAlignment = HAlign.Stretch,
			VerticalAlignment = VAlign.Center,
			FontSize = FontSize,
			Foreground = UiCfg.Inst.ForegroundColor,
			Margin = new Avalonia.Thickness(0, 0, FontSize*0.5, 0),
		};
		Grid.SetColumn(Text, 0);
		var CloseBtn = new Button{
			Background = Brushes.Transparent,
			HorizontalAlignment = HAlign.Right,
			VerticalAlignment = VAlign.Top,
			Padding = new Avalonia.Thickness(0),
		};
		CloseBtn.SetContent(Icons.CloseX(), o=>{
			o.Fill = Brushes.Red;
		});
		CloseBtn.Click += (s,e)=>{
			CloseActiveToast();
		};
		Grid.SetColumn(CloseBtn, 1);
		Layout.Children.Add(Text);
		Layout.Children.Add(CloseBtn);
		ToastBody.Child = Layout;
		Container.Children.Add(ToastBody);
		return new Border{
			Child = Container,
			HorizontalAlignment = HAlign.Stretch,
			VerticalAlignment = VAlign.Bottom,
		};
	}

	/// 啟動自動關閉任務。若中途出現新 Toast，舊任務會被取消。
	protected void ScheduleCloseToast(u64 DurationMs, Border CurrentToast){
		_ToastCts?.Cancel();
		_ToastCts?.Dispose();
		_ToastCts = new CancellationTokenSource();
		var Ct = _ToastCts.Token;
		_ = Task.Run(async ()=>{
			try{
				await Task.Delay((i32)Math.Max(1UL, DurationMs), Ct);
				if(Ct.IsCancellationRequested){
					return;
				}
				Dispatcher.UIThread.Post((Action)(()=>{
					if(ReferenceEquals(_ActiveToast, CurrentToast)){
						CloseActiveToast();
					}
				}));
			}catch(TaskCanceledException){
			}
		});
	}

	/// 關閉當前 Toast 並清理其計時任務。
	protected void CloseActiveToast(){
		_ToastCts?.Cancel();
		_ToastCts?.Dispose();
		_ToastCts = null;
		if(_ActiveToast is not null && Root.Children.Contains(_ActiveToast)){
			Root.Children.Remove(_ActiveToast);
		}
		_ActiveToast = null;
	}

	public partial nil HandleErr(obj? Ex){
		str? toLog = null;
		if(Ex is IAppErr Err){
			if(Err.Type is not null){
				var Str = I18n.Get(Err.Type.ToI18nKey(), Err.Args??[]);
				ShowDialog(Str);
				toLog += "\n"+Str+"\n"+str.Join("\n",Err.DebugArgs??[]);
			}
			toLog += "\n"+Err;
		}
		else if(Ex is Exception Exception){
			toLog = Exception+"";
			ShowDialog(I18n.Get(KeysErr.Common.UnknownErr.ToI18nKey()));
		}
		else{//非Exception 非 IAppErr
			toLog??="";
			toLog += "\n"+Ex+"";
			var err = KeysErr.Common.UnknownErr.ToErr();
			if(err.Type is not null){
				var Str = I18n.Get(err.Type.ToI18nKey(), err.Args??[]);
				ShowDialog(Str);
			}
		}
		Logger?.LogError(toLog);
		return NIL;
	}



	public MainView() {
		Inst ??= this;
		#if DEBUG
		Try();
		#endif
		DataContext = new MainViewModel();
		SvcPopup = new SvcPopup(Root);
		this.SetContent(GridStack.Grid);
		GridStack.Grid.SetRowDefs([
			new(1, GUT.Star),
		]);
		GridStack.Add(ViewNaviBase);
		InputElement.KeyDownEvent.AddClassHandler<TopLevel>(
			(s,e)=>{
				if(e.Key == Avalonia.Input.Key.Escape){
					if(TryHandleBackNavigation()){
						e.Handled = true;
					}
				}
			}
			,handledEventsToo: true
		);

		MgrViewNavi.Inst.ViewNavi = new ViewNavi(ViewNaviBase);
		var Navi = MgrViewNavi.Inst.ViewNavi;


		var Home = new ViewHome();
		// var Home = new ViewWordEditV2();

		Navi.GoTo(Home);

	}
}


