namespace Ngaq.Ui.Views.Dictionary.NormLangTag;

using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.Dictionary.Models.Po.NormLang;
using Ctx = VmNormLangTag;

/// 詞典源語言快捷標籤 ViewModel 的函數實現。
public partial class VmNormLangTag{
	protected partial VmNormLangTag(){}

	public static partial Ctx Mk(){
		return new Ctx();
	}

	public partial nil FromCfg(CfgDictionarySrcLangTag Cfg, PoNormLang? Po){
		Type = Cfg.Type == ELangIdentType.Unknown ? ELangIdentType.Bcp47 : Cfg.Type;
		Code = (Cfg.Code ?? "").Trim();
		Text = Cfg.Text ?? "";
		NativeName = Po?.NativeName ?? "";
		return NIL;
	}

	public partial CfgDictionarySrcLangTag ToCfg(){
		return new CfgDictionarySrcLangTag{
			Type = Type == ELangIdentType.Unknown ? ELangIdentType.Bcp47 : Type,
			Code = Code.Trim(),
			Text = Text.Trim(),
		};
	}

	public partial bool IsLang(ELangIdentType Type, str Code){
		return this.Type == Type
			&& str.Equals(this.Code, Code, StringComparison.OrdinalIgnoreCase);
	}
}
