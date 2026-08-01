namespace Ngaq.Core.Shared.User.UserCtx;

using Ngaq.Core.Shared.User.Models.Po.User;


public partial interface IUserCtx{
	public IdUser UserId{get;set;}
	public IDictionary<str, obj?>? Props{get;set;}
}
