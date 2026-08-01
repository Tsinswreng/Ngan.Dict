namespace Ngaq.Ui.Tools;
using Avalonia.Controls;
using Avalonia.Threading;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.Ctrls;

public static class ToolBtn{
	extension<T>(T z)
		where T: Button
	{

		/// 裝飾潙語義ʸʹ按鈕(即伸展, 內容居中)
		public T StretchCenter(){
			var o = z;
			o.HorizontalAlignment = HAlign.Stretch;
			o.HorizontalContentAlignment = HAlign.Center;
			o.VerticalAlignment = VAlign.Stretch;
			o.VerticalContentAlignment = VAlign.Center;
			return o;
		}
	}

	extension<T>(T z)
		where T: OpBtn
	{

		public IBtn ToIBtn(){
			return new OpBtnAsIBtn(z);
		}

		public async Task<nil> ClickAndWaitDone(CT Ct){
			z.PerformClick();
			while(z.State == OpBtn.EState.Working){
				Ct.ThrowIfCancellationRequested();
				await Task.Delay(10, Ct);
			}
			return NIL;
		}

		public T HookDoneEvent(Action OnDone){
			var oldOk = z.FnOk;
			var oldFail = z.FnFail;
			var oldCancel = z.FnCancel;
			z.FnOk = ()=>{
				oldOk?.Invoke();
				ToolBtn.PostDone(OnDone);
				return null;
			};
			z.FnFail = ex=>{
				oldFail?.Invoke(ex);
				ToolBtn.PostDone(OnDone);
				return null;
			};
			z.FnCancel = ()=>{
				oldCancel?.Invoke();
				ToolBtn.PostDone(OnDone);
				return null;
			};
			return z;
		}
	}

	static void PostDone(Action OnDone){
		Dispatcher.UIThread.Post(()=>{
			Dispatcher.UIThread.Post(()=>{
				OnDone();
			});
		});
	}

	sealed class OpBtnAsIBtn: IBtn{
		readonly OpBtn _OpBtn;

		public OpBtnAsIBtn(OpBtn OpBtn){
			_OpBtn = OpBtn;
			_OpBtn.HookDoneEvent(()=>{
				Done?.Invoke(this, EventArgs.Empty);
			});
		}

		public obj? Content{
			get{return _OpBtn.BtnContent;}
		}

		public async Task<nil> Click(CT Ct){
			return await _OpBtn.ClickAndWaitDone(Ct);
		}

		public event EventHandler? Done;
	}

}
