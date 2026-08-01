namespace Ngaq.Ui.Views.Settings;
using System.Collections.ObjectModel;
using Ngaq.Ui.Infra;
using Microsoft.Extensions.Logging;
using Ctx = VmCfgFont;
using Ngaq.Core.Infra.Cfg;
using Tsinswreng.CsCfg;
using Ngaq.Ui.Infra.I18n;

public partial class VmCfgFont
	:ViewModelBase
	,IMk<Ctx>
{
	ILogger<VmCfgFont>? Log{get;set;}
	public VmCfgFont(
		ILogger<VmCfgFont>? Log
	){
		this.Log = Log;
	}

	protected VmCfgFont(){}
	public static VmCfgFont Mk(){
		return new VmCfgFont();
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmCfgFont(){
		#if DEBUG
		{
			var o = new Ctx();
			Samples.Add(o);
		}
		#endif
	}


	protected str _InputFontSize = UiCfg.Inst.BaseFontSize+"";
	public str InputFontSize{
		get{return _InputFontSize;}
		set{SetProperty(ref _InputFontSize, value);}
	}


	protected f64 _FontSize = UiCfg.Inst.BaseFontSize;
	public f64 FontSize{
		get{return _FontSize;}
		set{SetProperty(ref _FontSize, value);}
	}


	public nil TryNeoFontSize(){
		try{
			//輸入0旹會崩潰 //TODO: avalonia 全局異常處理
			if(f64.TryParse(InputFontSize, out var numSize)){
				if(numSize <=0 || numSize > 64){
					this.ShowDialog(I18n.Get(KeysUiI18nCommon.FontSizeRangeError));
					return NIL;
				}
				FontSize = numSize;
			}
		}
		catch (System.Exception e){
			HandleErr(e);
		}
		return NIL;
	}

	public async Task<nil> ApplyNeoFontSize(){
		AppCfg.Inst.TrySetNoSave(
			KeysClientCfg.Ui.BaseFontSize.GetFullPathSegs()
			,FontSize
		);
		await AppCfg.Inst.Save(default);
		return NIL;
	}

}

