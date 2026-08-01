namespace Ngaq.Core.Tools.Json;


[Doc(@$"
DictJson is:
```ts
type T =
|i64
|f64
|str
|bool
|null
|IList<T> // IList<object?> in C#
|IDict<str, T> // IDict<string, object?> in C#
```
")]
public interface IDictJsonSerializer{
	[Doc(@$"
	#Rtn[DictJson]
	")]
	public obj? ToDictJson<T>(T O);
	public obj? FromDictJson(obj? DictJson, Type Type);
	public T? FromDictJson<T>(obj? DictJson);
}
