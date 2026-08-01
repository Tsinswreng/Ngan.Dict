using Ngan.Dict.Core.Infra.IF;
using Ngan.Dict.Core.Model.Sys.Po.Password;
using Ngan.Dict.Core.Models.Sys.Po.Password;
using Ngan.Dict.Core.Model.Sys.Po.RefreshToken;
using Ngan.Dict.Core.Shared.User.Models.Bo.Device;
using Ngan.Dict.Core.Shared.User.Models.Po.Device;
using Ngan.Dict.Core.Shared.User.Models.Po.RefreshToken;
using Ngan.Dict.Core.Shared.User.Models.Po.User;
using Ngan.Dict.Core.Shared.User.Svc;
using Ngan.Dict.Backend.Db.TswG;
using Ngan.Dict.Server.Db.User;
using Ngan.Dict.Server.Domains.User;
using Ngan.Dict.Server.Domains.User.Dao;
using Tsinswreng.CsSql;
using Tsinswreng.CsTreeTest;

namespace Ngan.Dict.Server.Test.Domains.User.Svc;

public partial class TestISvcUser: ITester{
	readonly ISvcUser _svcUser;
	readonly DaoUser _daoUser;
	readonly DaoToken _daoToken;
	readonly IRepo<PoUser, IdUser> _repoUser;
	readonly IRepo<PoPassword, IdPassword> _repoPassword;
	readonly IRepo<PoRefreshToken, IdRefreshToken> _repoRefreshToken;

	str _token = "";
	IdUser? _createdUserId = null;
	IdPassword? _createdPasswordId = null;
	readonly List<IdRefreshToken> _tokenIds = [];
	IdClient _clientId = new();

	public TestISvcUser(
		ISvcUser svcUser
		,DaoUser daoUser
		,DaoToken daoToken
		,IRepo<PoUser, IdUser> repoUser
		,IRepo<PoPassword, IdPassword> repoPassword
		,IRepo<PoRefreshToken, IdRefreshToken> repoRefreshToken
	){
		_svcUser = svcUser;
		_daoUser = daoUser;
		_daoToken = daoToken;
		_repoUser = repoUser;
		_repoPassword = repoPassword;
		_repoRefreshToken = repoRefreshToken;
	}

	public ITestNode RegisterTestsInto(ITestNode? node){
		node ??= new TestNode();
		node.Ordered = true;
		var register = node.MkTestFnRegister(
			typeof(TestISvcUser),
			[typeof(ISvcUser)],
			[],
			nameof(TestISvcUser)
		);
		var R = register.Register;

		R("SvcUser_Setup", async(o)=>{
			_token = "ut_server_svcu_" + Guid.NewGuid().ToString("N");
			_clientId = new IdClient();
			_createdUserId = null;
			_createdPasswordId = null;
			_tokenIds.Clear();
			return NIL;
		});

		RegisterAddLoginLogout(node);

		R("SvcUser_Cleanup", async(o)=>{
			await CleanupData();
			return NIL;
		});

		return node;
	}

	async Task CleanupData(){
		var ctx = new DbFnCtx();

		if(_tokenIds.Count > 0){
			await _repoRefreshToken.OrdHardDelById(ctx, AsyE(_tokenIds.Distinct().ToArray()), CT.None);
		}
		if(_createdPasswordId is not null){
			await _repoPassword.OrdHardDelById(ctx, AsyE(_createdPasswordId.Value), CT.None);
		}
		if(_createdUserId is not null){
			await _repoUser.OrdHardDelById(ctx, AsyE(_createdUserId.Value), CT.None);
		}
	}

	IServerUserCtx MkServerUserCtx(){
		return new ServerUserCtx{
			ClientId = _clientId,
			ClientType = EClientType.ApiTool,
			IpAddr = "127.0.0.1",
			UserAgent = "Ngan.Dict.Server.Test"
		};
	}

	static async IAsyncEnumerable<T> AsyE<T>(params T[] items){
		foreach(var i in items){
			yield return i;
		}
	}

	static async Task<List<T>> ToList<T>(IAsyncEnumerable<T> asy){
		var list = new List<T>();
		await foreach(var x in asy){
			list.Add(x);
		}
		return list;
	}
}
