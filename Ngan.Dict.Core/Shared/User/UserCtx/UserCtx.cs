namespace Ngan.Dict.Core.Shared.User.UserCtx;

using Ngan.Dict.Core.Infra.Errors;
using Ngan.Dict.Core.Infra.IF;
using Ngan.Dict.Core.Shared.User.Models.Po.User;
using Ngan.Dict.Core.Tools;
using Tsinswreng.CsErr;

public partial class UserCtx
	: IUserCtx
{
	public UserCtx(){

	}
	public IdUser _UserId = IdUser.Zero;
	public IdUser UserId{get{
		if(_UserId.IsNullOrDefault()){
			throw KeysErr.User.AuthenticationFailed.ToErr();
		}
		return _UserId;
	}set{
		_UserId = value;
	}}
	public IDictionary<str, obj?>? Props{get;set;}

}




