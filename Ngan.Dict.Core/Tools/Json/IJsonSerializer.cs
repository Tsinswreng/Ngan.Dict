using System.Text.Json;

namespace Ngaq.Core.Tools.Json;

public interface IJsonSerializer{
	public str Stringify<T>(T O);
	[Doc(@$"{nameof(JsonSerializer.Deserialize)} 返回 `T?`")]
	public T? Parse<T>(str Json);
	public obj? Parse(str Json, Type Type);
}

