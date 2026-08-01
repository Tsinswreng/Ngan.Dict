using Ngaq.Core.Frontend.User;
using Ngaq.Core.Shared.User.UserCtx;
using Tsinswreng.CsSql;

namespace Ngaq.Core.Infra;

[Doc(@$"Service context, mostly used in the first param of service API methods")]
public interface IDbUserCtx:IDbFnCtx{
	[Doc(@$"前端調用後端接口時 此成員留null即可、
	留null則 後端自動開啓連接和事務;
	非null則會使用已有的 {nameof(IDbFnCtx)}的事務和連接
	")]
	public IDbFnCtx? DbFnCtx{get;set;}
	public IUserCtx UserCtx{get;set;}
}

public partial class DbUserCtx : IDbUserCtx{
	public DbUserCtx(IUserCtx UserCtx, IDbFnCtx? DbFnCtx = null){
		this.DbFnCtx = DbFnCtx;
		this.UserCtx = UserCtx;
	}
	
	[BeaKona.AutoInterface(IncludeBaseInterfaces = true)]
	public IDbFnCtx? DbFnCtx{get;set;}
	public IUserCtx UserCtx{get;set;}
}

public static class ExtnIDbUserCtx{
	extension(IUserCtx z){
		public IDbUserCtx ToDbUserCtx(IDbFnCtx? DbFnCtx = null){
			return new DbUserCtx(z, DbFnCtx);
		}
	}
	extension(IFrontendUserCtxMgr z){
		public IDbUserCtx GetDbUserCtx(IDbFnCtx? DbFnCtx = null){
			return new DbUserCtx(z.GetUserCtx());
		}
	}
}
