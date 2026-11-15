// 從 Tsinswreng.CsYamlMd（0.0.1-alpha）原樣搬入本專案：YamlMd = markdown 裏嵌 YAML 的寫法，
// 供 LLM 詞典響應的解析使用。改為項目內置而非外部專案引用，是因為該庫尚未遷入 TswgLib，
// 而本項目只用到 ToYaml / TryToYamlLenient 兩個入口；行為保持原樣，測試仍以原庫用例為準。
namespace Ngan.Dict.Core.Infra.YamlMd;

using System.Text.RegularExpressions;
using Tsinswreng.CsCore;

public partial class MdTextBlock{
	public str? Lang{get;set;} = null;
	public str Text{get;set;} = "";


[Doc($$"""
判斷入參Text Trim之後是不是Md文本塊
三個或三個以上的 成對的 反點 纔算 Md 文本塊
如果不是則返null
如果是則解析出內容和語言後返回 MdTextBlock 物件
#Examples([
入參
```
```

返回{Lang:"",Text:""}
],[
入參
abc
返回null
],[
入參
````cs
using System;
````
返回{Lang:"cs", Text:"using System;"}
])
""")]
	public static MdTextBlock? GetTextBlock(str TextBlock){
		// 第一步：预处理输入文本，去除首尾空白
		string trimmedText = TextBlock?.Trim() ?? string.Empty;
		if (string.IsNullOrEmpty(trimmedText))
		{
			return null;
		}

		// 第二步：正则匹配Markdown代码块（至少3个成对反引号）
		// 正则说明：
		// ^(`{3,})    - 开头捕获至少3个反引号（确定代码块边界长度）
		// (\w*)       - 捕获可选的语言标识（如cs、java等）
		// \s*         - 匹配语言后任意空白（包括换行）
		// (.*?)       - 非贪婪匹配代码块内容（支持换行）
		// \s*\1$      - 匹配结尾与开头数量一致的反引号（保证成对）
		var codeBlockRegex = MyRegex();

		// 第三步：执行匹配并校验结果
		var match = codeBlockRegex.Match(trimmedText);
		if (!match.Success)
		{
			return null;
		}

		// 第四步：提取并清洗语言和内容
		string lang = match.Groups[2].Value.Trim();
		string text = match.Groups[3].Value
			.TrimEnd('\r', '\n') // 移除内容末尾的换行符
			.Trim(); // 移除内容首尾空白

		// 第五步：返回解析后的对象（语言为空时设为null）
		return new MdTextBlock
		{
			Lang = string.IsNullOrEmpty(lang) ? null : lang,
			Text = text
		};
	}

	[GeneratedRegex(
		@"^(`{3,})(\w*)\s*(.*?)\s*\1$"
		, RegexOptions.Multiline | RegexOptions.Singleline
	)]
	private static partial System.Text.RegularExpressions.Regex MyRegex();
}

