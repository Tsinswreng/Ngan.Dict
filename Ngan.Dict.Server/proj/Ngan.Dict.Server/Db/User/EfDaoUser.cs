#if false
namespace Ngan.Dict.Server.Db.User;

using Microsoft.EntityFrameworkCore;
using Ngan.Dict.Core.Shared.User.Models.Po.User;
using Ngan.Dict.Core.Models.Po;
using Ngan.Dict.Core.Models.Sys.Po.Password;
using Ngan.Dict.Backend.Db.TswG;

public partial class DaoEfUser(
	ServerDbCtx DbCtx
){

	public async Task<Func<
		str
		,CT
		,Task<PoUser?>
	>> FnSelectByUniqueName(
		IDbFnCtx DbFnCtx
		,CT Ct
	){
		var Fn = async(str UniqueName, CT Ct)=>{
			return await DbCtx.User.Where(u => u.UniqueName == UniqueName).FirstOrDefaultAsync(Ct);
		};
		return Fn;
	}

	public async Task<Func<
		str
		,CT
		,Task<PoUser?>
	>> FnSelectByEmail(
		IDbFnCtx DbFnCtx
		,CT Ct
	){
		var Fn = async(str Email, CT Ct)=>{
			return await DbCtx.User.Where(u => u.Email == Email).FirstOrDefaultAsync(Ct);
		};
		return Fn;
	}

	public async Task<Func<
		IdUser
		,CT
		,Task<PoPassword>
	>> FnSelectPasswordById(
		IDbFnCtx DbFnCtx
		,CT Ct
	){
		var Fn = async(IdUser UserId, CT Ct)=>{
			return await DbCtx.Password.Where(
				p => p.UserId == UserId
			).FirstOrDefaultAsync(Ct);
		};
		return Fn;
	}
}

#endif
