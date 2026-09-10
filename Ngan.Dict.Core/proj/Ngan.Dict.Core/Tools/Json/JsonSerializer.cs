using Tsinswreng.CsTools;
using System.Text.Json;
using Ngan.Dict.Core.Infra;

namespace Ngan.Dict.Core.Tools.Json;

/// 舊 IJsonSerializer（Stringify/Parse）已退役，由公共庫 Tsinswreng.CsJson.IJsonSerializer 取代。
/// 本類僅保留 IDictJsonSerializer 職責（DictJson ↔ 業務對象互轉）。
public class AppJsonSerializer
	:IDictJsonSerializer
{
	public static AppJsonSerializer Inst => field??=new();
	
	public obj? ToDictJson<T>(T O){
		var jsonElement = JSON.ToElement(O);
		if(jsonElement.ValueKind != JsonValueKind.Object){
			throw new NotSupportedException("ToDictJson only supports object root.");
		}
		return ToolJson.ParseJsonElement(jsonElement);
	}
	public obj? FromDictJson(obj? DictJson, Type Type){
		if(DictJson is null){
			return null;
		}
		if(DictJson is IDictionary<str, obj?> dict){
			var jsonElement = JsonSerializer.SerializeToElement(dict, AppJsonCtx.Inst.IDictionaryStringObject);
			return JSON.Parse(jsonElement, Type);
		}
		if(DictJson is IList<obj?> list){
			var listForSer = list as List<obj?> ?? [..list];
			var jsonElement = JsonSerializer.SerializeToElement(listForSer, AppJsonCtx.Inst.IListObject);
			return JSON.Parse(jsonElement, Type);
		}
		throw new NotSupportedException("DictJson should be a dictionary or a list.");
	}
	public T? FromDictJson<T>(obj? DictJson){
		return (T?)this.FromDictJson(DictJson, typeof(T));
	}
	
}