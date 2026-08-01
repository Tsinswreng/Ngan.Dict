namespace Ngaq.Core.Shared.Word.Models.Learn_;

using Ngaq.Core.Model.Po.Kv;



public enum ELearn{
	Add = 1
	,Rmb = 2
	,Fgt = 3

}

// public class ELearn{
// 	public const str Add = nameof(Add);
// 	public const str Rmb = nameof(Rmb);
// 	public const str Fgt = nameof(Fgt);
// }



#if false
public partial class OldELearn{
	protected static OldELearn? _Inst = null;
	public static OldELearn Inst => _Inst??= new OldELearn();

	public ELearn Add = ConstLearn.add;
	public ELearn Rmb = ConstLearn.rmb;
	public ELearn Fgt = ConstLearn.fgt;
}


#endif
