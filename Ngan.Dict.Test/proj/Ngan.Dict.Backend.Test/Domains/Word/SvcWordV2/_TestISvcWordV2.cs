using Ngan.Dict.Core.Infra;
using Ngan.Dict.Core.Infra.IF;
using Ngan.Dict.Core.Shared.Kv.Models;
using Ngan.Dict.Core.Model.Po.Kv;
using Ngan.Dict.Core.Model.Po.Learn_;
using Ngan.Dict.Core.Shared.StudyPlan.Models.Po.PreFilter;
using Ngan.Dict.Core.Shared.StudyPlan.Models.Po.StudyPlan;
using Ngan.Dict.Core.Shared.StudyPlan.Svc;
using Ngan.Dict.Core.Shared.User.Models.Po.User;
using Ngan.Dict.Core.Shared.User.UserCtx;
using Ngan.Dict.Core.Shared.Word.Models.Po.Kv;
using Ngan.Dict.Core.Shared.Word.Models.Po.Learn;
using Ngan.Dict.Core.Shared.Word.Models.Po.NormLangToUserLang;
using Ngan.Dict.Core.Shared.Word.Models.Po.Word;
using Ngan.Dict.Core.Shared.Word.Svc;
using Tsinswreng.CsSql;
using Tsinswreng.CsTreeTest;

namespace Ngan.Dict.Backend.Test.Domains.Word;

public partial class TestISvcWordV2: ITester{
	readonly ISvcWordV2 SvcWordV2;
	readonly IRepo<PoWord, IdWord> RepoWord;
	readonly IRepo<PoWordProp, IdWordProp> RepoProp;
	readonly IRepo<PoWordLearn, IdWordLearn> RepoLearn;
	readonly IRepo<PoNormLangToUserLang, IdNormLangToUserLang> RepoNormLangToUserLang;
	readonly IRepo<PoKv, IdKv> RepoKv;
	readonly IRepo<PoStudyPlan, IdStudyPlan> RepoStudyPlan;
	readonly IRepo<PoPreFilter, IdPreFilter> RepoPreFilter;
	readonly ISvcStudyPlan SvcStudyPlan;

	public TestISvcWordV2(
		ISvcWordV2 SvcWordV2
		,IRepo<PoWord, IdWord> RepoWord
		,IRepo<PoWordProp, IdWordProp> RepoProp
		,IRepo<PoWordLearn, IdWordLearn> RepoLearn
		,IRepo<PoNormLangToUserLang, IdNormLangToUserLang> RepoNormLangToUserLang
		,IRepo<PoKv, IdKv> RepoKv
		,IRepo<PoStudyPlan, IdStudyPlan> RepoStudyPlan
		,IRepo<PoPreFilter, IdPreFilter> RepoPreFilter
		,ISvcStudyPlan SvcStudyPlan
	){
		this.SvcWordV2 = SvcWordV2;
		this.RepoWord = RepoWord;
		this.RepoProp = RepoProp;
		this.RepoLearn = RepoLearn;
		this.RepoNormLangToUserLang = RepoNormLangToUserLang;
		this.RepoKv = RepoKv;
		this.RepoStudyPlan = RepoStudyPlan;
		this.RepoPreFilter = RepoPreFilter;
		this.SvcStudyPlan = SvcStudyPlan;
	}

	public ITestNode RegisterTestsInto(ITestNode? Node){
		Node ??= new TestNode();
		//Node.Ordered = true;
		RegisterGetWordsToLearn(Node);
		RegisterBatAddNewLearnRecord(Node);
		RegisterSoftDelJnWordInId(Node);
		RegisterSoftDelPoWordInId(Node);
		RegisterDelWordPropInId(Node);
		RegisterDelWordLearnInId(Node);
		RegisterHardDelSoftDeleted(Node);
		RegisterLlmDictWordToJnWord(Node);
		RegisterBatUpdHeadLang(Node);
		RegisterBatUpdPoWord(Node);
		RegisterBatChangeId(Node);
		RegisterBizSyncJnWordByBizId(Node);
		RegisterBizSyncJnWordByBizId_2(Node);
		RegisterBizSyncJnWordByBizIdFromStream(Node);
		RegisterBatSyncByDto(Node);
		RegisterGetAllWordsWithDel(Node);
		RegisterPackAllWordsWithDel(Node);
		RegisterUnpackJnWords(Node);
		RegisterMergeWord(Node);
		RegisterSyncNoChange(Node);
		RegisterSyncRemoteIsOlder(Node);
		RegisterSyncLocalNotExist(Node);
		RegisterSyncIdNotEqual(Node);
		RegisterPageSearch(Node);

		return Node;
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

	async Task TryCleanupByHeadOwner(IdUser Owner, str Token){
		try{
			await RunNoTxn(async(Ctx)=>{
				var words = (await ToList(RepoWord.GetAll(Ctx, CT.None)))
					.Where(x=>x.Owner == Owner && x.Head.StartsWith(Token))
					.ToList();
				if(words.Count == 0){
					return NIL;
				}

				var wordIds = words.Select(x=>x.Id).Distinct().ToArray();
				var propIds = (await ToList(RepoProp.GetAll(Ctx, CT.None)))
					.Where(x=>wordIds.Contains(x.WordId))
					.Select(x=>x.Id)
					.ToArray();
				var learnIds = (await ToList(RepoLearn.GetAll(Ctx, CT.None)))
					.Where(x=>wordIds.Contains(x.WordId))
					.Select(x=>x.Id)
					.ToArray();

				if(propIds.Length > 0){
					await RepoProp.OrdHardDelById(Ctx, AsyE(propIds), CT.None);
				}
				if(learnIds.Length > 0){
					await RepoLearn.OrdHardDelById(Ctx, AsyE(learnIds), CT.None);
				}
				await RepoWord.OrdHardDelById(Ctx, AsyE(wordIds), CT.None);
				return NIL;
			});
		}
		catch{
		}
	}
}
