namespace Ngaq.Ui.Views.Dictionary.NormLangTag;

using System.Collections;
using Ngaq.Core.Infra.Cfg;
using Ngaq.Core.Shared.Dictionary.Models;
using Tsinswreng.CsCfg;

/// 詞典源語言快捷標籤配置的函數實現。
public static partial class DictionarySrcLangTagCfg{
	public static partial IList<CfgDictionarySrcLangTag> Load(ICfgAccessor Cfg){
		List<CfgDictionarySrcLangTag> R = [];
		var RawItems = Cfg.Get(KeysClientCfg.Dictionary.SrcLangTags);
		if(RawItems is null){
			return R;
		}
		foreach(var Raw in RawItems){
			var Item = ParseItem(Raw);
			if(Item is null || str.IsNullOrWhiteSpace(Item.Code)){
				continue;
			}
		R.Add(Item);
		}
		return R;
	}

	public static async partial Task<nil> Save(
		ICfgAccessor Cfg,
		IEnumerable<CfgDictionarySrcLangTag> Tags,
		CT Ct
	){
		IList<obj?> RawItems = Tags
			.Where(x=>!str.IsNullOrWhiteSpace(x.Code))
			.Select(ToRaw)
			.Cast<obj?>()
			.ToList();
		Cfg.Set(KeysClientCfg.Dictionary.SrcLangTags, RawItems);
		await Cfg.Save(Ct);
		return NIL;
	}

	private static partial CfgDictionarySrcLangTag? ParseItem(obj? Raw){
		if(Raw is not IDictionary Dict){
			return null;
		}
		var Type = ELangIdentType.Bcp47;
		var TypeRaw = GetValue(Dict, nameof(CfgDictionarySrcLangTag.Type));
		if(TypeRaw is not null){
			if(TypeRaw is i64 TypeNum && Enum.IsDefined(typeof(ELangIdentType), (i32)TypeNum)){
				Type = (ELangIdentType)TypeNum;
			}else if(Enum.TryParse<ELangIdentType>(TypeRaw.ToString(), true, out var ParsedType)){
				Type = ParsedType;
			}
		}
		return new CfgDictionarySrcLangTag{
			Type = Type == ELangIdentType.Unknown ? ELangIdentType.Bcp47 : Type,
			Code = GetValue(Dict, nameof(CfgDictionarySrcLangTag.Code))?.ToString()?.Trim() ?? "",
			Text = GetValue(Dict, nameof(CfgDictionarySrcLangTag.Text))?.ToString() ?? "",
		};
	}

	/// 缺少字段的损坏配置按空值处理，不能让词典页因单项配置错误而无法打开。
	private static obj? GetValue(IDictionary Dict, str Key){
		return Dict.Contains(Key) ? Dict[Key] : null;
	}

	private static partial obj ToRaw(CfgDictionarySrcLangTag Tag){
		return new Dictionary<str, obj?>{
			[nameof(CfgDictionarySrcLangTag.Type)] = Tag.Type.ToString(),
			[nameof(CfgDictionarySrcLangTag.Code)] = Tag.Code.Trim(),
			[nameof(CfgDictionarySrcLangTag.Text)] = Tag.Text.Trim(),
		};
	}
}
