namespace Ngaq.Core.Shared.Dictionary.Models.Po.NormLang;

public class InitNormLang {
	public static INormLangDetail Mk(
		str Code, str NativeName, str EnglishName, f64 Weight
	) {
		var R = new NormLangDetail();
		R.Code = Code;
		R.NativeName = NativeName;
		R.EnglishName = EnglishName;
		R.Weight = Weight;
		return R;
	}

	[Doc(@$"內容不固定、後續可能增改")]
	public static IList<INormLangDetail> GetNormLangList() {
		return new List<INormLangDetail> {
			// 英语及变体 //帶地區變體的默認0權重
			Mk("en", "English", "English", 100),
			Mk("en-US", "American English", "American English", 0),
			Mk("en-GB", "British English", "British English", 0),
			Mk("en-AU", "Australian English", "Australian English", 0),
			Mk("en-CA", "Canadian English", "Canadian English", 0),
			Mk("en-IN", "Indian English", "Indian English", 0),

			// 中文及变体
			Mk("zh", "中文", "Chinese", 0),
			Mk("zh-CN", "简体中文", "Simplified Chinese", 99),
			Mk("zh-TW", "繁體中文", "Traditional Chinese (Taiwan)", 95),
			Mk("zh-HK", "香港繁體中文", "Traditional Chinese (Hong Kong)", 0),
			Mk("zh-SG", "新加坡简体中文", "Simplified Chinese (Singapore)", 0),

			// 西班牙语及变体
			Mk("es", "Español", "Spanish", 98),
			Mk("es-ES", "Español (España)", "Spanish (Spain)", 0),
			Mk("es-MX", "Español (México)", "Spanish (Mexico)", 0),
			Mk("es-AR", "Español (Argentina)", "Spanish (Argentina)", 0),
			Mk("es-CO", "Español (Colombia)", "Spanish (Colombia)", 0),

			// 法语及变体
			Mk("fr", "Français", "French", 85),
			Mk("fr-FR", "Français (France)", "French (France)", 0),
			Mk("fr-CA", "Français (Canada)", "French (Canada)", 0),
			Mk("fr-BE", "Français (Belgique)", "French (Belgium)", 0),
			Mk("fr-CH", "Français (Suisse)", "French (Switzerland)", 0),

			// 德语及变体
			Mk("de", "Deutsch", "German", 82),
			Mk("de-DE", "Deutsch (Deutschland)", "German (Germany)", 0),
			Mk("de-AT", "Deutsch (Österreich)", "German (Austria)", 0),
			Mk("de-CH", "Deutsch (Schweiz)", "German (Switzerland)", 0),

			// 意大利语及变体
			Mk("it", "Italiano", "Italian", 70),
			Mk("it-IT", "Italiano (Italia)", "Italian (Italy)", 0),
			Mk("it-CH", "Italiano (Svizzera)", "Italian (Switzerland)", 0),

			// 葡萄牙语及变体
			Mk("pt", "Português", "Portuguese", 90),
			Mk("pt-PT", "Português (Portugal)", "Portuguese (Portugal)", 0),
			Mk("pt-BR", "Português (Brasil)", "Portuguese (Brazil)", 0),

			// 俄语及变体
			Mk("ru", "Русский", "Russian", 87),
			Mk("ru-RU", "Русский (Россия)", "Russian (Russia)", 0),
			Mk("ru-UA", "Русский (Украина)", "Russian (Ukraine)", 0),

			// 日语
			Mk("ja", "日本語", "Japanese", 85),
			Mk("ja-JP", "日本語 (日本)", "Japanese (Japan)", 0),

			// 韩语
			Mk("ko", "한국어", "Korean", 80),
			Mk("ko-KR", "한국어 (대한민국)", "Korean (South Korea)", 0),

			// 阿拉伯语及变体
			Mk("ar", "العربية", "Arabic", 92),
			Mk("ar-SA", "العربية (السعودية)", "Arabic (Saudi Arabia)", 0),
			Mk("ar-EG", "العربية (مصر)", "Arabic (Egypt)", 0),
			Mk("ar-AE", "العربية (الإمارات)", "Arabic (UAE)", 0),

			// 印地语
			Mk("hi", "हिन्दी", "Hindi", 94),
			Mk("hi-IN", "हिन्दी (भारत)", "Hindi (India)", 0),

			// 孟加拉语
			Mk("bn", "বাংলা", "Bengali", 85),
			Mk("bn-IN", "বাংলা (ভারত)", "Bengali (India)", 0),
			Mk("bn-BD", "বাংলা (বাংলাদেশ)", "Bengali (Bangladesh)", 0),

			// 旁遮普语
			Mk("pa", "ਪੰਜਾਬੀ", "Punjabi", 75),
			Mk("pa-IN", "ਪੰਜਾਬੀ (ਭਾਰਤ)", "Punjabi (India)", 0),
			Mk("pa-PK", "پنجابی (پاکستان)", "Punjabi (Pakistan)", 0),

			// 泰米尔语
			Mk("ta", "தமிழ்", "Tamil", 72),
			Mk("ta-IN", "தமிழ் (இந்தியா)", "Tamil (India)", 0),
			Mk("ta-LK", "தமிழ் (இலங்கை)", "Tamil (Sri Lanka)", 0),

			// 泰卢固语
			Mk("te", "తెలుగు", "Telugu", 70),
			Mk("te-IN", "తెలుగు (భారతదేశం)", "Telugu (India)", 0),

			// 马拉地语
			Mk("mr", "मराठी", "Marathi", 68),
			Mk("mr-IN", "मराठी (भारत)", "Marathi (India)", 0),

			// 乌尔都语
			Mk("ur", "اردو", "Urdu", 68),
			Mk("ur-PK", "اردو (پاکستان)", "Urdu (Pakistan)", 0),
			Mk("ur-IN", "اردو (بھارت)", "Urdu (India)", 0),

			// 土耳其语
			Mk("tr", "Türkçe", "Turkish", 75),
			Mk("tr-TR", "Türkçe (Türkiye)", "Turkish (Turkey)", 0),

			// 荷兰语及变体
			Mk("nl", "Nederlands", "Dutch", 60),
			Mk("nl-NL", "Nederlands (Nederland)", "Dutch (Netherlands)", 59),
			Mk("nl-BE", "Nederlands (België)", "Dutch (Belgium)", 55),

			// 波兰语
			Mk("pl", "Polski", "Polish", 60),
			Mk("pl-PL", "Polski (Polska)", "Polish (Poland)", 59),

			// 瑞典语
			Mk("sv", "Svenska", "Swedish", 55),
			Mk("sv-SE", "Svenska (Sverige)", "Swedish (Sweden)", 54),

			// 丹麦语
			Mk("da", "Dansk", "Danish", 50),
			Mk("da-DK", "Dansk (Danmark)", "Danish (Denmark)", 49),

			// 挪威语
			Mk("no", "Norsk", "Norwegian", 48),
			Mk("nb-NO", "Norsk bokmål (Norge)", "Norwegian Bokmål (Norway)", 47),
			Mk("nn-NO", "Norsk nynorsk (Noreg)", "Norwegian Nynorsk (Norway)", 45),

			// 芬兰语
			Mk("fi", "Suomi", "Finnish", 48),
			Mk("fi-FI", "Suomi (Suomi)", "Finnish (Finland)", 47),

			// 希腊语
			Mk("el", "Ελληνικά", "Greek", 48),
			Mk("el-GR", "Ελληνικά (Ελλάδα)", "Greek (Greece)", 47),

			// 希伯来语
			Mk("he", "עברית", "Hebrew", 46),
			Mk("he-IL", "עברית (ישראל)", "Hebrew (Israel)", 45),

			// 泰语
			Mk("th", "ไทย", "Thai", 62),
			Mk("th-TH", "ไทย (ไทย)", "Thai (Thailand)", 61),

			// 越南语
			Mk("vi", "Tiếng Việt", "Vietnamese", 70),
			Mk("vi-VN", "Tiếng Việt (Việt Nam)", "Vietnamese (Vietnam)", 69),

			// 印尼语
			Mk("id", "Bahasa Indonesia", "Indonesian", 75),
			Mk("id-ID", "Bahasa Indonesia (Indonesia)", "Indonesian (Indonesia)", 74),

			// 马来语
			Mk("ms", "Bahasa Melayu", "Malay", 65),
			Mk("ms-MY", "Bahasa Melayu (Malaysia)", "Malay (Malaysia)", 64),
			Mk("ms-SG", "Bahasa Melayu (Singapura)", "Malay (Singapore)", 55),

			// 菲律宾语
			Mk("fil", "Filipino", "Filipino", 55),
			Mk("fil-PH", "Filipino (Pilipinas)", "Filipino (Philippines)", 54),

			// 匈牙利语
			Mk("hu", "Magyar", "Hungarian", 50),
			Mk("hu-HU", "Magyar (Magyarország)", "Hungarian (Hungary)", 49),

			// 捷克语
			Mk("cs", "Čeština", "Czech", 50),
			Mk("cs-CZ", "Čeština (Česko)", "Czech (Czech Republic)", 49),

			// 罗马尼亚语
			Mk("ro", "Română", "Romanian", 50),
			Mk("ro-RO", "Română (România)", "Romanian (Romania)", 49),

			// 乌克兰语
			Mk("uk", "Українська", "Ukrainian", 55),
			Mk("uk-UA", "Українська (Україна)", "Ukrainian (Ukraine)", 54),

			// 爪哇语
			Mk("jv", "Basa Jawa", "Javanese", 60),

			// 卡纳达语
			Mk("kn", "ಕನ್ನಡ", "Kannada", 55),

			// 古吉拉特语
			Mk("gu", "ગુજરાતી", "Gujarati", 55),

			// 迈蒂利语
			Mk("mai", "मैथिली", "Maithili", 50),

			// 奥里亚语
			Mk("or", "ଓଡ଼ିଆ", "Odia", 50),

			// 信德语
			Mk("sd", "سنڌي", "Sindhi", 48),

			// 阿萨姆语
			Mk("as", "অসমীয়া", "Assamese", 45),

			// 尼泊尔语
			Mk("ne", "नेपाली", "Nepali", 48),

			// 僧伽罗语
			Mk("si", "සිංහල", "Sinhala", 48),

			// 高棉语
			Mk("km", "ភាសាខ្មែរ", "Khmer", 48),

			// 老挝语
			Mk("lo", "ພາສາລາວ", "Lao", 45),

			// 缅甸语
			Mk("my", "မြန်မာစာ", "Burmese", 48),

			// 宿务语
			Mk("ceb", "Cebuano", "Cebuano", 45),

			// 伊洛卡诺语
			Mk("ilo", "Ilokano", "Ilocano", 40),

			// 希利盖农语
			Mk("hil", "Hiligaynon", "Hiligaynon", 40),

			// 比科尔语
			Mk("bik", "Bikol", "Bikol", 38),

			// 瓦瑞语
			Mk("war", "Waray", "Waray", 38),

			// 邦阿西南语
			Mk("pag", "Pangasinan", "Pangasinan", 35),

			// 邦板牙语
			Mk("pam", "Kapampangan", "Kapampangan", 35),

			// 米南加保语
			Mk("min", "Baso Minangkabau", "Minangkabau", 45),

			// 布吉语
			Mk("bug", "Basa Ugi", "Buginese", 40),

			// 亚齐语
			Mk("ace", "Bahsa Acèh", "Acehnese", 38),

			// 巴厘语
			Mk("ban", "Basa Bali", "Balinese", 40),

			// 班贾语
			Mk("bjn", "Bahasa Banjar", "Banjar", 38),

			// 马都拉语
			Mk("mad", "Bhâsa Mâdhurâ", "Madurese", 38),

			// 望加锡语
			Mk("mak", "Basang Mangkasaraʼ", "Makassarese", 35),

			// 德顿语
			Mk("tet", "Tetun", "Tetum", 30),

			// 帕皮阿门托语
			Mk("pap", "Papiamentu", "Papiamento", 25),

			// 毛利语
			Mk("mi", "Māori", "Maori", 20),

			// 夏威夷语
			Mk("haw", "ʻŌlelo Hawaiʻi", "Hawaiian", 10),

			// 斐济语
			Mk("fj", "Vosa Vakaviti", "Fijian", 15),

			// 比斯拉马语
			Mk("bi", "Bislama", "Bislama", 10),

			// 巴布亚皮钦语
			Mk("tpi", "Tok Pisin", "Tok Pisin", 15),

			// 普什图语
			Mk("ps", "پښتو", "Pashto", 45),

			// 塔吉克语
			Mk("tg", "Тоҷикӣ", "Tajik", 40),

			// 土库曼语
			Mk("tk", "Türkmençe", "Turkmen", 40),

			// 柯尔克孜语
			Mk("ky", "Кыргызча", "Kyrgyz", 38),

			// 迪维希语
			Mk("dv", "ދިވެހި", "Dhivehi", 20),

			// 哈萨克语
			Mk("kk", "Қазақша", "Kazakh", 45),

			// 蒙古语
			Mk("mn", "Монгол хэл", "Mongolian", 40),

			// 维吾尔语
			Mk("ug", "ئۇيغۇرچە", "Uyghur", 35),

			// 斯瓦希里语
			Mk("sw", "Kiswahili", "Swahili", 55),

			// 祖鲁语
			Mk("zu", "isiZulu", "Zulu", 45),

			// 科萨语
			Mk("xh", "isiXhosa", "Xhosa", 45),

			// 塞索托语
			Mk("st", "Sesotho", "Sotho", 40),

			// 茨瓦纳语
			Mk("tn", "Setswana", "Tswana", 40),

			// 斯瓦蒂语
			Mk("ss", "SiSwati", "Swati", 35),

			// 聪加语
			Mk("ts", "Xitsonga", "Tsonga", 35),

			// 文达语
			Mk("ve", "Tshivenḓa", "Venda", 30),

			// 恩德贝莱语
			Mk("nd", "isiNdebele", "Ndebele", 30),

			// 齐切瓦语
			Mk("ny", "Chichewa", "Chichewa", 35),

			// 绍纳语
			Mk("sn", "chiShona", "Shona", 45),

			// 约鲁巴语
			Mk("yo", "Èdè Yorùbá", "Yoruba", 45),

			// 伊博语
			Mk("ig", "Ásụ̀sụ́ Ìgbò", "Igbo", 45),

			// 豪萨语
			Mk("ha", "Hausa", "Hausa", 48),

			// 富拉语
			Mk("ff", "Fulfulde", "Fula", 40),

			// 阿肯语
			Mk("ak", "Akan", "Akan", 35),

			// 埃维语
			Mk("ee", "Eʋegbe", "Ewe", 30),

			// 班巴拉语
			Mk("bm", "Bamanankan", "Bambara", 35),

			// 林加拉语
			Mk("ln", "Lingála", "Lingala", 35),

			// 刚果语
			Mk("kg", "Kikongo", "Kongo", 35),

			// 卢巴语
			Mk("lu", "Tshiluba", "Luba-Kasai", 30),

			// 卢旺达语
			Mk("rw", "Kinyarwanda", "Kinyarwanda", 40),

			// 基隆迪语
			Mk("rn", "Ikirundi", "Kirundi", 38),

			// 卢干达语
			Mk("lg", "Luganda", "Ganda", 35),

			// 尼扬科莱语
			Mk("nyn", "Runyankore", "Nyankole", 25),

			// 奇加语
			Mk("cgg", "Rukiga", "Chiga", 25),

			// 特索语
			Mk("teo", "Ateso", "Teso", 20),

			// 卡伦金语
			Mk("kln", "Kalenjin", "Kalenjin", 20),

			// 卡姆巴语
			Mk("kam", "Kikamba", "Kamba", 20),

			// 梅鲁语
			Mk("mer", "Kimeru", "Meru", 20),

			// 古西语
			Mk("guz", "Ekegusii", "Gusii", 20),

			// 索马里语
			Mk("so", "Soomaaliga", "Somali", 35),

			// 奥罗莫语
			Mk("om", "Oromoo", "Oromo", 40),

			// 提格里尼亚语
			Mk("ti", "ትግርኛ", "Tigrinya", 35),

			// 阿法尔语
			Mk("aa", "Qafar", "Afar", 20),

			// 阿尔巴尼亚语
			Mk("sq", "Shqip", "Albanian", 42),

			// 马其顿语
			Mk("mk", "Македонски", "Macedonian", 40),

			// 保加利亚语
			Mk("bg", "Български", "Bulgarian", 45),

			// 塞尔维亚语
			Mk("sr", "Српски", "Serbian", 48),

			// 克罗地亚语
			Mk("hr", "Hrvatski", "Croatian", 46),

			// 波斯尼亚语
			Mk("bs", "Bosanski", "Bosnian", 44),

			// 斯洛文尼亚语
			Mk("sl", "Slovenščina", "Slovenian", 40),

			// 立陶宛语
			Mk("lt", "Lietuvių", "Lithuanian", 38),

			// 拉脱维亚语
			Mk("lv", "Latviešu", "Latvian", 36),

			// 爱沙尼亚语
			Mk("et", "Eesti", "Estonian", 34),

			// 马耳他语
			Mk("mt", "Malti", "Maltese", 30),

			// 冰岛语
			Mk("is", "Íslenska", "Icelandic", 25),

			// 苏格兰盖尔语
			Mk("gd", "Gàidhlig", "Scottish Gaelic", 10),

			// 威尔士语
			Mk("cy", "Cymraeg", "Welsh", 20),

			// 法罗语
			Mk("fo", "Føroyskt", "Faroese", 15),

			// 奥塞梯语
			Mk("os", "Ирон", "Ossetian", 20),

			// 巴什基尔语
			Mk("ba", "Башҡортса", "Bashkir", 25),

			// 鞑靼语
			Mk("tt", "Татарча", "Tatar", 30),

			// 车臣语
			Mk("ce", "Нохчийн", "Chechen", 25),

			// 阿瓦尔语
			Mk("av", "Авар", "Avar", 20),

			// 列兹金语
			Mk("lez", "Лезги", "Lezghian", 20),

			// 库梅克语
			Mk("kum", "Къумукъ", "Kumyk", 15),

			// 卡拉恰伊-巴尔卡尔语
			Mk("krc", "Къарачай-Малкъар", "Karachay-Balkar", 15),

			// 阿布哈兹语
			Mk("ab", "Аҧсуа", "Abkhaz", 15),

			// 科米语
			Mk("kv", "Коми", "Komi", 15),

			// 乌德穆尔特语
			Mk("udm", "Удмурт", "Udmurt", 15),

			// 莫克沙语
			Mk("mdf", "Мокшень", "Moksha", 12),

			// 埃尔齐亚语
			Mk("myv", "Эрзянь", "Erzya", 12),

			// 瓜拉尼语
			Mk("gn", "Avañe'ẽ", "Guarani", 25),

			// 艾马拉语
			Mk("ay", "Aymar aru", "Aymara", 20),

			// 克丘亚语
			Mk("qu", "Runa Simi", "Quechua", 30),

			// 切罗基语
			Mk("chr", "ᏣᎳᎩ", "Cherokee", 8),

			// 纳瓦霍语
			Mk("nv", "Diné bizaad", "Navajo", 10),

			// 莫霍克语
			Mk("moh", "Kanien’kéha", "Mohawk", 5),

			// 克里语
			Mk("cr", "ᓀᐦᐃᔭᐍᐏᐣ", "Cree", 8),

			// 因纽特语
			Mk("iu", "ᐃᓄᒃᑎᑐᑦ", "Inuktitut", 6),

			// 阿姆哈拉语
			Mk("am", "አማርኛ", "Amharic", 50),

			// 白俄罗斯语
			Mk("be", "Беларуская", "Belarusian", 35),

			// 格鲁吉亚语
			Mk("ka", "ქართული", "Georgian", 32),

			// 亚美尼亚语
			Mk("hy", "Հայերեն", "Armenian", 32),

			// 阿塞拜疆语
			Mk("az", "Azərbaycanca", "Azerbaijani", 45),

			// 巴斯克语
			Mk("eu", "Euskara", "Basque", 28),

			// 加泰罗尼亚语
			Mk("ca", "Català", "Catalan", 45),

			// 加利西亚语
			Mk("gl", "Galego", "Galician", 38),

			// 南非荷兰语
			Mk("af", "Afrikaans", "Afrikaans", 45),

			// 斯洛伐克语
			Mk("sk", "Slovenčina", "Slovak", 45),

			// 乌兹别克语
			Mk("uz", "Oʻzbekcha", "Uzbek", 48),

			// 藏语
			Mk("bo", "བོད་སྐད་", "Tibetan", 20),

			// 宗喀语
			Mk("dz", "རྫོང་ཁ", "Dzongkha", 12),

			// 拉丁语
			Mk("la", "Latina", "Latin", 15),

			// 世界语
			Mk("eo", "Esperanto", "Esperanto", 10),
		};
	}
}
