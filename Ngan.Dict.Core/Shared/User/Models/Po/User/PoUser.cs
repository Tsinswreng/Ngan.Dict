namespace Ngan.Dict.Core.Shared.User.Models.Po.User;

using Ngan.Dict.Core.Shared.Base.Models.Po;
using Ngan.Dict.Core.Infra;


public partial class PoUser
	:PoBaseBizTime
	,AppI_Id<IdUser>
{
	public IdUser Id{get;set;} = new();
	public str UniqName{get;set;} = "";
	public str? NickName{get;set;}
	public str Email{get;set;} = "";
	public str? PhoneNumber{get;set;}
	public str? Avatar{get;set;}
	//public IdRole RoleId{get;set;}
	//public IEnumerable<Id_Password> PasswordIds{get;set;}=[]; //AI曰不要此
	//public ICollection<Model_Password> Passwords{get;set;}=[]; //取消導航屬性 只在關聯實體類中留外鍵 降侵入性

}

