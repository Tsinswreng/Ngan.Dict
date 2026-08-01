namespace Ngaq.Core.Shared.Word.Models.Po.Kv;

public  partial class ConstTokens{
	protected static ConstTokens? _Inst = null;
	public static ConstTokens Inst => _Inst??= new ConstTokens();

	
	/// 命名空間與名 之分隔符
	/// 適用于Const_PropKey及Const_Learn等
	/// 內置鍵名等皆在頂級命名空間下 如":annotation", ":tag"等 勿省`:`
	/// 用戶自定義鍵名則在用戶命名空間下 如"Tsinswreng:Annotation"
	
	public str SepNamespaceEtName = ":";
	public str Concat(str? ns, str name){
		ns = ns??"";
		if(ns == ""){
			return name;
		}
		return ns + SepNamespaceEtName + name;
	}

}

public static class ExtnIPoKv{
	public static IPoKv SetStrToken(
		this IPoKv z
		,str? ns
		,str name
		,str value
	){
		var T = ConstTokens.Inst;
		var key = T.Concat(ns, name);
		z.SetStrStr(key, value);
		return z;
	}

	// public static str GetStrToken(
	// 	this IPoKv z
	// 	,str? ns
	// 	,str name
	// ){
	// 	var T = ConstTokens.Inst;
	// 	var key = T.Concat(ns, name);
	// 	//return z.GetStr(key);
	// }
}


// public  partial class Const_Tokens{
// 	
// 	/// 命名空間與名 之分隔符
// 	/// 適用于Const_PropKey及Const_Learn等
// 	/// 內置鍵名等皆在頂級命名空間下 如":annotation", ":tag"等
// 	/// 用戶自定義鍵名則在用戶命名空間下 如"Tsinswreng:Annotation"
// 	
// 	public const str Sep_NamespaceEtName = ":";
// 	public static str Concat(str? ns, str name){
// 		ns = ns??"";
// 		return ns + Sep_NamespaceEtName + name;
// 	}
// }
