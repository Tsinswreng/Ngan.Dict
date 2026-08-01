using Ngaq.Core.Infra.IF;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Tsinswreng.CsSql;

namespace Ngaq.Core.Shared.Dictionary.Models.Po.NormLang;

[Doc(@$"
User Defined Normalized Language
")]
public partial class PoNormLang
	:PoBaseBizTime
	,I_Id<IdNormLang>
	,I_Owner
	,INormLangDetail
{
	[Doc(@$"無實體業務含義、不要用來做持久關聯。
	相同的語言 被刪除又重新添加後、 {nameof(Id)}會變、
	({nameof(Type)},{nameof(Code)})不變。
	")]
	public IdNormLang Id { get; set; } = new();
	public IdUser Owner { get; set; } = IdUser.Zero;
	public ELangIdentType Type{get;set;} = ELangIdentType.Bcp47;
	public str Code {get;set;} = "";
	public str NativeName {get;set;} = "";
	public str EnglishName {get;set;} = "";
	public f64 Weight {get;set;} = 0;
}
