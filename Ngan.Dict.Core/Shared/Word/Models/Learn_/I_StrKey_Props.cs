namespace Ngaq.Core.Shared.Word.Models.Learn_;

using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Tsinswreng.CsTools;



public partial interface I_StrKey_Props{
	public IDictionary<str, IList<IProp>> StrKey_Props{get;set;}
	#if Impl
	= new Dictionary<str, IList<IProp>>();
	#endif
}

public static class ExtnI_StrKey_Props{
	public static IDictionary<str, IList<IProp>> FromPoKvs(
		this IDictionary<str, IList<IProp>> z
		,IEnumerable<PoWordProp> PoKvs
	){
		var sortedKvs = PoKvs.OrderByDescending(x=>x.LatestBizTime).ToList();
		foreach(var PoKv in sortedKvs){
			if(PoKv.KType == EKvType.Str){
				var Prop = PoKv.ToIProp();
				z.AddInValues(
					PoKv.KStr??""
					,Prop
				);
			}
		}
		return z;
	}
}

