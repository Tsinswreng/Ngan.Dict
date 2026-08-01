namespace Ngaq.Ui.Views.Settings.ServerStorage;

using Ngaq.Core.Infra.Cfg;
using Ngaq.Ui.Infra;
using Tsinswreng.CsCfg;

using Ctx = VmCfgServerStorage;

public partial class VmCfgServerStorage: ViewModelBase, IMk<Ctx>{
	protected VmCfgServerStorage(){
		Cfg = AppCfg.Inst;
		Init();
	}

	public static Ctx Mk(){
		return new Ctx(AppCfg.Inst);
	}

	ICfgAccessor? Cfg;
	public VmCfgServerStorage(ICfgAccessor? Cfg){
		this.Cfg = Cfg??AppCfg.Inst;
		Init();
	}

	void Init(){
		if(AnyNull(Cfg)){
			return;
		}
		ServerBaseUrl = Cfg.Get(KeysClientCfg.ServerBaseUrl)??"";
		SqlitePath = Cfg.Get(KeysClientCfg.SqlitePath)??"";
	}

	public str ServerBaseUrl{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public str SqlitePath{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public async Task<nil> Save(CT Ct){
		if(AnyNull(Cfg)){
			return NIL;
		}
		await Task.Run(async ()=>{
			Cfg.Set(KeysClientCfg.ServerBaseUrl, ServerBaseUrl.Trim());
			Cfg.Set(KeysClientCfg.SqlitePath, SqlitePath.Trim());
			await Cfg.Save(Ct);
		});
		return NIL;
	}
}

