#define Impl
namespace Ngaq.Core.Shared.Word.Models.Po.Learn;

using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Model.Po.Learn_;
using Ngaq.Core.Word.Models.Po.Word;
using Ngaq.Core.Tools;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Tsinswreng.CsTempus;
using Ngaq.Core.Infra.IF;


// public  partial class Po_Learn:Po_Kv{
// 	public static new Po_Learn Example{get;set;} = new Po_Learn();
// }


/// 不當改其CreatedAt與UpdatedAt。唯可刪實體

public partial class PoWordLearn
	:PoBaseBizTime
	,IPoBase
	,AppI_Id<IdWordLearn>
	,I_WordId
{

	public static PoWordLearn Example{get;set;} = new PoWordLearn();

	public IdWordLearn Id { get; set; } = new IdWordLearn();

	public IdWord WordId{get;set;}

	public ELearn LearnResult{get;set;}= ELearn.Add;
}

public static partial class ExtnPoWordLearn{
	public static UnixMs Time(
		this PoWordLearn z
	){
		if(z.BizUpdatedAt.IsNullOrDefault()){
			return z.BizCreatedAt;
		}
		return z.BizUpdatedAt.Value;
	}
}
