using Tsinswreng.CsTools;
using System.Text.Json;
using Ngaq.Core.Infra;

namespace Ngaq.Core.Tools.Json;

public class AppJsonSerializer
	:IJsonSerializer
	,IDictJsonSerializer
{
	public static AppJsonSerializer Inst => field??=new();
	[Impl]
	public str Stringify<T>(T O){
		return JSON.Stringify(O);
	}

	[Impl]
	public T? Parse<T>(str Json){
		return JSON.Parse<T>(Json);
	}

	[Impl]
	public obj? Parse(str Json, Type Type){
		return JSON.Parse(Json, Type);
	}
	
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
