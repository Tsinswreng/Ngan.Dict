namespace Ngan.Dict.Server.Domains.User;

using Ngan.Dict.Core.Shared.User.Models.Bo.Device;
using Ngan.Dict.Core.Shared.User.Models.Po.Device;
using Ngan.Dict.Core.Shared.User.UserCtx;



public interface IServerUserCtx:IUserCtx{
	public str? IpAddr{get;set;}
	public IdClient ClientId{get;set;}
	public str? UserAgent{get;set;}
	public EClientType ClientType{get;set;}
}
