namespace Ngaq.Ui.Views.Settings.LearnWord;
using System.Collections.ObjectModel;
using Ngaq.Core.Infra.Cfg;
using Ngaq.Core.Shared.Kv.Svc;
using Ngaq.Ui.Infra;
using Tsinswreng.CsCfg;
using Tsinswreng.CsCore;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;
using Ctx = VmCfgLearnWord;
public partial class VmCfgLearnWord: ViewModelBase, IMk<Ctx>{
	//蔿從構造函數依賴注入、故以靜態工廠代無參構造器
	protected VmCfgLearnWord(){
		Cfg = AppCfg.Inst;
		Init();
	}
	public static Ctx Mk(){
		return new Ctx(AppCfg.Inst);
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmCfgLearnWord(){
		#if DEBUG
		{
			var o = new Ctx();
			Samples.Add(o);
		}
		#endif
	}

	//ISvcKv? SvcKv;
	ICfgAccessor? Cfg;
	public VmCfgLearnWord(
		ICfgAccessor? Cfg
	){
		var z = this;
		z.Cfg = Cfg??AppCfg.Inst;

		Init();
	}

	void Init(){
		if(AnyNull(Cfg)){
			return;
		}
		EnableRandomBackground = Cfg.Get(KeysClientCfg.Word.EnableRandomBackground);
		EnableAutoPronounce = Cfg.Get(KeysClientCfg.Word.EnableAutoPronounce);
		MaxDisplayedWordCount = (Cfg.Get(KeysClientCfg.Word.MaxDisplayedWordCount)).ToString();
		var langs = Cfg.Get(KeysClientCfg.Word.FilterLanguage) as IList<obj>;
		LanguageFilterExpr = str.Join("\n", langs??[]);

		// var lua = Cfg.Get(ItemsClientCfg.Word.LuaFilterExpr);
		// LuaFilterExpr = lua??"";
	}

	public bool EnableRandomBackground{
		get;
		set{SetProperty(ref field, value);}
	} = false;

	public bool EnableAutoPronounce{
		get;
		set{SetProperty(ref field, value);}
	} = false;

	public str LanguageFilterExpr{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public str MaxDisplayedWordCount{
		get;
		set{SetProperty(ref field, value);}
	} = "500";


	public async Task<nil> Save(CT Ct){
		if(AnyNull(Cfg)){
			return NIL;
		}
		if(!u64.TryParse(MaxDisplayedWordCount, out var maxDisplayedWordCount)){
			ShowDialog(I18n[K.MaxDisplayedWordCountMustBeUnsignedInteger]);
			return NIL;
		}
		//var langs = LanguageFilterExpr.Split('\n').AsOrToList();
		IList<str>? langs = null;
		if(!str.IsNullOrEmpty(LanguageFilterExpr)){
			langs = LanguageFilterExpr.Split('\n').AsOrToList();
		}
		await Task.Run(async()=>{
			Cfg.Set(KeysClientCfg.Word.FilterLanguage, langs);
			Cfg.Set(KeysClientCfg.Word.EnableRandomBackground, EnableRandomBackground);
			Cfg.Set(KeysClientCfg.Word.EnableAutoPronounce, EnableAutoPronounce);
			Cfg.Set(KeysClientCfg.Word.MaxDisplayedWordCount, maxDisplayedWordCount);
			await Cfg.Save(Ct);
		});
		return NIL;
	}



}
