namespace Ngaq.Core.Shared.Word.Models.Learn_;

using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Infra;
using Tsinswreng.Srefl;



public partial interface IProp:IPoKv{

}

public partial class Prop
	:PoWordProp
	,IProp
{

}

public static class ExtnIProp{
	public static IProp ToIProp(
		this IPoKv z
	){
		var Mgr = CoreDictMapper.Inst.PropAccessorReg;
		if(!Mgr.Type_PropAccessor.TryGetValue(z.GetType(), out var SrcAccessor)){
			throw new Exception($"No {nameof(IPropAccessor)} registered for type: {z.GetType()}");
		}
		if(!Mgr.Type_PropAccessor.TryGetValue(typeof(Prop), out var TarAccessor)){
			throw new Exception($"No {nameof(IPropAccessor)} registered for type: {typeof(Prop)}");
		}
		var R = new Prop();
		foreach(var Key in SrcAccessor.GetGetterNames(z)){
			if(!SrcAccessor.TryGet(z, Key, out var V)){
				continue;
			}
			TarAccessor.TrySet(R, Key, V);
		}

		return R;
	}
}

