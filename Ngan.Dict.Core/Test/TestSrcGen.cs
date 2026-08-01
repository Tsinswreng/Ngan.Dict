// using Ngaq.Core.Model.Po;
// using Ngaq.Core.Model.Po.Kv;
// using Ngaq.Core.Model.Po.Word;
// using Tsinswreng.CsDictMapper.Dict;
// using Tsinswreng.CsDictMapper.Dict.Attributes;
// using Tsinswreng.CsDictMapper.Dict.CodeGenerator.Ctx;


// namespace Ngaq.Core.Test;


// public  partial class ClassA{
// 	public string String{get;set;} = "String";
// 	public int Int{get;set;} = 1;
// 	public bool Bool{get;set;} = true;
// }
// public  partial class ClassB{

// }

// [DictType(typeof(ClassA))]
// [DictType(typeof(ClassB))]
// [DictType(typeof(Po_Kv))]
// [DictType(typeof(Po_Word))]
// public partial class DictCtx{

// }


// public  partial class Test{

// 	public static void NoGeneric(){
// 		var kv = new Po_Kv();
// 		var word = new Po_Word();
// 		var ctx = new DictCtx();
// 		var dict = DictCtx.ToDict(kv);
// 		var KvDict = DictCtx.ToDict(kv);
// 		System.Console.WriteLine(
// 			KvDict[nameof(Po_Kv.CreatedAt)]
// 		);
// 		var WordDict = DictCtx.ToDict(word);
// 		System.Console.WriteLine(
// 			WordDict[nameof(Po_Word.CreatedAt)]
// 		);

// 		WordDict[nameof(Po_Word.Lang)] = "English";
// 		DictCtx.Assign(word, WordDict);
// 		System.Console.WriteLine(
// 			word.Lang
// 		);
// 	}

// 	public static void Generic<T>(T A){
// 		var ADict = DictCtx.ToDict(A);
// 		ADict["CreatedAt"] = -1L;
// 		DictCtx.Assign(A, ADict);
// 	}

// 	public static void TestGeneric(){
// 		var word = new Po_Word();
// 		Generic(word);
// 		System.Console.WriteLine(
// 			word.CreatedAt
// 		);
// 	}

// }

// // [Hello]
// // public partial class A{
// // 	static void Test(A o){
// // 		System.Console.WriteLine(
// // 			o.Hello//A.Hello屬性是源生成器生成的
// // 		);
// // 	}
// // }


// // public partial class DictCtx_{
// // 	public static class TypedFnSaver<T>{
// // 		public static Func<T, Dictionary<string, object>> Fn_ToDict;
// // 		public static Func<T, Dictionary<string, object>, T> Fn_Assign;

// // #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
// // 		static TypedFnSaver(){
// // 			// 源生成器在此为每个类型生成类似如下代码：
// // 			if(false){
// // 				//System.Console.WriteLine();
// // 			}
// // 			else if (typeof(T) == typeof(ClassA)){
// // 				Fn_ToDict = (obj) => {
// // 					return ToDict((ClassA)(object)obj!);
// // 				};
// // 				Fn_Assign = (obj, dict) => {
// // 					var t = (ClassA)(object)obj!;
// // 					Assign(t, dict);
// // 					return obj;
// // 				};
// // 			}
// // 			else if(typeof(T) == typeof(ClassB)){
// // 				//...
// // 			}
// // 		}
// // 	}
// // }

// // public partial class DictCtx_{
// // 	public static Dictionary<string, object> ToDict<T>(T obj){
// // 		var fn = TypedFnSaver<T>.Fn_ToDict;
// // 		return fn(obj);
// // 	}

// // 	public static T Assign<T>(T obj, Dictionary<string, object> dict){
// // 		var fn = TypedFnSaver<T>.Fn_Assign;
// // 		return fn(obj, dict);
// // 	}

// // 	public static Dictionary<string, object> ToDict(ClassA obj){
// // 		return new Dictionary<string, object>{
// // ["String"] = obj.String,
// // ["Int"] = obj.Int,
// // ["Bool"] = obj.Bool,
// // 		};
// // 	}
// // 	public static ClassA Assign(ClassA o, Dictionary<string, object> d){
// // o.String = (string)d["String"];
// // o.Int = (int)d["Int"];
// // o.Bool = (bool)d["Bool"];
// // 		return o;
// // 	}



// // 	public static void Update<T>(T Old, T Neo){
// // 		var NeoDict = DictCtx_.ToDict(Neo);
// // 		DictCtx_.Assign(Old, NeoDict);
// // 	}

// // 	static void Test(ClassA Old, ClassA Neo){
// // 		Update<ClassA>(Old,Neo);
// // 	}

// // }
