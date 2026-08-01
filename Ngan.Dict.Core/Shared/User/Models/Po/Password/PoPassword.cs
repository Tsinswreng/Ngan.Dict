namespace Ngaq.Core.Models.Sys.Po.Password;

using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Model.Sys.Po.Password;


public partial class PoPassword
	:PoBase
	,AppI_Id<IdPassword>
{
	public IdPassword Id{get;set;}
	public IdUser UserId{get;set;}
	public EAlgo Algo{get;set;} = EAlgo.Argon2id;
	public str Text{get;set;}="";
	
	/// 蜮直ᵈ被含于Text中 如用Argon2id旹 此旹不用管 鹽
	
	//public str? Salt{get;set;} =""; 使Text含Salt。具體格式決于Algo
	public enum EAlgo{
		Argon2id = 1,
	}

}
