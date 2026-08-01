using Ngaq.Core.Infra.IF;
using Tsinswreng.CsIfaceGen;
using Tsinswreng.Srefl;
namespace Ngaq.Core.Infra;

public class CfgIfaceGen{
	public const str OutDir = nameof(Tsinswreng)+"."+nameof(Tsinswreng.CsIfaceGen)+"/";
}


[IfaceGen(
	ParentType = typeof(IAppSerializable)
	,Name = nameof(CoreDictMapper)
	,PhFullType = "TYPE"
	,PhIdentifierSafeFullType = "ID"
	,OutDir = CfgIfaceGen.OutDir+nameof(CoreDictMapper)
	,Template =
"""

namespace Ngaq.Core.Infra{
using Tsinswreng.Srefl;
	[SreflType(typeof(TYPE))]
	public partial class CoreDictMapper{
		//public static string ID = "TYPE";
	}
}

"""
)]
public interface IIfaceGenCfg_CoreDictMapper{

}

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
Ngaq.Core/GenAppJsonCtx.sh 中 引用了此類的名稱
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
