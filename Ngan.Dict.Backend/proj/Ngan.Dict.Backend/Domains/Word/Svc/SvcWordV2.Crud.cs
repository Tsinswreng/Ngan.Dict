namespace Ngan.Dict.Backend.Domains.Word.Svc;

using System.Runtime.CompilerServices;
using Ngan.Dict.Backend.Domains.Word.Dao;
using Ngan.Dict.Core.Infra;
using Ngan.Dict.Core.Infra.Errors;
using Ngan.Dict.Core.Model.Po.Kv;
using Ngan.Dict.Core.Model.Po.Learn_;
using Ngan.Dict.Core.Shared.Base.Models.Po;
using Ngan.Dict.Core.Shared.Dictionary.Models;
using Ngan.Dict.Core.Shared.User.Models.Po.User;
using Ngan.Dict.Core.Shared.Word.Models;
using Ngan.Dict.Core.Shared.Word.Models.Dto;
using Ngan.Dict.Core.Shared.Word.Models.Learn_;
using Ngan.Dict.Core.Shared.Word.Models.Po.Kv;
using Ngan.Dict.Core.Shared.Word.Models.Po.Learn;
using Ngan.Dict.Core.Shared.Word.Models.Po.Word;
using Ngan.Dict.Core.Shared.Word.Svc;
using Ngan.Dict.Core.Tools;
using Tsinswreng.CsErr;
using Tsinswreng.CsSql;
using Tsinswreng.CsTempus;
using Tsinswreng.CsTools;
using IStr_Obj = System.Collections.Generic.IDictionary<str, obj?>;

public partial class SvcWordV2
{
	// ■ 流式版(對外入口):整流一個事務,批大小/切批交給庫原語 SqlFlow.BatchesInOnly,
	//   逐批調 IList 批核心(其內 EnsureTxn 加入外層事務)。
	public Task<nil> OrdAddNewLearnRecord(
		IDbUserCtx Ctx
		,IAsyncEnumerable<PoWordLearn> PoWordLearnAsyE, CT Ct
	){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, DbCtx => {
			var localCtx = new DbUserCtx(Ctx.UserCtx, DbCtx);
			return SqlFlow.BatchesInOnly(PoWordLearnAsyE, (batch, ct2)=> OrdAddNewLearnRecord(localCtx, batch, ct2), Ct);
		});
	}

	// ■ IList 批核心:函數只管一批——整批校驗 Owner、一次 OrdAdd、一次觸碰 BizUpdatedAt。
	public Task<nil> OrdAddNewLearnRecord(
		IDbUserCtx Ctx
		,IList<PoWordLearn> WordLearns, CT Ct
	){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			var DistinctIds = DistinctWordIds(WordLearns.Select(x=>x.WordId).ToList());
			if(DistinctIds.Count == 0){
				return NIL;
			}
			await EnsureOwner(DbCtx, Ctx.UserCtx.UserId, DistinctIds, Ct);
			await RepoLearn.OrdAdd(DbCtx, WordLearns, Ct);
			await DaoWordV2.BatAltWordAfterUpd(DbCtx, DistinctIds, Ct);
			return NIL;
		});
	}

	// ■ 流式版(對外入口):整流一個事務,逐批調 IList 批核心。
	public Task<nil> SoftDelJnWordInId(
		IDbUserCtx Ctx,
		IAsyncEnumerable<IdWord> Ids, CT Ct
	){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, DbCtx => {
			var localCtx = new DbUserCtx(Ctx.UserCtx, DbCtx);
			return SqlFlow.BatchesInOnly(Ids, (batch, ct2)=> SoftDelJnWordInId(localCtx, batch, ct2), Ct);
		});
	}

	// ■ IList 批核心:函數只管一批——整批校驗 Owner、一次聚合軟刪(根+資產)。
	public Task<nil> SoftDelJnWordInId(
		IDbUserCtx Ctx,
		IList<IdWord> Ids, CT Ct
	){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			var DistinctIds = DistinctWordIds(Ids);
			if(DistinctIds.Count == 0){
				return NIL;
			}
			await EnsureOwner(DbCtx, Ctx.UserCtx.UserId, DistinctIds, Ct);
			await RepoWord.SoftDelAggInId<JnWord>(DbCtx, DistinctIds, Ct);
			return NIL;
		});
	}

	// ■ 流式版(對外入口):整流一個事務,逐批調 IList 批核心。
	public Task<nil> SoftDelPoWordInId(
		IDbUserCtx Ctx,
		IAsyncEnumerable<IdWord> Ids, CT Ct
	){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, DbCtx => {
			var localCtx = new DbUserCtx(Ctx.UserCtx, DbCtx);
			return SqlFlow.BatchesInOnly(Ids, (batch, ct2)=> SoftDelPoWordInId(localCtx, batch, ct2), Ct);
		});
	}

	/// 只軟刪聚合根，不動資產刪除狀態。
	// ■ IList 批核心:函數只管一批——整批校驗 Owner、一次 IN 軟刪根。
	public Task<nil> SoftDelPoWordInId(
		IDbUserCtx Ctx,
		IList<IdWord> Ids, CT Ct
	){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			var DistinctIds = DistinctWordIds(Ids);
			if(DistinctIds.Count == 0){
				return NIL;
			}
			await EnsureOwner(DbCtx, Ctx.UserCtx.UserId, DistinctIds, Ct);
			// 批量刪除一律 IN 語義(無序、忽略不存在、可重複)——整批一次 UPDATE。
			await RepoWord.SoftDelInId(DbCtx, DistinctIds, Ct);
			return NIL;
		});
	}

	/// 清理當前用戶詞庫中全部已軟刪除數據（根詞與資產）。
	public async Task<nil> HardDelSoftDeleted(IDbUserCtx Ctx, CT Ct){
		return await SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			return await DaoWordV2.HardDelSoftDeletedByOwner(DbCtx, Ctx.UserCtx.UserId, Ct);
		});
	}

	async Task<nil> EnsureOwner(
		IDbFnCtx Ctx
		,IdUser UserId
		,IList<IdWord> Ids
		,CT Ct
	){
		if(Ids.Count == 0){
			return NIL;
		}
		var Got = await DaoWordV2.BatGetPoWordByIdWithDel(Ctx, Ids, Ct);
		if(Got.Count != Ids.Count){
			throw KeysErr.Word.__And__IsNotSameUserWord.ToErr(UserId);
		}
		for(var i = 0; i < Ids.Count; i++){
			var Po = Got[i];
			var Id = Ids[i];
			if(Po is null || Po.Owner != UserId){
				throw KeysErr.Word.__And__IsNotSameUserWord.ToErr(Id, UserId);
			}
		}
		return NIL;
	}

	public async Task<JnWord> LlmDictWordToJnWord(IDbUserCtx Ctx, IReqLlmDict Req, IRespLlmDict LlmDict, CT Ct){
		var SrcLang = Req.OptLang.SrcLang;
		var NormLangType = SrcLang.Type == ELangIdentType.Unknown ? ELangIdentType.Bcp47 : SrcLang.Type;
		var NormLangCode = string.IsNullOrWhiteSpace(SrcLang.Code) ? "en" : SrcLang.Code.Trim();
		var Lang = await ResolveUserLangByNormLang(Ctx, NormLangType, NormLangCode, Ct);
		var Now = UnixMs.Now();

		var R = new JnWord{
			Word = new PoWord{
				Owner = Ctx.UserCtx.UserId,
				Head = (LlmDict.Head ?? "").Trim(),
				Lang = Lang,
				StoredAt = Now,
				BizCreatedAt = Now,
				BizUpdatedAt = Now,
			},
			Props = [],
			Learns = [],
		};

		AppendDescrProps(R.Props, LlmDict.Descrs, Now);
		AppendPronunciationProps(R.Props, LlmDict.Pronunciations, Now);
		R.EnsureForeignId();
		return R;
	}
	
	public async Task<JnWord> LlmDictWordToJnWordWithLearn(
		IDbUserCtx Ctx
		,IReqLlmDict Req
		,IRespLlmDict LlmDict, CT Ct
	){
		var Jnword = await LlmDictWordToJnWord(Ctx, Req, LlmDict, Ct);
		var learn = new PoWordLearn{
			WordId = Jnword.Word.Id,
			LearnResult = ELearn.Add,
			BizCreatedAt = UnixMs.Now(),
		};
		Jnword.Learns.Add(learn);
		return Jnword;
	}

	// ■ 流式版(對外入口):整流一個事務,逐批調 IList 批核心。
	public Task<nil> OrdAddJnWord(IDbUserCtx Ctx, IAsyncEnumerable<JnWord> Words, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, DbCtx => {
			var localCtx = new DbUserCtx(Ctx.UserCtx, DbCtx);
			return SqlFlow.BatchesInOnly(Words, (batch, ct2)=> OrdAddJnWord(localCtx, batch, ct2), Ct);
		});
	}

	/// 批量新增整詞聚合。先統一 Owner，再確保資產外鍵指向聚合根。
	// ■ IList 批核心:函數只管一批——歸屬歸一化後、整批 OrgAddAgg 一次完成。
	public Task<nil> OrdAddJnWord(IDbUserCtx Ctx, IList<JnWord> Words, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			var owner = Ctx.UserCtx.UserId;
			foreach(var w in Words){
				w.Owner = owner;
				w.EnsureForeignId();
			}
			try{
				await RepoWord.OrdAddAgg<JnWord>(DbCtx, Words, Ct);
			}catch(Exception e){
				throw KeysErr.Common.DataIllegalOrConflict.ToErr(e.Message);
			}
			return NIL;
		});
	}

	// ■ 流式版(對外入口):整流一個事務,逐批調 IList 批核心。
	public Task<nil> OrdUpdWordProp(IDbUserCtx Ctx, IAsyncEnumerable<PoWordProp> WordProps, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, DbCtx => {
			var localCtx = new DbUserCtx(Ctx.UserCtx, DbCtx);
			return SqlFlow.BatchesInOnly(WordProps, (batch, ct2)=> OrdUpdWordProp(localCtx, batch, ct2), Ct);
		});
	}

	/// 批量更新屬性資產。需先校驗全部屬性所屬單詞的 Owner。
	// ■ IList 批核心:函數只管一批——整批校驗、一次 OrdUpd、一次觸碰。
	public Task<nil> OrdUpdWordProp(IDbUserCtx Ctx, IList<PoWordProp> WordProps, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			if(WordProps.Count == 0){
				return NIL;
			}
			var wordIds = DistinctWordIds(WordProps.Select(x=>x.WordId).ToList());
			if(wordIds.Count == 0){
				return NIL;
			}
			await EnsureOwner(DbCtx, Ctx.UserCtx.UserId, wordIds, Ct);
			await RepoProp.OrdUpd(DbCtx, WordProps, Ct);
			await DaoWordV2.BatAltWordAfterUpd(DbCtx, wordIds, Ct);
			return NIL;
		});
	}

	// ■ 流式版(對外入口):整流一個事務,逐批調 IList 批核心。
	public Task<nil> DelWordPropInId(IDbUserCtx Ctx, IAsyncEnumerable<IdWordProp> Ids, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, DbCtx => {
			var localCtx = new DbUserCtx(Ctx.UserCtx, DbCtx);
			return SqlFlow.BatchesInOnly(Ids, (batch, ct2)=> DelWordPropInId(localCtx, batch, ct2), Ct);
		});
	}

	/// 批量軟刪屬性資產，並觸碰其聚合根 BizUpdatedAt。
	// ■ IList 批核心:函數只管一批——整批讀回定位所屬根、一次校驗、一次軟刪、一次觸碰。
	public Task<nil> DelWordPropInId(IDbUserCtx Ctx, IList<IdWordProp> Ids, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			if(Ids.Count == 0){
				return NIL;
			}
			var props = await RepoProp.OrdGetByIdWithDel(DbCtx, Ids, Ct);
			var wordIds = DistinctWordIds(props.Where(x=>x is not null).Select(x=>x!.WordId).ToList());
			if(wordIds.Count > 0){
				await EnsureOwner(DbCtx, Ctx.UserCtx.UserId, wordIds, Ct);
			}
			await RepoProp.SoftDelInId(DbCtx, Ids, Ct);
			if(wordIds.Count > 0){
				await DaoWordV2.BatAltWordAfterUpd(DbCtx, wordIds, Ct);
			}
			return NIL;
		});
	}

	// ■ 流式版(對外入口):整流一個事務,逐批調 IList 批核心。
	public Task<nil> DelWordLearnInId(IDbUserCtx Ctx, IAsyncEnumerable<IdWordLearn> Ids, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, DbCtx => {
			var localCtx = new DbUserCtx(Ctx.UserCtx, DbCtx);
			return SqlFlow.BatchesInOnly(Ids, (batch, ct2)=> DelWordLearnInId(localCtx, batch, ct2), Ct);
		});
	}

	/// 批量軟刪學習資產，並觸碰其聚合根 BizUpdatedAt。
	// ■ IList 批核心:函數只管一批——整批讀回定位所屬根、一次校驗、一次軟刪、一次觸碰。
	public Task<nil> DelWordLearnInId(IDbUserCtx Ctx, IList<IdWordLearn> Ids, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			if(Ids.Count == 0){
				return NIL;
			}
			var learns = await RepoLearn.OrdGetByIdWithDel(DbCtx, Ids, Ct);
			var wordIds = DistinctWordIds(learns.Where(x=>x is not null).Select(x=>x!.WordId).ToList());
			if(wordIds.Count > 0){
				await EnsureOwner(DbCtx, Ctx.UserCtx.UserId, wordIds, Ct);
			}
			await RepoLearn.SoftDelInId(DbCtx, Ids, Ct);
			if(wordIds.Count > 0){
				await DaoWordV2.BatAltWordAfterUpd(DbCtx, wordIds, Ct);
			}
			return NIL;
		});
	}

	/// 先按 Head/Lang 需要時遷移目標詞，再更新其餘字段。
	// ■ 流式版(對外入口):整流一個事務,逐批調 IList 批核心、結果攤平輸出。
	public Task<IAsyncEnumerable<RespUpdPoWord>> OrdUpdPoWord(IDbUserCtx Ctx, IAsyncEnumerable<PoWord> PoWords, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			var localCtx = new DbUserCtx(Ctx.UserCtx, DbCtx);
			var all = new List<RespUpdPoWord>();
			await foreach(var batchR in SqlFlow.Batches(
				PoWords, async(batch, ct2)=> (IEnumerable<RespUpdPoWord>)await OrdUpdPoWord(localCtx, batch, ct2), Ct
			).WithCancellation(Ct)){
				all.AddRange(batchR);
			}
			return ToAsyE(all);
		});
	}

	// ■ IList 批核心:函數只管一批——按位置批量查舊詞、逐條校驗、
	//   需遷移 BizId 者集中一步遷移、其餘字段一步批量更新、一次觸碰。
	public Task<IList<RespUpdPoWord>> OrdUpdPoWord(IDbUserCtx Ctx, IList<PoWord> PoWords, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			var results = new List<RespUpdPoWord>(PoWords.Count);

			// step 1: 整批查舊詞(位置一一對應、查無補 null)。
			var oldPos = await DaoWordV2.BatGetPoWordByIdWithDel(
				DbCtx, PoWords.Select(x=>x.Id).ToList(), Ct
			);

			// step 2: 需要遷移 (Head,Lang) 的參數集中收集、一次 BatUpdHeadLangCore。
			var headLangArgs = new List<PoWord>(PoWords.Count);
			var headLangIdx = new List<IdWord>(PoWords.Count); // arg.Id → 是否遷移,以位置對應
			var needMigrate = new List<bool>(PoWords.Count);
			var updDicts = new List<(IdWord FinalId, IStr_Obj Dict)>(PoWords.Count);
			var touchIds = new List<IdWord>(PoWords.Count);
			for(var i = 0; i < PoWords.Count; i++){
				var arg = PoWords[i];
				var old = oldPos[i];
				if(old is null){
					throw KeysErr.Word.WordOfId__NotFound.ToErr(arg.Id);
				}
				old.CheckOwner(Ctx.UserCtx.UserId);

				var hasUpdatedBizId = old.Head != arg.Head || old.Lang != arg.Lang;
				needMigrate.Add(hasUpdatedBizId);
				if(hasUpdatedBizId){
					headLangArgs.Add(arg);
				}
			}

			// step 3: 集中遷移(若有),返回位置對齊的 RespUpdBizId。
			IList<RespUpdBizId>? headLangR = null;
			if(headLangArgs.Count > 0){
				headLangR = await BatUpdHeadLangCore(DbCtx, Ctx.UserCtx.UserId, headLangArgs, Ct);
			}

			// step 4: 組裝其餘字段更新字典與觸碰清單。
			var mi = 0;
			for(var i = 0; i < PoWords.Count; i++){
				var arg = PoWords[i];
				IdWord finalId = arg.Id;
				RespUpdBizId? respUpdBizId = null;
				if(needMigrate[i]){
					respUpdBizId = headLangR![mi++];
					finalId = respUpdBizId.FinalId;
				}

				var upd = new Dictionary<str, obj?>{
					[nameof(PoWord.Owner)] = Ctx.UserCtx.UserId,
					[nameof(PoWord.StoredAt)] = arg.StoredAt,
					[nameof(PoWord.BizCreatedAt)] = arg.BizCreatedAt,
					[nameof(PoWord.BizUpdatedAt)] = arg.BizUpdatedAt,
				};
				updDicts.Add((finalId, upd));
				touchIds.Add(finalId);
				results.Add(new RespUpdPoWord{
					HasUpdatedBizId = needMigrate[i],
					RespUpdBizId = respUpdBizId,
					FinalId = finalId,
				});
			}

			// step 5: 一次性更新其餘字段、一次性觸碰。
			if(updDicts.Count > 0){
				await RepoWord.OrdUpdByCodeDict(
					DbCtx, updDicts.Select(x=>x.FinalId).ToList(), updDicts.Select(x=>x.Dict).ToList(), Ct
				);
				await DaoWordV2.BatAltWordAfterUpd(
					DbCtx, DistinctWordIds(touchIds), Ct
				);
			}
			return (IList<RespUpdPoWord>)results;
		});
	}

	/// 批量改主鍵(Old→New)，並同步遷移資產外鍵(見 DaoWordV2.BatChangeWordId)。
	/// 主要用途：同步 IdNotEqual 衝突收斂後、把庫中行遷到「保留 Id」上；New 必須全庫空位。
	/// 校驗口徑：含軟刪行(已刪詞仍佔用 Id/BizId 語義)；Old 必須屬當前用戶、New 必須不存在，違例即拋。
	// ■ 流式版(對外入口):整流一個事務,逐批調 IList 批核心。
	public Task<nil> OrdChangeId(IDbUserCtx Ctx, IAsyncEnumerable<(IdWord Old, IdWord New)> Ids, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, DbCtx => {
			var localCtx = new DbUserCtx(Ctx.UserCtx, DbCtx);
			return SqlFlow.BatchesInOnly(Ids, (batch, ct2)=> OrdChangeId(localCtx, batch, ct2), Ct);
		});
	}

	// ■ IList 批核心:函數只管一批——整批校驗(批量查 Old 存在/屬主、批量查 New 空位)、一次遷移。
	public Task<nil> OrdChangeId(IDbUserCtx Ctx, IList<(IdWord Old, IdWord New)> Ids, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			// Old==New 的 noop 先剔除，避免對主鍵列做無意義的自我 UPDATE
			var moves = Ids.Where(x=>x.Old != x.New).ToList();
			if(moves.Count == 0){
				return NIL;
			}

			// step 1: 整批檢查 old 存在且屬於當前用戶;new 不能已存在(各一次批量查)。
			var oldPos = await DaoWordV2.BatGetPoWordByIdWithDel(
				DbCtx, moves.Select(x=>x.Old).ToList(), Ct
			);
			var newPos = await DaoWordV2.BatGetPoWordByIdWithDel(
				DbCtx, moves.Select(x=>x.New).ToList(), Ct
			);
			for(var i = 0; i < moves.Count; i++){
				var (oldId, newId) = moves[i];
				var old = oldPos[i];
				if(old is null){
					throw KeysErr.Word.WordOfId__NotFound.ToErr(oldId);
				}
				old.CheckOwner(Ctx.UserCtx.UserId);

				var neo = newPos[i];
				if(neo is not null){
					throw KeysErr.Common.DataIllegalOrConflict.ToErr(oldId, newId);
				}
			}

			// step 2: 主鍵與資產外鍵一併遷移。
			await DaoWordV2.BatChangeWordId(DbCtx, moves, Ct);
			return NIL;
		});
	}

	// ■ 流式版(對外入口):整流一個事務,逐批調 IList 批核心、結果攤平輸出。
	public async IAsyncEnumerable<RespUpdBizId> OrdUpdHeadLang(
		IDbUserCtx Ctx,
		IAsyncEnumerable<PoWord> PoWords,
		[EnumeratorCancellation] CT Ct
	){
		var r = await SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			var localCtx = new DbUserCtx(Ctx.UserCtx, DbCtx);
			var all = new List<RespUpdBizId>();
			await foreach(var batchR in SqlFlow.Batches(
				PoWords, async(batch, ct2)=> (IEnumerable<RespUpdBizId>)await OrdUpdHeadLang(localCtx, batch, ct2), Ct
			).WithCancellation(Ct)){
				all.AddRange(batchR);
			}
			return all;
		});
		foreach(var one in r){
			yield return one;
		}
	}

	// ■ IList 批核心(供 OrdUpdHeadLang/OrdUpdPoWord 共用):函數只管一批。
	public Task<IList<RespUpdBizId>> OrdUpdHeadLang(
		IDbUserCtx Ctx, IList<PoWord> PoWords, CT Ct
	){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, DbCtx => BatUpdHeadLangCore(DbCtx, Ctx.UserCtx.UserId, PoWords, Ct));
	}

	async Task<IList<RespUpdBizId>> BatUpdHeadLangCore(
		IDbFnCtx DbCtx,
		IdUser UserId,
		IList<PoWord> Args,
		CT Ct
	){
		if(Args.Count == 0){
			return [];
		}

		// step 1: 批量查當前 Id 對應的舊詞，按位置對齊校驗存在與權限。
		var inputIds = Args.Select(x=>x.Id).ToList();
		var wordsOfId = await DaoWordV2.BatGetPoWordByIdWithDel(DbCtx, inputIds, Ct);
		// step 2: 批量查目標 (Head,Lang) 對應詞（含軟刪），後續按索引一一處理。
		var keys = Args.Select(x=>new Head_Lang(x.Head, x.Lang)).ToList();
		var targets = await DaoWordV2.BatGetPoWordByOwnerHeadLangWithDel(DbCtx, UserId, keys, Ct);

		var r = new List<RespUpdBizId>(Args.Count);
		var updHeadLangArgs = new List<(IdWord Id, str Head, str Lang)>();
		var restoreIds = new List<IdWord>();
		var softDelIds = new List<IdWord>();
		var moveAssetsArgs = new List<(IdWord Old, IdWord New)>();
		var touchIds = new List<IdWord>();

		for(var i = 0; i < Args.Count; i++){
			var arg = Args[i];
			var wordOfId = wordsOfId[i];
			if(wordOfId is null){
				throw KeysErr.Word.WordOfId__NotFound.ToErr(arg.Id);
			}
			wordOfId.CheckOwner(UserId);

			if(wordOfId.Head == arg.Head && wordOfId.Lang == arg.Lang){
				r.Add(new RespUpdBizId{
					Result = EUpdBizIdResult.BizIdAlreadyEqual,
					FinalId = wordOfId.Id,
				});
				continue;
			}

			var target = targets[i];
			if(target is null){
				updHeadLangArgs.Add((wordOfId.Id, arg.Head, arg.Lang));
				r.Add(new RespUpdBizId{
					Result = EUpdBizIdResult.DataOfBizIdNotExist,
					FinalId = wordOfId.Id,
				});
				continue;
			}

			if(!target.DelAt.IsNullOrDefault()){
				restoreIds.Add(target.Id);
			}

			// step 3: 目標已存在時，按規則軟刪舊詞並遷移資產。
			softDelIds.Add(wordOfId.Id);
			moveAssetsArgs.Add((wordOfId.Id, target.Id));
			touchIds.Add(wordOfId.Id);
			touchIds.Add(target.Id);
			r.Add(new RespUpdBizId{
				Result = EUpdBizIdResult.BizIdNotEqual,
				FinalId = target.Id,
			});
		}

		if(updHeadLangArgs.Count > 0){
			await DaoWordV2.BatUpdHeadLangById(DbCtx, updHeadLangArgs, Ct);
		}
		if(restoreIds.Count > 0){
			await DaoWordV2.BatRestoreInId(DbCtx, DistinctWordIds(restoreIds), Ct);
		}
		if(softDelIds.Count > 0){
			await RepoWord.SoftDelInId(DbCtx, DistinctWordIds(softDelIds), Ct);
		}
		if(moveAssetsArgs.Count > 0){
			await DaoWordV2.BatMoveAssetsToWordId(DbCtx, moveAssetsArgs, Ct);
		}
		if(touchIds.Count > 0){
			await DaoWordV2.BatAltWordAfterUpd(DbCtx, DistinctWordIds(touchIds), Ct);
		}
		return r;
	}
	
	// ■ 流式版(對外入口):整流一個事務,逐批調 IList 批核心。
	public Task<nil> OrdAddWordProp(IDbUserCtx Ctx, IAsyncEnumerable<PoWordProp> WordProps, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, DbCtx => {
			var localCtx = new DbUserCtx(Ctx.UserCtx, DbCtx);
			return SqlFlow.BatchesInOnly(WordProps, (batch, ct2)=> OrdAddWordProp(localCtx, batch, ct2), Ct);
		});
	}

	/// 批量新增屬性資產。需先校驗全部屬性所屬單詞的 Owner。
	// ■ IList 批核心:函數只管一批——整批校驗、一次 OrdAdd、一次觸碰。
	public Task<nil> OrdAddWordProp(IDbUserCtx Ctx, IList<PoWordProp> WordProps, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			if(WordProps.Count == 0){
				return NIL;
			}
			var wordIds = DistinctWordIds(WordProps.Select(x=>x.WordId).ToList());
			if(wordIds.Count == 0){
				return NIL;
			}
			await EnsureOwner(DbCtx, Ctx.UserCtx.UserId, wordIds, Ct);
			await RepoProp.OrdAdd(DbCtx, WordProps, Ct);
			await DaoWordV2.BatAltWordAfterUpd(DbCtx, wordIds, Ct);
			return NIL;
		});
	}

	// ■ 流式版(對外入口):整流一個事務,逐批調 IList 批核心。
	public Task<nil> OrdAddWordLearn(IDbUserCtx Ctx, IAsyncEnumerable<PoWordLearn> WordLearns, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, DbCtx => {
			var localCtx = new DbUserCtx(Ctx.UserCtx, DbCtx);
			return SqlFlow.BatchesInOnly(WordLearns, (batch, ct2)=> OrdAddWordLearn(localCtx, batch, ct2), Ct);
		});
	}

	/// 批量新增學習資產。需先校驗全部學習記錄所屬單詞的 Owner。
	// ■ IList 批核心:函數只管一批——整批校驗、一次 OrdAdd、一次觸碰。
	public Task<nil> OrdAddWordLearn(IDbUserCtx Ctx, IList<PoWordLearn> WordLearns, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			if(WordLearns.Count == 0){
				return NIL;
			}
			var wordIds = DistinctWordIds(WordLearns.Select(x=>x.WordId).ToList());
			if(wordIds.Count == 0){
				return NIL;
			}
			await EnsureOwner(DbCtx, Ctx.UserCtx.UserId, wordIds, Ct);
			await RepoLearn.OrdAdd(DbCtx, WordLearns, Ct);
			await DaoWordV2.BatAltWordAfterUpd(DbCtx, wordIds, Ct);
			return NIL;
		});
	}

	// ■ 流式版(對外入口):整流一個事務,逐批調 IList 批核心。
	public Task<nil> OrdUpdWordLearn(IDbUserCtx Ctx, IAsyncEnumerable<PoWordLearn> WordLearns, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, DbCtx => {
			var localCtx = new DbUserCtx(Ctx.UserCtx, DbCtx);
			return SqlFlow.BatchesInOnly(WordLearns, (batch, ct2)=> OrdUpdWordLearn(localCtx, batch, ct2), Ct);
		});
	}

	/// 批量更新學習資產。需先校驗全部學習記錄所屬單詞的 Owner。
	// ■ IList 批核心:函數只管一批——整批校驗、一次 OrdUpd、一次觸碰。
	public Task<nil> OrdUpdWordLearn(IDbUserCtx Ctx, IList<PoWordLearn> WordLearns, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			if(WordLearns.Count == 0){
				return NIL;
			}
			var wordIds = DistinctWordIds(WordLearns.Select(x=>x.WordId).ToList());
			if(wordIds.Count == 0){
				return NIL;
			}
			await EnsureOwner(DbCtx, Ctx.UserCtx.UserId, wordIds, Ct);
			await RepoLearn.OrdUpd(DbCtx, WordLearns, Ct);
			await DaoWordV2.BatAltWordAfterUpd(DbCtx, wordIds, Ct);
			return NIL;
		});
	}

	
}
