using Ngaq.Core.Shared.User.Models.Bo.Device;
using Ngaq.Core.Shared.User.Models.Po.Device;
using Ngaq.Core.Infra.IF;

namespace Ngaq.Core.Shared.User.Models.Req {
	using Ngaq.Core.Shared.Base.Models.Req;
// [MkFactory(
// 	For = typeof(ReqLogin)
// 	,MethodName = "Mk"
// )]
public partial class ReqLogin: IReq{
	public str? UniqName{get;set;}
	public str? Email{get;set;}
	public str? Password{get;set;}
	public EUserIdentityMode UserIdentityMode{get;set;} = EUserIdentityMode.UniqName;
	public bool KeepLogin{get;set;} = false;
	public IdClient? CliendId{get;set;}
	public EClientType CliendType{get;set;}
	public str? IpAddr{get;set;}

	public enum EUserIdentityMode{
		UniqName = 1,
		Email = 2
	}
	public nil CheckSelf(){
		var z = this;
		var R = new List<str>();
		if(z.UserIdentityMode == EUserIdentityMode.UniqName){
			if(str.IsNullOrEmpty(z.UniqName)){

			}
		}
		return NIL;
	}
}

}//~Ns



