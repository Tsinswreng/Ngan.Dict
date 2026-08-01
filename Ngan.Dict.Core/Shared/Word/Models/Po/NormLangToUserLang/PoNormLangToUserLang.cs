using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Tsinswreng.CsSql;

namespace Ngaq.Core.Shared.Word.Models.Po.NormLangToUserLang;

[Doc(@$"
此實體中定義 標準語言 與 用戶自定義的語言(即 {nameof(PoWord.Lang)}) 之映射。
適用于查字典時加入詞庫。
{nameof(NormLang)} 對 {nameof(UserLang)} 可能是多對一。
使用唯一名 作映射、不便于使用Id、緣Id無業務含義
")]
public partial class PoNormLangToUserLang
	:PoBaseBizTime
	,I_Id<IdNormLangToUserLang>
	,I_Owner
{
	public IdNormLangToUserLang Id { get; set; } = new();
	public IdUser Owner { get; set; }
	
	[Doc(@$"關聯的 標準化的 語言標識 類型 如 {nameof(ELangIdentType.Bcp47)}")]
	public ELangIdentType NormLangType{get;set;} = ELangIdentType.Bcp47;
	
	[Doc(@$"標準化的 語言標識
	如 當使用 {nameof(ELangIdentType.Bcp47)}時的 `zh-Hant-TW`")]
	public str? NormLang {get;set;} = "";
	
	public str? Descr {get;set;} = "";
	
	[Doc(@$"用戶自定義的語言
	#See[{nameof(PoWord.Lang)}]
	")]
	public str UserLang{get;set;} = "";
	
}
