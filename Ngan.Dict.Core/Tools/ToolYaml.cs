namespace Ngaq.Core.Tools;

using System.Collections.Generic;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

public class ToolYaml{
	/// 將YAML字符串解析爲字典。此實現兼容AOT，使用YamlDotNet的低級Parser API。
	/// 支持YAML錨點(&)和別名(*)。
	public static IDictionary<string, object?> YamlStrToDict(string YamlStr){
		var parser = new Parser(new StringReader(YamlStr));
		var anchors = new Dictionary<AnchorName, object?>();

		// 跳過 StreamStart 和 DocumentStart 事件
		while(parser.MoveNext()){
			if(parser.Current is StreamStart || parser.Current is DocumentStart){
				continue;
			}
			break;
		}

		var result = ParseValueFromCurrent(parser, anchors);

		if(result is IDictionary<string, object?> dict){
			return dict;
		}
		return new Dictionary<string, object?>();
	}

	
	/// 從當前parser位置解析值（不移動parser）
	private static object? ParseValueFromCurrent(Parser parser, Dictionary<AnchorName, object?> anchors){
		var current = parser.Current;

		switch(current){
			case Scalar scalar:
				return ParseScalar(scalar, anchors);

			case SequenceStart:
				return ParseSequence(parser, anchors);

			case MappingStart:
				return ParseMapping(parser, anchors);

			case AnchorAlias alias:
				return anchors.TryGetValue(alias.Value, out var anchoredValue)
					? anchoredValue
					: null;

			default:
				return null;
		}
	}

	
	/// 移動parser並解析下一個值
	private static object? ParseValue(Parser parser, Dictionary<AnchorName, object?> anchors){
		if(!parser.MoveNext()){
			return null;
		}

		return ParseValueFromCurrent(parser, anchors);
	}

	private static object? ParseScalar(Scalar scalar, Dictionary<AnchorName, object?> anchors){
		var value = scalar.Value;

		// 處理錨點
		if(!scalar.Anchor.IsEmpty){
			anchors[scalar.Anchor] = value;
		}

		// 處理特殊值
		if(scalar.IsPlainImplicit || scalar.Style == ScalarStyle.Plain){
			// 嘗試解析爲null
			if(value == "null" || value == "~" || string.IsNullOrEmpty(value)){
				if(!scalar.Anchor.IsEmpty){
					anchors[scalar.Anchor] = null;
				}
				return null;
			}

			// 嘗試解析爲布爾值
			if(value == "true"){
				return true;
			}
			if(value == "false"){
				return false;
			}

			// 嘗試解析爲數字
			if(long.TryParse(value, out var longValue)){
				return longValue;
			}
			if(double.TryParse(value, out var doubleValue)){
				return doubleValue;
			}
		}

		return value;
	}

	private static List<object?> ParseSequence(Parser parser, Dictionary<AnchorName, object?> anchors){
		var list = new List<object?>();
		var sequenceStart = parser.Current as SequenceStart;
		var anchor = sequenceStart?.Anchor ?? AnchorName.Empty;

		while(parser.MoveNext()){
			if(parser.Current is SequenceEnd){
				break;
			}
			list.Add(ParseValueFromCurrent(parser, anchors));
		}

		// 註冊錨點
		if(!anchor.IsEmpty){
			anchors[anchor] = list;
		}

		return list;
	}

	private static Dictionary<string, object?> ParseMapping(Parser parser, Dictionary<AnchorName, object?> anchors){
		var dict = new Dictionary<string, object?>();
		var mappingStart = parser.Current as MappingStart;
		var anchor = mappingStart?.Anchor ?? AnchorName.Empty;

		while(parser.MoveNext()){
			if(parser.Current is MappingEnd){
				break;
			}

			// 解析鍵
			var keyScalar = parser.Current as Scalar;
			if(keyScalar == null){
				// 跳過非標量鍵（複合鍵等）
				continue;
			}
			var key = keyScalar.Value;

			// 解析值
			var value = ParseValue(parser, anchors);
			dict[key] = value;
		}

		// 註冊錨點
		if(!anchor.IsEmpty){
			anchors[anchor] = dict;
		}

		return dict;
	}
}

