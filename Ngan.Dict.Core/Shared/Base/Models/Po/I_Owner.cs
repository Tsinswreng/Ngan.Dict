using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Shared.User.Models.Po.User;
using Tsinswreng.CsErr;

namespace Ngaq.Core.Shared.Base.Models.Po;

public interface I_Owner{
	public IdUser Owner{get;set;}
}


public static class ExtnI_Owner{
	extension<TPo>(TPo z)
		where TPo:I_Owner
	{
		public TPo CheckOwner(
			IdUser UserId
		){
			var x = z;
			if(x.Owner != UserId){
				throw KeysErr.Common.PermissionDenied.ToErr();
			}
			return x;
		}
		public TPo AssignOwner(IdUser UserId){
			z.Owner = UserId;
			return z;
		}
	}
	extension<TPo>(IEnumerable<TPo> z)
		where TPo:I_Owner
	{
		public IEnumerable<TPo> CheckOwner(
			IdUser UserId
		){
			return z.Select(x=>{
				if(x.Owner != UserId){
					throw KeysErr.Common.PermissionDenied.ToErr();
				}
				return x;
			});
		}
		public IEnumerable<TPo> AssignOwner(IdUser UserId){
			return z.Select(x=>{
				x.Owner = UserId;
				return x;
			});
		}
	}
	extension<TPo>(IAsyncEnumerable<TPo> z)
		where TPo:I_Owner
	{
		public IAsyncEnumerable<TPo> CheckOwner(
			IdUser UserId
		){
			return z.Select(x=>{
				if(x.Owner != UserId){
					throw KeysErr.Common.PermissionDenied.ToErr();
				}
				return x;
			});
		}
		public IAsyncEnumerable<TPo> AssignOwner(IdUser UserId){
			return z.Select(x=>{
				x.Owner = UserId;
				return x;
			});
		}
	}
}
