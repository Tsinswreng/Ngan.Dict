using Tsinswreng.CsRefl;
namespace Ngan.Dict.Core.Infra;


[Doc(@$"本應用的型別元資料來源（取代舊 Tsinswreng.Srefl 之 CoreDictMapper）。

	來源組合：先問 {nameof(JsonTypeInfoSrc)}（掛過 [JsonSerializable] 的型別走 STJ 源生成、AOT 可用），
	未註冊者落到 {nameof(ReflTypeInfoSrc)} 兜底。

	用處：按名讀寫實例（{nameof(ITypeInfoSrcExtn.TryGet)}／{nameof(ITypeInfoSrcExtn.TrySet)}）、
	物件與淺字典互轉（{nameof(ITypeInfoSrcExtn.ToInstDict)}）、
	以及 CsSql 建表與倉儲所需的成員元資料。
")]
public static class AppTypeInfoSrc{
	private static ITypeInfoSrc? _Inst = null;

	[Doc(@$"全域唯一實例；第一次取用時建。
		兩條來源各自內部有緩存，故本實例建一次就夠。
	")]
	public static ITypeInfoSrc Inst => _Inst??= new MergedTypeInfoSrc(
		new JsonTypeInfoSrc(AppJsonCtx.Default)
		,new ReflTypeInfoSrc()
	);
}

public static class ExtnIDict{
	public static str Print<K,V>(
		IDictionary<K,V> z
	){
		List<str> res = new List<str>();
		foreach(var kvp in z){
			res.Add(kvp.Key + ":" + kvp.Value + "\n");
		}
		return str.Join("", res);
	}
}
