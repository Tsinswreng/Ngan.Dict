namespace Ngaq.Ui.Views.Dictionary;

using System;
using Ngaq.Core.Frontend.Hotkey;
using Ngaq.Core.Infra.Cfg;
using Tsinswreng.CsCfg;

/// 讀取並解析「剪貼板查詞熱鍵」配置的統一入口。
/// 把配置解析從平臺層抽離，避免 Windows/Linux 重複實現。
public interface IParseDictionaryLookupHotkeyCfg{
	/// 從 AppCfg 讀取熱鍵配置並解析。
	/// <param name="OnWarning">解析遇到未知值時的回調，可選。</param>
	/// <returns>解析後的熱鍵配置（帶回退值）。</returns>
	public DictionaryLookupHotkeyCfg ReadFromAppCfg(Action<str>? OnWarning = null);

	/// 解析原始字串配置。
	/// <param name="RawModifiers">修飾鍵原始值（例如 Alt|Shift）。</param>
	/// <param name="RawKey">主鍵原始值（例如 E）。</param>
	/// <param name="OnWarning">解析遇到未知值時的回調，可選。</param>
	/// <returns>解析後的熱鍵配置（帶回退值）。</returns>
	public DictionaryLookupHotkeyCfg Parse(str RawModifiers, str RawKey, Action<str>? OnWarning = null);
}

/// 熱鍵配置 DTO。
public sealed record DictionaryLookupHotkeyCfg(EHotkeyModifiers Modifiers, EHotkeyKey Key);

/// 「剪貼板查詞熱鍵」配置解析器。
public class DictionaryLookupHotkeyCfgParser : IParseDictionaryLookupHotkeyCfg{
	/// 從全局配置讀取，並委派到純解析邏輯。
	public DictionaryLookupHotkeyCfg ReadFromAppCfg(Action<str>? OnWarning = null){
		var cfg = AppCfg.Inst;
		var rawModifiers = ExtnICfgAccessor.Get(cfg, KeysClientCfg.Hotkey.DictionaryLookup.Modifiers) ?? "";
		var rawKey = ExtnICfgAccessor.Get(cfg, KeysClientCfg.Hotkey.DictionaryLookup.Key) ?? "";
		return Parse(rawModifiers, rawKey, OnWarning);
	}

	/// 解析配置並在失敗時回退到默認值，保證啓動流程穩定。
	public DictionaryLookupHotkeyCfg Parse(str RawModifiers, str RawKey, Action<str>? OnWarning = null){
		var modifiers = ParseModifiers(RawModifiers, EHotkeyModifiers.Alt, OnWarning);
		var key = ParseKey(RawKey, EHotkeyKey.E, OnWarning);
		return new DictionaryLookupHotkeyCfg(modifiers, key);
	}

	/// 解析修飾鍵字符串，支持多值（Ctrl|Shift、Ctrl+Shift、Ctrl Shift）。
	private static EHotkeyModifiers ParseModifiers(
		str Raw,
		EHotkeyModifiers Fallback,
		Action<str>? OnWarning
	){
		if(str.IsNullOrWhiteSpace(Raw)){
			return Fallback;
		}

		var merged = EHotkeyModifiers.None;
		var tokens = Raw.Split(['|', '+', ',', ' '], StringSplitOptions.RemoveEmptyEntries);
		foreach(var token in tokens){
			if(Enum.TryParse<EHotkeyModifiers>(token.Trim(), true, out var item)){
				merged |= item;
				continue;
			}
			OnWarning?.Invoke($"Unknown hotkey modifier in config: {token}");
		}

		return merged == EHotkeyModifiers.None ? Fallback : merged;
	}

	/// 解析主鍵字符串。
	private static EHotkeyKey ParseKey(
		str Raw,
		EHotkeyKey Fallback,
		Action<str>? OnWarning
	){
		if(str.IsNullOrWhiteSpace(Raw)){
			return Fallback;
		}

		if(Enum.TryParse<EHotkeyKey>(Raw.Trim(), true, out var key)){
			return key;
		}

		OnWarning?.Invoke($"Unknown hotkey key in config: {Raw}");
		return Fallback;
	}
}