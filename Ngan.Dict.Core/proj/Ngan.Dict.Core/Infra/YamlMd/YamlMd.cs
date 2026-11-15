// 從 Tsinswreng.CsYamlMd（0.0.1-alpha）原樣搬入本專案：YamlMd = markdown 裏嵌 YAML 的寫法，
// 供 LLM 詞典響應的解析使用。改為項目內置而非外部專案引用，是因為該庫尚未遷入 TswgLib，
// 而本項目只用到 ToYaml / TryToYamlLenient 兩個入口；行為保持原樣，測試仍以原庫用例為準。
namespace Ngan.Dict.Core.Infra.YamlMd;
using Tsinswreng.CsCore;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using System.Text;

public interface IYamlMd{
	[Doc(@$"")]
	public string ToYaml(string YamlMd);
}

/// YamlMd 是一種 markdown 與 yaml 結合的寫法，便於在 yaml 中結合無需額外縮進與轉義的多行文本塊。
public class YamlMd: IYamlMd{
	public static YamlMd Inst=>field??=new YamlMd();
	/// 將 YamlMd 格式的字符串轉換為標準 YAML 格式
	/// <param name="yamlMd">YamlMd 格式的字符串</param>
	/// <returns>標準 YAML 格式的字符串</returns>
	public string ToYaml(string yamlMd){
		var pipeline = new MarkdownPipelineBuilder().Build();
		var document = Markdown.Parse(yamlMd, pipeline);

		var topYaml = string.Empty;
		var anchors = new List<AnchorContent>();

		var children = document.ToList();
		var i = 0;

		// 1. 提取頂層的 yaml 代碼塊
		while(i < children.Count){
			var block = children[i];
			if(block is FencedCodeBlock codeBlock && codeBlock.Info == "yaml"){
				topYaml = codeBlock.Lines.ToString();
				i++;
				break;
			}
			i++;
		}

		// 2. 提取一級標題及其後的第一個代碼塊
		while(i < children.Count){
			var block = children[i];

			// 檢查是否是一級標題
			if(block is HeadingBlock heading && heading.Level == 1){
				// 提取標題文本
				var headingText = ExtractHeadingText(heading);

				// 查找該標題後的第一個代碼塊，直到遇到下一個一級標題或結束
				string? codeBlockValue = null;
				var foundCodeBlock = false;

				for(var j = i + 1; j < children.Count; j++){
					var nextBlock = children[j];

					// 遇到下一個一級標題就停止
					if(nextBlock is HeadingBlock nextHeading && nextHeading.Level == 1){
						break;
					}

					// 找到第一個代碼塊
					if(nextBlock is FencedCodeBlock fencedCode && !foundCodeBlock){
						foundCodeBlock = true;
						var content = fencedCode.Lines.ToString();
						// 判斷是否為空內容
						codeBlockValue = string.IsNullOrEmpty(content) ? "" : content;
						break;
					}
				}

				// 如果沒找到代碼塊，設為 null
				if(!foundCodeBlock){
					codeBlockValue = null;
				}

				anchors.Add(new AnchorContent(headingText, codeBlockValue));
			}

			i++;
		}

		// 3. 構建最終的 yaml
		var anchorDefinitions = new List<string>();

		foreach(var anchor in anchors){
			if(anchor.Value == null){
				anchorDefinitions.Add($"{anchor.Name}: &{anchor.Name} null");
			}else if(anchor.Value == ""){
				anchorDefinitions.Add($"{anchor.Name}: &{anchor.Name} \"\"");
			}else{
				// 使用 |+ 保留尾部換行符
				var indentedValue = anchor.Value
					.Split('\n')
					.Select(line => "  " + line)
					.Aggregate((a, b) => a + "\n" + b);
				anchorDefinitions.Add($"{anchor.Name}: &{anchor.Name} |+\n{indentedValue}");
			}
		}

		// 拼接結果：錨點定義 + 空行 + 原始 yaml
		var result = string.Join("\n", anchorDefinitions) + "\n\n" + topYaml;

		return result;
	}

	
	/// 從標題塊中提取純文本
	
	private string ExtractHeadingText(HeadingBlock heading){
		var text = new StringBuilder();

		if(heading.Inline != null){
			foreach(var inline in heading.Inline){
				if(inline is LiteralInline literal){
					text.Append(literal.Content.ToString());
				}
			}
		}

		return text.ToString();
	}

	private class AnchorContent{
		public string Name { get; }
		public string? Value { get; }

		public AnchorContent(string name, string? value){
			Name = name;
			Value = value;
		}
	}
}


public enum ELenientParseResult{
	[Doc(@$"Normal YamlMd format")]
	Normal = 0,
	[Doc(@$"parse failed, cannot reconise")]
	Failed = -1,
	[Doc(@$"
````md
```yaml
key: *__value
```

# __value
```
this is value block
```
````
	")]
	WrappedByExtraTextBlock,
	[Doc(@$"
````md
```yaml
key: *__value
```

# __value
```
this is value block
```
````

ConclusionOrOtherAdditionalText:
blabla...
	")]
	ExtraTextBlockWithConclusion,
}

public static class ExtnYamlMd{
	extension(IYamlMd z){
		[Doc(@$"parse YamlMd leniently, usually from LLM output
		#Params([yamlMd str that may not be valid], [out str parsedYaml])
		")]
		public ELenientParseResult TryToYamlLenient(
			str YamlMd, out str R
		){
			R = "";
			if(string.IsNullOrWhiteSpace(YamlMd)){
				return ELenientParseResult.Failed;
			}

			// step 1: strict path. If current text already looks like YamlMd, keep normal result.
			if(TryParseCandidate(z, YamlMd, out R)){
				if(HasAdditionalConclusionText(YamlMd)){
					return ELenientParseResult.ExtraTextBlockWithConclusion;
				}
				return ELenientParseResult.Normal;
			}

			// step 2: common LLM output: wrap the whole YamlMd in an extra markdown code block.
			var wrappedBlock = MdTextBlock.GetTextBlock(YamlMd);
			if(wrappedBlock is not null && TryParseCandidate(z, wrappedBlock.Text, out R)){
				return ELenientParseResult.WrappedByExtraTextBlock;
			}

			// step 3: tolerate additional explanation text before/after a wrapped block.
			var pipeline = new MarkdownPipelineBuilder().Build();
			var document = Markdown.Parse(YamlMd, pipeline);
			foreach(var block in document){
				if(block is not FencedCodeBlock fencedCode){
					continue;
				}
				var innerText = fencedCode.Lines.ToString();
				if(!TryParseCandidate(z, innerText, out R)){
					continue;
				}
				var hasSuffixText = HasNonWhitespaceSuffix(YamlMd, fencedCode.Span.End + 1);
				if(hasSuffixText){
					return ELenientParseResult.ExtraTextBlockWithConclusion;
				}
				return ELenientParseResult.WrappedByExtraTextBlock;
			}

			R = "";
			return ELenientParseResult.Failed;
		}
	}

	/// Try strict parsing from one candidate text. Candidate is accepted only if it contains a yaml fenced block.
	/// <param name="YamlMdParser">YamlMd parser implementation.</param>
	/// <param name="Candidate">Candidate text that may contain YamlMd.</param>
	/// <param name="R">Parsed YAML output.</param>
	/// <returns>true when candidate can be parsed as YamlMd; otherwise false.</returns>
	private static bool TryParseCandidate(IYamlMd YamlMdParser, str Candidate, out str R){
		R = "";
		if(!ContainsTopYamlBlock(Candidate)){
			return false;
		}
		R = YamlMdParser.ToYaml(Candidate);
		return true;
	}

	/// Check whether text contains at least one fenced code block with yaml info.
	/// <param name="Text">Input markdown text.</param>
	/// <returns>true when a yaml fenced block exists; otherwise false.</returns>
	private static bool ContainsTopYamlBlock(str Text){
		var pipeline = new MarkdownPipelineBuilder().Build();
		var document = Markdown.Parse(Text, pipeline);
		foreach(var block in document){
			if(block is not FencedCodeBlock fencedCode){
				continue;
			}
			if(string.Equals(fencedCode.Info?.Trim(), "yaml", StringComparison.OrdinalIgnoreCase)){
				return true;
			}
		}
		return false;
	}

	/// Determine whether there is non-whitespace tail text after a specified index.
	/// <param name="Text">Original full text.</param>
	/// <param name="StartIndex">First index of tail segment.</param>
	/// <returns>true when trailing text contains visible characters; otherwise false.</returns>
	private static bool HasNonWhitespaceSuffix(str Text, i32 StartIndex){
		var normalizedStart = Math.Clamp(StartIndex, 0, Text.Length);
		return !string.IsNullOrWhiteSpace(Text[normalizedStart..]);
	}

	/// Detect additional non-structural markdown blocks after entering a YamlMd document.
	/// Structural blocks are level-1 headings and fenced code blocks.
	/// <param name="Text">Original YamlMd candidate text.</param>
	/// <returns>true when extra explanatory text exists; otherwise false.</returns>
	private static bool HasAdditionalConclusionText(str Text){
		var pipeline = new MarkdownPipelineBuilder().Build();
		var document = Markdown.Parse(Text, pipeline);
		var enteredYamlMd = false;
		foreach(var block in document){
			if(!enteredYamlMd){
				if(block is FencedCodeBlock fencedCode
					&& string.Equals(fencedCode.Info?.Trim(), "yaml", StringComparison.OrdinalIgnoreCase)){
					enteredYamlMd = true;
				}
				continue;
			}
			if(block is HeadingBlock heading && heading.Level == 1){
				continue;
			}
			if(block is FencedCodeBlock){
				continue;
			}
			var start = Math.Clamp(block.Span.Start, 0, Text.Length);
			var endExclusive = Math.Clamp(block.Span.End + 1, 0, Text.Length);
			if(endExclusive <= start){
				continue;
			}
			var rawBlockText = Text[start..endExclusive];
			if(!string.IsNullOrWhiteSpace(rawBlockText)){
				return true;
			}
		}
		return false;
	}
}
