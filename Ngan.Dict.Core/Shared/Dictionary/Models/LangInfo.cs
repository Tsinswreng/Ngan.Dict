namespace Ngaq.Core.Shared.Dictionary.Models;
/// 语言信息（源/目标/额外目标语言通用结构）
[Obsolete(@$"use {nameof(NormLangDetail)}")]
public class LangInfo {
	/// ISO 639-1 语言代码（如en、zh、ja）
	public string Iso639_1 { get; set; } = "";

	/// 可选：地区变体（如us|uk|au|cn|jp...）
	public string? Variety { get; set; }

	/// 可选：书写系统（用于多书写语言如中文简繁，hans=简体, hant=繁体）
	public string? Script { get; set; }
}

