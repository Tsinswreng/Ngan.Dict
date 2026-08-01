#define Impl
namespace Ngaq.Core.Shared.Word.Models.Po.Kv;

using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Infra;
using Ngaq.Core.Model.Po.Kv;
using Ngaq.Core.Models.Po;
using Ngaq.Core.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Models.Po.Word;


//TODO 分組㕥辨一詞多義? group, groupId, groupCnt
public partial class PoWordProp
	:PoBaseBizTime
	,AppI_Id<IdWordProp>
	,IPoKv
	,I_WordId
{
	public IdWordProp Id { get; set; } = new IdWordProp();

	public IdWord WordId{get;set;}

	public EKvType KType { get; set; } = EKvType.Str;
	[Doc(@$"#See[{nameof(KeysProp)}]")]
	public str? KStr { get; set; }
	
	/// KType非I64旹、忽略KI64。用匪空類型可免裝箱
	
	public i64 KI64 { get; set; }
	//public str KeyType {get; set;} = "";

	public EKvType VType { get; set; }

	public str? VStr { get; set; }

	public i64 VI64 { get; set; }

	public f64 VF64 { get; set; }
	public u8[]? VBinary {get;set;}
}
