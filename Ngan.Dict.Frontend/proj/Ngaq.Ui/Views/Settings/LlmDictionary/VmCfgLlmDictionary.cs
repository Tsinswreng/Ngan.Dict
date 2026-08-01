namespace Ngaq.Ui.Views.Settings.LlmDictionary;

using Ngaq.Core.Infra.Cfg;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Shared.Dictionary.Svc;
using Ngaq.Ui.Infra;
using Avalonia.Threading;
using Tsinswreng.CsCfg;

using Ctx = VmCfgLlmDictionary;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

public partial class VmCfgLlmDictionary: ViewModelBase, IMk<Ctx>{
	protected VmCfgLlmDictionary(){
		Cfg = AppCfg.Inst;
		_ = Init(default);
	}

	public static Ctx Mk(){
		return new Ctx(AppCfg.Inst, null, null);
	}

	ICfgAccessor? Cfg;
	ISvcDictionary? SvcDictionary;
	IFrontendUserCtxMgr? FrontendUserCtxMgr;
	public VmCfgLlmDictionary(
		ICfgAccessor? Cfg,
		ISvcDictionary? SvcDictionary,
		IFrontendUserCtxMgr? FrontendUserCtxMgr
	){
		this.Cfg = Cfg??AppCfg.Inst;
		this.SvcDictionary = SvcDictionary;
		this.FrontendUserCtxMgr = FrontendUserCtxMgr;
		_ = Init(default);
	}

	/// 初始化設置頁顯示值。
	/// ApiUrl / ApiKey / Model / ExtraBodyJson 直接來自本地配置；
	/// Prompt 經由詞典服務讀取，以便自動落默認值。
	public async Task<nil> Init(CT Ct){
		if(AnyNull(Cfg)){
			return NIL;
		}
		ApiUrl = Cfg.Get(KeysClientCfg.LlmDictionary.ApiUrl)??"";
		ApiKey = Cfg.Get(KeysClientCfg.LlmDictionary.ApiKey)??"";
		Model = Cfg.Get(KeysClientCfg.LlmDictionary.Model)??"";
		ExtraBodyJson = Cfg.Get(KeysClientCfg.LlmDictionary.ExtraBodyJson)??"";
		var PromptFromCfg = Cfg.Get(KeysClientCfg.LlmDictionary.Prompt)??"";
		Prompt = PromptFromCfg;
		if(AnyNull(SvcDictionary, FrontendUserCtxMgr)){
			return NIL;
		}
		try{
			var LoadedPrompt = await Task.Run(
				()=>SvcDictionary.GetLlmDictSysPromptOrDflt(FrontendUserCtxMgr.GetDbUserCtx(), Ct),
				Ct
			);
			Dispatcher.UIThread.Post(()=>{
				Prompt = LoadedPrompt;
			});
		}catch(Exception Ex){
			HandleErr(Ex);
		}
		return NIL;
	}

	public str ApiUrl{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public str ApiKey{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public str Model{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	/// 直接寫入請求體頂層的額外 JSON 對象。
	/// 用於配置 provider-specific 參數，例如關閉某些模型的默認思考模式。
	public str ExtraBodyJson{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public str Prompt{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public async Task<nil> Save(CT Ct){
		if(AnyNull(Cfg)){
			return NIL;
		}
		try{
			await Task.Run(async ()=>{
				Cfg.Set(KeysClientCfg.LlmDictionary.ApiUrl, ApiUrl.Trim());
				Cfg.Set(KeysClientCfg.LlmDictionary.ApiKey, ApiKey.Trim());
				Cfg.Set(KeysClientCfg.LlmDictionary.Model, Model.Trim());
				Cfg.Set(KeysClientCfg.LlmDictionary.ExtraBodyJson, ExtraBodyJson.Trim());
				await Cfg.Save(Ct);
				if(!AnyNull(SvcDictionary, FrontendUserCtxMgr)){
					var SavedPrompt = await SvcDictionary.SetLlmDictSysPrompt(
						FrontendUserCtxMgr.GetDbUserCtx(),
						Prompt,
						Ct
					);
					Dispatcher.UIThread.Post(()=>{
						Prompt = SavedPrompt;
					});
				}else{
					Cfg.Set(KeysClientCfg.LlmDictionary.Prompt, Prompt);
					await Cfg.Save(Ct);
				}
			}, Ct);
			ShowToast(I18n[K.Saved]);
		}catch(Exception Ex){
			HandleErr(Ex);
		}
		return NIL;
	}
}

