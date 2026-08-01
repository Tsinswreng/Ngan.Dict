namespace Ngaq.Ui.Views.Settings.Hotkey;

using Ngaq.Core.Infra.Cfg;
using Ngaq.Ui.Infra;
using Tsinswreng.CsCfg;

using Ctx = VmCfgHotkey;

/// 快捷鍵配置 VM。
public partial class VmCfgHotkey: ViewModelBase, IMk<Ctx>{
	protected VmCfgHotkey(){
		Cfg = AppCfg.Inst;
		Init();
	}

	/// DI 缺省工廠。
	/// <returns>Vm 實例。</returns>
	public static Ctx Mk(){
		return new Ctx(AppCfg.Inst);
	}

	ICfgAccessor? Cfg;

	/// 構造函數。
	/// <param name="Cfg">配置訪問器，為 null 時使用全局配置。</param>
	public VmCfgHotkey(ICfgAccessor? Cfg){
		this.Cfg = Cfg ?? AppCfg.Inst;
		Init();
	}

	/// 從配置載入快捷鍵設置。
	void Init(){
		if(AnyNull(Cfg)){
			return;
		}
		DictionaryLookupModifiers = Cfg.Get(KeysClientCfg.Hotkey.DictionaryLookup.Modifiers) ?? "";
		DictionaryLookupKey = Cfg.Get(KeysClientCfg.Hotkey.DictionaryLookup.Key) ?? "";
	}

	/// 查詞熱鍵修飾鍵，支持多值，使用 "|" 分隔。
	public str DictionaryLookupModifiers{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	/// 查詞熱鍵主鍵。
	public str DictionaryLookupKey{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	/// 保存快捷鍵配置。
	/// <param name="Ct">取消令牌。</param>
	/// <returns>空值。</returns>
	public async Task<nil> Save(CT Ct){
		if(AnyNull(Cfg)){
			return NIL;
		}

		await Task.Run(async ()=>{
			Cfg.Set(KeysClientCfg.Hotkey.DictionaryLookup.Modifiers, DictionaryLookupModifiers.Trim());
			Cfg.Set(KeysClientCfg.Hotkey.DictionaryLookup.Key, DictionaryLookupKey.Trim());
			await Cfg.Save(Ct);
		});

		return NIL;
	}
}
