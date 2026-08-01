namespace Ngaq.Core.Infra.Errors;
[Obsolete]
public partial class EnumErr{
	public static OldAppErr Mk(str Id, str? Ns = null){
		var R = new OldAppErr{
			Key = Id
			,Namespace = Ns
		};
		ErrMgr.Inst.Register(Id,Ns);
		return R;
	}
}


