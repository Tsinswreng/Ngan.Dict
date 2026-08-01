namespace Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Infra.IF;

//分更好
enum TestKvType2{
	Str_Str=1
	,Str_I64=2
	,Str_F64=3
	,I64_Str=4
	,I64_I64=5
	,I64_F64=6
	,F64_Str=7
	,F64_I64=8
	,F64_F64=9
}

public enum EKvType{
	Str = 1
	,I64 = 2
	,F64 = 3
	,Binary = 4

}

public partial interface IPoKv:IAppSerializable{

	#region IPoKv
	public EKvType KType { get; set; }
	public str? KStr { get; set; }
	
	/// KType非I64旹、忽略KI64。用匪空類型可免裝箱
	
	public i64 KI64 { get; set; }

	public EKvType VType { get; set; }
	public str? VStr { get; set; }
	public i64 VI64 { get; set; }
	public f64 VF64 { get; set; }
	public u8[]? VBinary {get;set;}
	#endregion IPoKv
}
