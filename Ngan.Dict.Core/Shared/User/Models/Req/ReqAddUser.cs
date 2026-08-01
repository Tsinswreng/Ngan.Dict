namespace Ngaq.Core.Shared.User.Models.Req;

using Ngaq.Core.Shared.Base.Models.Req;

public partial class ReqAddUser: IReq{
	public str? UniqName{get;set;} = "";
	public str Email{get;set;} = "";
	public str Password{get;set;} = "";
}
