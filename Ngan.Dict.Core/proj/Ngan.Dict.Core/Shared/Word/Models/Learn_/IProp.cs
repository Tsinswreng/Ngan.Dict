namespace Ngan.Dict.Core.Shared.Word.Models.Learn_;

using Ngan.Dict.Core.Shared.Word.Models.Po.Kv;
using Ngan.Dict.Core.Infra;
using Tsinswreng.CsRefl;



public partial interface IProp:IPoKv{

}

public partial class Prop
	:PoWordProp
	,IProp
{

}

public static class ExtnIProp{
	[Doc(@$"把來源物件當下的可讀可寫成員逐個抄到新 Prop 上（按成員名對齊）。
		成員名用 {nameof(AppTypeInfoSrc)} 之元資料取得。
	")]
	public static IProp ToIProp(
		this IPoKv z
	){
		var Src = AppTypeInfoSrc.Inst;
		var TarInfo = Src.GetInfo(typeof(Prop));
		var R = new Prop();
		foreach(var (Name, V) in Src.ToInstViewDict(z)){
			if(!TarInfo.TryGetMember(Name, out var TarM)){
				continue;
			}
			TarM.TrySet(R, V);
		}

		return R;
	}
}

