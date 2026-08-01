namespace Ngaq.Ui.Views.Settings.Lang;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ngaq.Core.Infra.Cfg;
using Ngaq.Core.Shared.Word.Svc;
using Ngaq.Ui.Infra;
using Tsinswreng.CsCfg;

using Ctx = VmCfgLang;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

public partial class VmCfgLang: ViewModelBase, IMk<Ctx>{
	/// 语言下拉项：显示 Code + NativeName。
	public sealed class UiLangOption{
		public str Code{get;set;} = "";
		public str NativeName{get;set;} = "";
		public str DisplayText => NativeName == "" ? Code : $"{Code} - {NativeName}";
		public override str ToString(){
			return DisplayText;
		}
	}

	protected VmCfgLang(){
		Cfg = AppCfg.Inst;
		Init();
	}

	public static Ctx Mk(){
		return new Ctx(AppCfg.Inst, null);
	}

	ICfgAccessor? Cfg;
	ISvcNormLang? SvcNormLang;
	public VmCfgLang(
		ICfgAccessor? Cfg,
		ISvcNormLang? SvcNormLang
	){
		this.Cfg = Cfg??AppCfg.Inst;
		this.SvcNormLang = SvcNormLang;
		Init();
	}

	void Init(){
		if(AnyNull(Cfg)){
			return;
		}
		Lang = Cfg.Get(KeysClientCfg.Lang)??"en";
		LangInput = Lang;
	}

	public ObservableCollection<UiLangOption> UiLangOptions{get;set;} = [];

	HashSet<str> UiLangCodeSet{get;set;} = new(StringComparer.OrdinalIgnoreCase);
	Dictionary<str, str> UiLangDisplayToCode{get;set;} = new(StringComparer.OrdinalIgnoreCase);

	public str Lang{
		get;
		set{SetProperty(ref field, value);}
	} = "en";

	/// 绑定到可编辑下拉框输入文本。
	public str LangInput{
		get;
		set{SetProperty(ref field, value);}
	} = "en";

	/// 加載 UI 語言候選列表。列表中的 code 用于下拉與保存校驗。
	public async Task<nil> InitUiLangOptions(CT Ct){
		if(AnyNull(SvcNormLang)){
			return NIL;
		}
		try{
			UiLangOptions.Clear();
			UiLangCodeSet.Clear();
			UiLangDisplayToCode.Clear();
			await foreach(var LangInfo in SvcNormLang.OrdGetUiLangs(Ct).WithCancellation(Ct)){
				var Code = (LangInfo.Code ?? "").Trim();
				var NativeName = (LangInfo.NativeName ?? "").Trim();
				if(Code == ""){
					continue;
				}
				if(!UiLangCodeSet.Add(Code)){
					continue;
				}
				var Option = new UiLangOption{
					Code = Code,
					NativeName = NativeName,
				};
				UiLangOptions.Add(Option);
				UiLangDisplayToCode[Option.DisplayText] = Option.Code;
			}
			var CurOpt = UiLangOptions.FirstOrDefault(x=>x.Code.Equals(Lang, StringComparison.OrdinalIgnoreCase));
			LangInput = CurOpt?.DisplayText ?? Lang;
		}catch(Exception E){
			HandleErr(E);
		}
		return NIL;
	}

	/// 把输入文本规范化为语言 Code。
	/// 支持纯 Code、下拉展示文本、以及 "code - NativeName" 形式。
	str NormalizeInputToLangCode(str Input){
		var Text = (Input ?? "").Trim();
		if(Text == ""){
			return "";
		}
		if(UiLangCodeSet.Contains(Text)){
			return Text;
		}
		if(UiLangDisplayToCode.TryGetValue(Text, out var CodeByDisplay)){
			return CodeByDisplay;
		}
		var Idx = Text.IndexOf(" - ", StringComparison.Ordinal);
		if(Idx > 0){
			var MaybeCode = Text[..Idx].Trim();
			if(UiLangCodeSet.Contains(MaybeCode)){
				return MaybeCode;
			}
		}
		return Text;
	}

	public async Task<nil> Save(CT Ct){
		if(AnyNull(Cfg)){
			return NIL;
		}
		var LangCode = NormalizeInputToLangCode(LangInput);
		Lang = LangCode;
		// 設置頁只允許保存接口給出的候選語言。
		if(UiLangCodeSet.Count > 0 && !UiLangCodeSet.Contains(LangCode)){
			ShowDialog(I18n[K.SelectCandidateLangValue]);
			return NIL;
		}
		await Task.Run(async ()=>{
			Cfg.Set(KeysClientCfg.Lang, LangCode);
			await Cfg.Save(Ct);
		});
		ShowToast(I18n[K.Saved]);
		return NIL;
	}
}

