using Ngan.Dict.Core.Infra.IF;
using Tsinswreng.Srefl;
namespace Ngan.Dict.Core.Infra;


// IfaceGen 已移除：CoreDictMapper 的 [SreflType] 註冊改由 Ngan.Dict.Scripts 的 GenAppJsonCtx 生成
#region IfaceGen 舊配置（僅留檔）
// [IfaceGen(
// 	ParentType = typeof(IAppSerializable)
// 	,Name = nameof(CoreDictMapper)
// 	,PhFullType = "TYPE"
// 	,PhIdentifierSafeFullType = "ID"
// 	,OutDir = CfgIfaceGen.OutDir+nameof(CoreDictMapper)
// 	,Template =
// """
//
// namespace Ngan.Dict.Core.Infra{
// using Tsinswreng.Srefl;
// 	[SreflType(typeof(TYPE))]
// 	public partial class CoreDictMapper{
// 		//public static string ID = "TYPE";
// 	}
// }
//
// """
// )]
// public interface IIfaceGenCfg_CoreDictMapper{
//
// }
#endregion

#if false
[DictType(typeof(PoCfg))]
[DictType(typeof(IPoKv))]
[DictType(typeof(PoWordProp))]
[DictType(typeof(PoWord))]
[DictType(typeof(PoWordLearn))]
[DictType(typeof(Prop))]
[DictType(typeof(JnWord))]
#endif

//[DictType(typeof(AppCfg))] //TargetType須惟一


[Doc(@$"
Ngan.Dict.Scripts 的 GenAppJsonCtx 生成之代碼中 引用了此類的名稱
故謹慎重命名、留作CoreDictMapper
")]
public partial class CoreDictMapper{
protected static CoreDictMapper? _Inst = null;
public static CoreDictMapper Inst => _Inst??= new CoreDictMapper();


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
