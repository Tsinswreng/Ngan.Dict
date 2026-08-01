namespace Ngaq.Core.Shared.Word.Models.Po.Kv;

public partial class ConstLearn{
	public const str add = nameof(add);
	public const str rmb = nameof(rmb);
	public const str fgt = nameof(fgt);
}



public partial class OldConstLearn{
	protected static OldConstLearn? _Inst = null;
	public static OldConstLearn Inst => _Inst??= new OldConstLearn();

	public str add = nameof(add);
	public str rmb = nameof(rmb);
	public str fgt = nameof(fgt);

}
