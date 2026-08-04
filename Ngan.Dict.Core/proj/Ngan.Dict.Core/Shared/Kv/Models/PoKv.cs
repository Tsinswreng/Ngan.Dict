namespace Ngan.Dict.Core.Shared.Kv.Models;

using Ngan.Dict.Core.Shared.Base.Models.Po;
using Ngan.Dict.Core.Shared.User.Models.Po.User;
using Ngan.Dict.Core.Shared.Word.Models.Po.Kv;
using Ngan.Dict.Core.Infra;
using Ngan.Dict.Core.Infra.IF;

//[MkFactory(For=typeof(PoKv))]
public partial class PoKv
	:PoBase
	,AppI_Id<IdKv>
	,IPoKv
{

	[Impl(typeof(AppI_Id<IdKv>))]
	public IdKv Id{get;set;} = new();

	public IdUser Owner{get;set;} = IdUser.Zero;

	#region IPoKv
	[Impl(typeof(IPoKv))]
	public EKvType KType { get; set; } = EKvType.Str;
	[Impl(typeof(IPoKv))]
	public str? KStr { get; set; } = null;
	/// KType非I64旹、忽略KI64。用匪空類型可免裝箱
	[Impl(typeof(IPoKv))]
	public i64 KI64 { get; set; } = 0;

	[Impl(typeof(IPoKv))]
	public EKvType VType { get; set; } = EKvType.Str;

	[Impl(typeof(IPoKv))]
	public str? VStr { get; set; } = null;
	[Impl(typeof(IPoKv))]
	public i64 VI64 { get; set; } = 0;
	[Impl(typeof(IPoKv))]
	public f64 VF64 { get; set; } = 0.0;
	public u8[]? VBinary {get;set;} = null;
	#endregion IPoKv
}
