using Ngan.Dict.Core.Infra.IF;
using Ngan.Dict.Core.Models.Sys.Req;
using Ngan.Dict.Core.Shared.User.Models.Po.User;
using Ngan.Dict.Server.Http.Domains.User;
using Tsinswreng.CsTreeTest;

namespace Ngan.Dict.Server.Test.Domains.User.Http;

public partial class TestCtrlrUser{
	public void RegisterLogout(ITestNode node){
		var register = node.MkTestFnRegister(
			typeof(TestCtrlrUser),
			[typeof(CtrlrOpenUser)],
			[nameof(CtrlrOpenUser.Logout)],
			nameof(TestCtrlrUser)
		);
		var R = register.Register;

		R("Logout_Should_UseRealSvcAndReturn200", async(o)=>{
			var ctrlr = MkCtrlr();
			var loginResp = await LoginByService(CT.None);
			var userId = IdUser.Parse(loginResp.UserId);
			var ctx = MkHttpCtxForUser(userId);

			var result = await ctrlr.Logout(new ReqLogout(), ctx, CT.None);
			await AssertResultNotNull(result);
			return NIL;
		});
	}
}
