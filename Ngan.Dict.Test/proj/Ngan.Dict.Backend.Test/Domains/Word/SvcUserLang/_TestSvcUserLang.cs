using Ngan.Dict.Core.Infra;
using Ngan.Dict.Core.Infra.Errors;
using Ngan.Dict.Core.Infra.IF;
using Ngan.Dict.Core.Shared.Dictionary.Models;
using Ngan.Dict.Core.Shared.User.Models.Po.User;
using Ngan.Dict.Core.Shared.User.UserCtx;
using Ngan.Dict.Core.Shared.Word.Models.Po.UserLang;
using Ngan.Dict.Core.Shared.Word.Models.Po.Word;
using Ngan.Dict.Core.Shared.Word.Svc;
using Tsinswreng.CsErr;
using Tsinswreng.CsSql;
using Tsinswreng.CsTempus;
using Tsinswreng.CsTreeTest;

namespace Ngan.Dict.Backend.Test.Domains.Word;

public partial class TestISvcUserLang: ITester{
	readonly ISvcUserLang SvcUserLang = GetRSvc<ISvcUserLang>();
	readonly IRepo<PoUserLang, IdUserLang> RepoUserLang = GetRSvc<IRepo<PoUserLang, IdUserLang>>();
	readonly IRepo<PoWord, IdWord> RepoWord = GetRSvc<IRepo<PoWord, IdWord>>();

	IdUser _ownerA = new();
	IdUser _ownerB = new();
	str _token = "";
	readonly List<IdUserLang> _userLangIds = [];
	readonly List<IdWord> _wordIds = [];

	public ITestNode RegisterTestsInto(ITestNode? Node){
		Node ??= new TestNode();
		Node.Ordered = true;

		var register = Node.MkTestFnRegister(
			typeof(TestISvcUserLang)
			,[typeof(ISvcUserLang), typeof(IRepo<PoUserLang, IdUserLang>), typeof(IRepo<PoWord, IdWord>)]
			,[]
			,nameof(TestISvcUserLang)
		);
		var R = register.Register;

		R("SvcUserLang_Setup_InsertSeedData", async(o)=>{
			await InsertSeedData();
			return NIL;
		});
		RegisterPageUserLang(Node);
		RegisterBatAddUserLang(Node);
		RegisterBatUpdUserLang(Node);
		RegisterGetUnregisteredUserLangs(Node);
		RegisterAddAllUnregisteredUserLangs(Node);

		R("SvcUserLang_Cleanup_AllInsertedData", async(o)=>{
			await CleanupData();
			return NIL;
		});

		return Node;
	}

	static void AssertThrowsErrItem(
		Exception Ex
		,IErrNode Expected
		,str CaseName
	){
		if(Ex is not AppErr appErr){
			throw new Exception($"{CaseName} should throw AppErr, got {Ex.GetType().FullName}");
		}
		if(!ReferenceEquals(appErr.Type, Expected)){
			throw new Exception($"{CaseName} should throw expected err item, got [{appErr.Key}]");
		}
	}

	async Task InsertSeedData(){
		_ownerA = new IdUser();
		_ownerB = new IdUser();
		_token = "ut_user_lang_" + Guid.NewGuid().ToString("N");

		var rows = new[]{
			new PoUserLang{
				Id = new IdUserLang(),
				Owner = _ownerA,
				UniqName = _token + "_page_a_1",
				Descr = "a1",
				RelLangType = ELangIdentType.Bcp47,
				RelLang = _token + "_page_a_1",
				BizUpdatedAt = UnixMs.FromUnixMs(1001),
			},
			new PoUserLang{
				Id = new IdUserLang(),
				Owner = _ownerA,
				UniqName = _token + "_page_a_2",
				Descr = "a2",
				RelLangType = ELangIdentType.Bcp47,
				RelLang = _token + "_page_a_2",
				BizUpdatedAt = UnixMs.FromUnixMs(1002),
			},
			new PoUserLang{
				Id = new IdUserLang(),
				Owner = _ownerA,
				UniqName = _token + "_page_a_3_other_search",
				Descr = "a3",
				RelLangType = ELangIdentType.Bcp47,
				RelLang = _token + "_page_a_3_other_search",
				BizUpdatedAt = UnixMs.FromUnixMs(1003),
			},
			new PoUserLang{
				Id = new IdUserLang(),
				Owner = _ownerB,
				UniqName = _token + "_page_a_4_other_owner",
				Descr = "b1",
				RelLangType = ELangIdentType.Bcp47,
				RelLang = _token + "_page_a_4_other_owner",
				BizUpdatedAt = UnixMs.FromUnixMs(1004),
			},
		};
		await RunNoTxn(async(Ctx)=>{
			await RepoUserLang.OrdAdd(Ctx, AsyE(rows), CT.None);
			return NIL;
		});
		_userLangIds.AddRange(rows.Select(x=>x.Id));
	}

	async Task CleanupData(){
		await RunNoTxn(async(Ctx)=>{
			var userLangIds = _userLangIds.Distinct().ToArray();
			var wordIds = _wordIds.Distinct().ToArray();
			if(userLangIds.Length > 0){
				await RepoUserLang.OrdHardDelById(Ctx, AsyE(userLangIds), CT.None);
			}
			if(wordIds.Length > 0){
				await RepoWord.OrdHardDelById(Ctx, AsyE(wordIds), CT.None);
			}
			return NIL;
		});
	}

	Task<TRtn> RunNoTxn<TRtn>(Func<IDbFnCtx, Task<TRtn>> Fn){
		IDbFnCtx Ctx = new DbFnCtx();
		return Fn(Ctx);
	}

	static async IAsyncEnumerable<T> AsyE<T>(params T[] Items){
		foreach(var I in Items){
			yield return I;
		}
	}

	static async Task<List<T>> ToList<T>(IAsyncEnumerable<T>? Asy){
		if(Asy is null){
			return [];
		}
		var R = new List<T>();
		await foreach(var x in Asy){
			R.Add(x);
		}
		return R;
	}

	IUserCtx MkUser(IdUser UserId){
		return new UserCtx{UserId = UserId};
	}

	IDbUserCtx MkUserCtx(IdUser UserId){
		return new DbUserCtx(MkUser(UserId));
	}
}
