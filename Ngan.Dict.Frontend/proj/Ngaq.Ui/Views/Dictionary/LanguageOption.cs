namespace Ngaq.Ui.Views.Dictionary;

using Ngaq.Core.Shared.Dictionary.Models;

public class LanguageOption {
	public string DisplayName { get; set; } = "";
	public NormLangDetail LangInfo { get; set; } = new();

	public LanguageOption() { }

	public LanguageOption(string displayName, string code) {
		DisplayName = displayName;
		LangInfo = new NormLangDetail {
			Type = ELangIdentType.Bcp47,
			Code = code,
			NativeName = displayName,
		};
	}

	public override string ToString() => DisplayName;
}

public static class LanguageOptions {
	public static List<LanguageOption> SourceLanguages { get; } = [
		new LanguageOption("English", "en"),
		new LanguageOption("中文", "zh"),
		new LanguageOption("日本語", "ja"),
	];

	public static List<LanguageOption> DefaultTargetLanguages { get; } = [
		new LanguageOption("中文", "zh"),
		new LanguageOption("English", "en"),
		new LanguageOption("日本語", "ja"),
	];

	public static List<LanguageOption> GetTargetLanguages(LanguageOption? sourceLang) {
		if (sourceLang == null) {
			return DefaultTargetLanguages;
		}
		var srcCode = sourceLang.LangInfo.Code;
		return srcCode switch {
			"en" => [new LanguageOption("中文", "zh"), new LanguageOption("日本語", "ja")],
			"zh" => [new LanguageOption("English", "en"), new LanguageOption("日本語", "ja")],
			"ja" => [new LanguageOption("English", "en"), new LanguageOption("中文", "zh")],
			_ => DefaultTargetLanguages,
		};
	}

	public static LanguageOption DefaultSourceLanguage => SourceLanguages[0];
	public static LanguageOption DefaultTargetLanguage => DefaultTargetLanguages[0];
}
