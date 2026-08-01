// namespace Ngaq.Core.Tools;

// using System.Text.Encodings.Web;
// using System.Text.Json;
// using System.Text.Json.Serialization;

// [JsonSerializable(typeof(Dictionary<string, object?>))]
// [JsonSerializable(typeof(IDictionary<string, object?>))]
// [JsonSerializable(typeof(List<object?>))]
// [JsonSerializable(typeof(IList<object?>))]
// [JsonSerializable(typeof(string))]
// [JsonSerializable(typeof(int))]
// [JsonSerializable(typeof(long))]
// [JsonSerializable(typeof(double))]
// [JsonSerializable(typeof(float))]
// [JsonSerializable(typeof(bool))]
// [JsonSerializable(typeof(decimal))]
// [JsonSerializable(typeof(DateTime))]
// [JsonSerializable(typeof(DateTimeOffset))]
// [JsonSerializable(typeof(Guid))]
// internal partial class DictJsonCtx : JsonSerializerContext
// {
// }


// public class DictJson{
// 	static JsonSerializerOptions Opt = new JsonSerializerOptions{
// 		//WriteIndented = true
// 		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // 允許原樣輸出
// 		,PropertyNamingPolicy = null  // 关闭命名策略
// 		,TypeInfoResolver = DictJsonCtx.Default
// 		,ReadCommentHandling = JsonCommentHandling.Skip
// 	};
// 	public static str ToJson(IDictionary<string, object?> data){
// 		//string json = JsonSerializer.Serialize(data, DictJsonCtx.Default.DictionaryStringObject); // 這樣不報錯、但是會把漢字轉成unicode碼
// 		string json = JsonSerializer.Serialize(data, Opt);
// 		return json;
// 	}

// 	// 反序列化：JSON 字符串 -> IDictionary<string,object?>
// 	// 此方法不能處理嵌套字典、對于嵌套字典、解析後其內仍潙JsonElement
// 	// public static IDictionary<str, obj?> FromJson(str json){
// 	// 	// 先用 JsonNode 解析，再反序列化到目标类型，全程不走反射
// 	// 	var node = JsonNode.Parse(json)!;
// 	// 	return node.Deserialize(DictJsonCtx.Default.IDictionaryStringObject)!;
// 	// }

// 	public static IDictionary<string, object?> FromJson(string json){
// 		using var doc = JsonDocument.Parse(json);
// 		return ParseJsonElement(doc.RootElement) as IDictionary<string, object?>
// 			?? throw new InvalidOperationException("JSON root is not an object.");
// 	}

// 	private static object? ParseJsonElement(JsonElement element)
// 	{
// 		switch (element.ValueKind)
// 		{
// 			case JsonValueKind.Object:
// 				var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
// 				foreach (var prop in element.EnumerateObject())
// 				{
// 					dict[prop.Name] = ParseJsonElement(prop.Value);
// 				}
// 				return dict;

// 			case JsonValueKind.Array:
// 				var list = new List<object?>();
// 				foreach (var item in element.EnumerateArray())
// 				{
// 					list.Add(ParseJsonElement(item));
// 				}
// 				return list;

// 			case JsonValueKind.String:
// 				return element.GetString();

// 			case JsonValueKind.Number:
// 				if (element.TryGetInt64(out long l))
// 					return l;
// 				if (element.TryGetDouble(out double d))
// 					return d;
// 				return element.GetDecimal();

// 			case JsonValueKind.True:
// 				return true;

// 			case JsonValueKind.False:
// 				return false;

// 			case JsonValueKind.Null:
// 				return null;

// 			default:
// 				return null;
// 		}
// 	}

// }
