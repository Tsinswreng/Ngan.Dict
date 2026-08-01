using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using Ngaq.Core.Infra;
using Ngaq.Core.Tools.Json;
namespace Ngaq.Core.Tools;


public static class JSON {
	
	/// 縱Opt中指定了TypeInfoResolver亦不效、肰直ᵈ傳TypeInfo又不可兼傳Opt。故需把選項加于appJsonCtxʹ註解
	// static JsonSerializerOptions Opt = new JsonSerializerOptions{
	// 	//WriteIndented = true
	// 	Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // 允許原樣輸出
	// 	,PropertyNamingPolicy = null  // 关闭命名策略
	// 	,TypeInfoResolver = AppJsonCtx.Default
	// 	,ReadCommentHandling = JsonCommentHandling.Skip
	// 	//,Converters = { new CustomJsonConvtrFctry() }
	// 	// ,Converters = {

	// 	// }
	// };

	public static string Stringify<T>(T o){
		//勿用 AppJsonCtx.Default
		var typeInfo = AppJsonCtx.Inst.GetTypeInfo(typeof(T)) as JsonTypeInfo<T>;
		if (typeInfo == null){
			throw new InvalidOperationException($"Type {typeof(T)} is not registered in AppJsonCtx");
		}
		return JsonSerializer.Serialize(o, typeInfo);
	}

	public static T? Parse<T>(string json){
		//勿用 AppJsonCtx.Default
		var typeInfo = AppJsonCtx.Inst.GetTypeInfo(typeof(T)) as JsonTypeInfo<T>;
		if (typeInfo == null){
			throw new InvalidOperationException($"Type {typeof(T)} is not registered in AppJsonCtx");
		}
		return JsonSerializer.Deserialize(json, typeInfo);
	}
	
	public static obj? Parse(string json, Type T){
		var typeInfo = AppJsonCtx.Inst.GetTypeInfo(T);
		if (typeInfo == null){
			throw new InvalidOperationException($"Type {T} is not registered in AppJsonCtx");
		}
		return JsonSerializer.Deserialize(json, typeInfo);
	}

	/// 使用源生成的 TypeInfo 把对象直接序列化为 JsonElement，避免中间 JSON 字符串。
	public static JsonElement ToElement<T>(T o){
		var typeInfo = AppJsonCtx.Inst.GetTypeInfo(typeof(T)) as JsonTypeInfo<T>;
		if (typeInfo == null){
			throw new InvalidOperationException($"Type {typeof(T)} is not registered in AppJsonCtx");
		}
		return JsonSerializer.SerializeToElement(o, typeInfo);
	}

	/// 使用源生成的 TypeInfo 从 JsonElement 反序列化到目标类型，避免中间 JSON 字符串。
	public static obj? Parse(JsonElement element, Type T){
		var typeInfo = AppJsonCtx.Inst.GetTypeInfo(T);
		if (typeInfo == null){
			throw new InvalidOperationException($"Type {T} is not registered in AppJsonCtx");
		}
		return JsonSerializer.Deserialize(element, typeInfo);
	}
	
	
}


