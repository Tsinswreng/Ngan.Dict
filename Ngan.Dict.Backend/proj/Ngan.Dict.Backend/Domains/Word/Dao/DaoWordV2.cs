using Ngan.Dict.Backend.Word.Dao;

namespace Ngan.Dict.Backend.Domains.Word.Dao;

using System.Runtime.CompilerServices;
using Ngan.Dict.Backend.Db.TswG;
using Ngan.Dict.Backend.Domains.Word;
using Ngan.Dict.Core.Infra.IF;
using Ngan.Dict.Core.Model.Po.Kv;
using Ngan.Dict.Core.Model.Po.Learn_;
using Ngan.Dict.Core.Shared.Base.Models.Po;
using Ngan.Dict.Core.Shared.StudyPlan.Models.PreFilter;
using Ngan.Dict.Core.Shared.User.Models.Po;
using Ngan.Dict.Core.Shared.User.Models.Po.User;
using Ngan.Dict.Core.Shared.Word.Models;
using Ngan.Dict.Core.Shared.Word.Models.Po.Kv;
using Ngan.Dict.Core.Shared.Word.Models.Po.Learn;
using Ngan.Dict.Core.Shared.Word.Models.Po.Word;
using Ngan.Dict.Core.Word.Models.Po.Word;
using Tsinswreng.CsCore;
using Tsinswreng.CsPage;
using Tsinswreng.CsSql;
using Tsinswreng.CsTools;
using IStr_Obj = System.Collections.Generic.IDictionary<str, obj?>;

public partial class DaoWordV2(
	ISqlCmdMkr SqlCmdMkr
	,ITblMgr TblMgr
	,IRepo<PoWord, IdWord> RepoWord
	,IRepo<PoWordProp, IdWordProp> RepoProp
	,IRepo<PoWordLearn, IdWordLearn> RepoLearn
){
	ITable<PoWord> TW => TblMgr.GetTbl<PoWord>();
	ITable<PoWordProp> TP => TblMgr.GetTbl<PoWordProp>();
	ITable<PoWordLearn> TL => TblMgr.GetTbl<PoWordLearn>();
	IRepo<PoWordProp, IdWordProp> RepoProp = RepoProp;
	IRepo<PoWordLearn, IdWordLearn> RepoLearn = RepoLearn;
	PreFilterSqlMkr PreFilterSqlMkr = new();

	/// 常規聚合修改(新增/更新/刪除資產、遷移等)後的統一收尾：
	/// 按根 Id 批量把 PoWord.BizUpdatedAt 置為當前時間(DB 側生成)。
	/// ISvcWordV2 文檔頭約定：凡動過 JnWord(含資產)必須經此觸碰——同步端以 BizUpdatedAt 判新舊，漏觸碰=丟改動。
	public Task<nil> BatAltWordAfterUpd(
		IDbFnCtx Ctx, IAsyncEnumerable<IdWord> Ids, CT Ct
	){
		return RepoWord.AsAppRepo().BatBizTouch(Ctx, Ids, Ct);
	}

	/// 硬刪當前用戶已軟刪除數據；僅刪 DelAt!=0 的行，不做聚合補全，可能留下孤兒資產。
	public async Task<nil> HardDelSoftDeletedByOwner(
		IDbFnCtx Ctx,
		IdUser Owner,
		CT Ct
	){
		var pOwner = TW.Prm("Owner");
		var ownerRaw = TW.UpperToRaw(Owner, nameof(PoWord.Owner));
		var sql =
$"""
DELETE FROM {TP.Qt(TP.DbTblName)}
WHERE {TP.QtCol(nameof(IPoBase.DelAt))} <> 0
AND {TP.QtCol(nameof(PoWordProp.WordId))} IN (
	SELECT {TW.QtCol(nameof(PoWord.Id))}
	FROM {TW.Qt(TW.DbTblName)}
	WHERE {TW.QtCol(nameof(PoWord.Owner))} = {pOwner}
);

DELETE FROM {TL.Qt(TL.DbTblName)}
WHERE {TL.QtCol(nameof(IPoBase.DelAt))} <> 0
AND {TL.QtCol(nameof(PoWordLearn.WordId))} IN (
	SELECT {TW.QtCol(nameof(PoWord.Id))}
	FROM {TW.Qt(TW.DbTblName)}
	WHERE {TW.QtCol(nameof(PoWord.Owner))} = {pOwner}
);

DELETE FROM {TW.Qt(TW.DbTblName)}
WHERE {TW.QtCol(nameof(IPoBase.DelAt))} <> 0
AND {TW.QtCol(nameof(PoWord.Owner))} = {pOwner};
""";
		var cmd = await SqlCmdMkr.Prepare(Ctx, sql, Ct);
		Ctx.AddToDispose(cmd);
		await cmd.RawArgs(new Dictionary<str, obj?>{
			[pOwner.Name] = ownerRaw,
		}).AsyE1d(Ct).FirstOrDefaultAsync(Ct);
		return NIL;
	}

	/// 批量按 Id 取單詞主體(含軟刪)。Ord 語義：出參與入參 Ids 位置一一對應、查無補 null。
	public IAsyncEnumerable<PoWord?> BatGetPoWordByIdWithDel(
		IDbFnCtx Ctx
		,IAsyncEnumerable<IdWord> Ids
		,CT Ct
	){
		return RepoWord.OrdGetByIdWithDel(Ctx, Ids, Ct);
	}

	/// 批量按 Id 取完整聚合(根+Props+Learns、含軟刪)。Ord 語義：出參與入參位置一一對應、查無補 null。
	/// 與 BatGetPoWordByIdWithDel 的分工：先按 BizId 命中根、再用本函數按根 Id 回讀明細(見同步鏈路 step2/3)。
	public IAsyncEnumerable<JnWord?> BatGetJnWordByIdWithDel(
		IDbFnCtx Ctx
		,IAsyncEnumerable<IdWord> Ids
		,CT Ct
	){
		return RepoWord.OrdGetAggByIdWithDel<JnWord>(Ctx, Ids, Ct);
	}
	

	public IAsyncEnumerable<IdWord?> BatGetIdByOwnerHeadLang(
		IDbFnCtx Ctx
		,IdUser Owner
		,IAsyncEnumerable<Head_Lang> HeadLangs
		,CT Ct
	){
		var hl = HeadLangs;
		var Sql = TW.SqlSplicer().Select(x=>x.Id).From().Where1()
		.And(TW.SqlIsNonDel())
		.AndEq(x=>x.Owner, y=>y.One(Owner))
		.AndEq(x=>x.Head, y=>y.Many(hl, z=>z.Head))
		.AndEq(x=>x.Lang, y=>y.Many(hl, z=>z.Lang))
		;
		var Dicts = SqlCmdMkr.RunDupliSql(Ctx, Sql, Ct);
		return Dicts.Select(x=>{
			if(x is null){
				return null;
			}
			var RawId = x[TW.Memb(e=>e.Id)];
			return (IdWord?)IdWord.FromByteArr((u8[])RawId!);
		});
	}
	
	/// 批量按 (Owner, Head, Lang) 查單詞主體（含已軟刪除），返回順序與入參一一對應。
	/// 實現要點：SqlSplicer 構造「一條模板 SQL」——Owner 用 One(常量綁定)、Head/Lang 用 Many(取自入參流)；
	/// RunDupliSql 同構批量執行：每入參元素實例化一條等值 SQL、按批拼進一個 CommandText 一次往返，
	/// 每個結果集取首行、空補 null——這正是「IN 子句做不到」的複合鍵保序查詢(BizId 匹配的地基)。
	public IAsyncEnumerable<PoWord?> BatGetPoWordByOwnerHeadLangWithDel(
		IDbFnCtx Ctx,
		IdUser Owner,
		IAsyncEnumerable<Head_Lang> HeadLangs,
		CT Ct
	){
		var hl = HeadLangs;
		var sql = TW.SqlSplicer().Select("*").From().Where1()
			.AndEq(x=>x.Owner, y=>y.One(Owner))
			.AndEq(x=>x.Head, y=>y.Many(hl, z=>z.Head))
			.AndEq(x=>x.Lang, y=>y.Many(hl, z=>z.Lang))
		;
		return SqlCmdMkr.RunDupliSql(Ctx, TW, sql, Ct);
	}

	/// 批量更新單詞主體的 Head/Lang，並同步觸發 BizUpdatedAt。
	public async Task<nil> BatUpdHeadLangById(
		IDbFnCtx Ctx,
		IAsyncEnumerable<(IdWord Id, str Head, str Lang)> Args,
		CT Ct
	){
		await using var batch = new BatchCollector<(IdWord Id, str Head, str Lang), nil>(async(rows, Ct)=>{
			if(rows.Count == 0){
				return NIL;
			}
			var ids = new List<IdWord>(rows.Count);
			var dicts = new List<IDictionary<str, obj?>>(rows.Count);
			foreach(var row in rows){
				ids.Add(row.Id);
				dicts.Add(new Dictionary<str, obj?>{
					[nameof(PoWord.Head)] = row.Head,
					[nameof(PoWord.Lang)] = row.Lang,
				});
			}
			await RepoWord.OrdUpdByCodeDict(Ctx, ToolAsyE.ToAsyE(ids), ToolAsyE.ToAsyE(dicts), Ct);
			await BatAltWordAfterUpd(Ctx, ToolAsyE.ToAsyE(ids), Ct);
			return NIL;
		});
		await batch.ConsumeAll(Args, Ct);
		return NIL;
	}

	/// 批量取消軟刪除（DelAt = 0），並同步觸發 BizUpdatedAt。
	public async Task<nil> BatRestoreInId(
		IDbFnCtx Ctx,
		IAsyncEnumerable<IdWord> Ids,
		CT Ct
	){
		await using var batch = new BatchCollector<IdWord, nil>(async(idBatch, Ct)=>{
			if(idBatch.Count == 0){
				return NIL;
			}
			var codeDicts = idBatch.Select(_=>new Dictionary<str, obj?>{
				[nameof(IPoBase.DelAt)] = default(IdDel),
			});
			await RepoWord.OrdUpdByCodeDict(Ctx, ToolAsyE.ToAsyE(idBatch), ToolAsyE.ToAsyE(codeDicts), Ct);
			await BatAltWordAfterUpd(Ctx, ToolAsyE.ToAsyE(idBatch), Ct);
			return NIL;
		});
		await batch.ConsumeAll(Ids, Ct);
		return NIL;
	}

	/// 批量把資產 (WordProp/WordLearn) 的 WordId 從 Old 移到 New。
	/// 「移動」洏非「複製」：搬完後 Old 名下不再有任何資產；Old 通常已/將被軟刪(見 OrdUpdHeadLang 衝突分支)。
	/// 先讀後寫是必要的：改外鍵要先知道名下有哪些資產行(Upsert/Upd 都需要完整實體)。
	public async Task<nil> BatMoveAssetsToWordId(
		IDbFnCtx Ctx,
		IAsyncEnumerable<(IdWord Old, IdWord New)> Moves,
		CT Ct
	){
		await using var batch = new BatchCollector<(IdWord Old, IdWord New), nil>(async(moveBatch, Ct)=>{
			if(moveBatch.Count == 0){
				return NIL;
			}
			// step 1: 按外鍵列批量拉取 Old 名下全部資產(含軟刪、避免漏搬)。
			// IncludeEntitysByKeys：跨表按 WordId IN(...) 一批取回、回 Dictionary<IdWord, IList<資產>>；
			// OptQry.InParamCnt 告知預期 IN 數量、由庫按 DB 決定切批(sqlite 參數上限敏感)。
			var oldIds = moveBatch.Select(x=>x.Old).Distinct().ToList();
			var opt = new OptQry{
				InParamCnt = (u64)oldIds.Count,
				IncludeDeleted = true,
			};
			var propsByOld = await RepoWord.IncludeEntitysByKeys(
				Ctx,
				nameof(I_WordId.WordId),
				opt,
				oldIds,
				x=>x.WordId,
				TP,
				Ct
			);
			var learnsByOld = await RepoWord.IncludeEntitysByKeys(
				Ctx,
				nameof(I_WordId.WordId),
				opt,
				oldIds,
				x=>x.WordId,
				TL,
				Ct
			);

			// step 2: 內存改掛——逐條 (Old,New) 把名下實體的 WordId 就地改成 New、聚成待回寫清單。
			// 註：moveBatch 中同一 Old 理論上不該重複出現；若重複、第二輪讀到的已是改掛後狀態。
			var updProps = new List<PoWordProp>();
			var updLearns = new List<PoWordLearn>();
			foreach(var (oldId, newId) in moveBatch){
				foreach(var prop in propsByOld.GetValueOrDefault(oldId, [])){
					prop.WordId = newId;
					updProps.Add(prop);
				}
				foreach(var learn in learnsByOld.GetValueOrDefault(oldId, [])){
					learn.WordId = newId;
					updLearns.Add(learn);
				}
			}
			// step 3: 整行回寫(OrdUpd 含軟刪資產原狀態一併覆蓋、只換外鍵)、兩表各自一次同構批量。
			// 不動資產 BizUpdatedAt：內容未變、僅歸屬變(接口文檔明示)，聚合根的觸碰由調用方負責。
			if(updProps.Count > 0){
				await RepoProp.OrdUpd(Ctx, ToolAsyE.ToAsyE(updProps), Ct);
			}
			if(updLearns.Count > 0){
				await RepoLearn.OrdUpd(Ctx, ToolAsyE.ToAsyE(updLearns), Ct);
			}
			return NIL;
		});
		await batch.ConsumeAll(Moves, Ct);
		return NIL;
	}

	/// 批量更改 Word 主鍵 Id，並同步把全部資產外鍵一起遷移。
	/// 調用前提(OrdChangeId 已校驗)：Old 存在、New 全庫不存在。
	/// 注意：手拼多語句 SQL 走 Prepare+RawArgs 逃逸通道——Repo 更新系 API 一律忽略主鍵列、無法改 Id，
	/// 這是目前庫能力下「改主鍵」的唯一寫法(CsSql 改造時此場景為剛需)。
	public async Task<nil> BatChangeWordId(
		IDbFnCtx Ctx,
		IAsyncEnumerable<(IdWord Old, IdWord New)> Moves,
		CT Ct
	){
		await using var batch = new BatchCollector<(IdWord Old, IdWord New), nil>(async(moveBatch, Ct)=>{
			var normalized = moveBatch.Where(x=>x.Old != x.New).ToList();
			if(normalized.Count == 0){
				return NIL;
			}

			// step 1: 每對 (Old,New) 實例化一條 UPDATE 主鍵語句、以 ';' 拼成一個命令一次往返；
			// 參數名帶下標避免碰撞、值經 UpperToRaw 轉強類型 Id 的庫側表示(BLOB/bytea)。
			// 直接更新主鍵 Id。Repo.BatUpdByCodeDict 會忽略 Id 欄位，不能用於主鍵變更。
			var stmts = new List<str>(normalized.Count);
			var arg = new Dictionary<str, obj?>();
			for(var i = 0; i < normalized.Count; i++){
				var (oldId, newId) = normalized[i];
				var pOld = TW.Prm($"old_{i}");
				var pNew = TW.Prm($"new_{i}");
				stmts.Add(
					$"UPDATE {TW.Qt(TW.DbTblName)} " +
					$"SET {TW.QtCol(nameof(PoWord.Id))} = {pNew} " +
					$"WHERE {TW.QtCol(nameof(PoWord.Id))} = {pOld}"
				);
				arg[pOld.Name] = TW.UpperToRaw(oldId, nameof(PoWord.Id));
				arg[pNew.Name] = TW.UpperToRaw(newId, nameof(PoWord.Id));
			}
			var sql = str.Join(";\n", stmts);
			var cmd = await SqlCmdMkr.Prepare(Ctx, sql, Ct);
			Ctx.AddToDispose(cmd);
			await cmd.RawArgs(arg).AsyE1d(Ct).FirstOrDefaultAsync(Ct);

			// step 2: 根主鍵改完後立刻搬資產——此刻資產外鍵仍指向 Old，必須跟進到新 Id。
			// step 3: 觸碰新 Id 的 BizUpdatedAt(遷移視同變更；Old 行已改名、無從觸碰)。
			await BatMoveAssetsToWordId(Ctx, ToolAsyE.ToAsyE(normalized), Ct);
			await BatAltWordAfterUpd(Ctx, ToolAsyE.ToAsyE(normalized.Select(x=>x.New)), Ct);
			return NIL;
		});
		await batch.ConsumeAll(Moves, Ct);
		return NIL;
	}

	public IAsyncEnumerable<JnWord> GetWordsByOwner(
		IDbFnCtx Ctx
		,IdUser Owner
		,CT Ct
	){
		var Sql = TW.SqlSplicer().Select("*").From().WhereNonDel()
		.AndEq(x=>x.Owner, y=>y.One(Owner))
		.OrderByDesc(x=>x.BizCreatedAt)
		;
		var PoWords = SqlCmdMkr.RunDupliSql(Ctx, TW, Sql, Ct)
		.Where(x=>x is not null)
		.Select(x=>x!)
		;

		return CollectJnWordsByBatch(Ctx, PoWords, Ct);
	}

	/// 按詞頭搜索單詞；匹配規則由 IsExact 控制。
	/// 搜索、排序、分頁都在數據庫層完成，避免把整個詞庫拉到服務層後再裁切。
	public async Task<IPageAsyE<JnWord>> PageWordsByOwnerAndHead(
		IDbFnCtx Ctx,
		IdUser Owner,
		IPageQry PageQry,
		str Head,
		bool IsExact,
		CT Ct
	){
		var normalized = (Head ?? "").Trim();
		if(normalized == ""){
			return PageQry.ToPageAsyE(
				ToolAsyE.ToAsyE(Array.Empty<JnWord>()),
				HasTotCnt: true,
				TotCnt: 0
			);
		}

		var pOwner = TW.Prm("Owner");
		var pHead = TW.Prm("Head");
		var whereHead = IsExact
			? $"{TW.QtCol(nameof(PoWord.Head))} = {pHead}"
			: $"{TW.QtCol(nameof(PoWord.Head))} LIKE {pHead} || '%'";
		var countAlias = "TotCnt";
		var countSql =
$"""
SELECT COUNT(1) AS {TW.Qt(countAlias)}
FROM {TW.Qt(TW.DbTblName)}
WHERE 1=1
AND {TW.SqlIsNonDel()}
AND {TW.QtCol(nameof(PoWord.Owner))} = {pOwner}
AND {whereHead}
""";
		var pageSql =
$"""
SELECT *
FROM {TW.Qt(TW.DbTblName)}
WHERE 1=1
AND {TW.SqlIsNonDel()}
AND {TW.QtCol(nameof(PoWord.Owner))} = {pOwner}
AND {whereHead}
ORDER BY {TW.QtCol(nameof(PoWord.Head))} ASC, {TW.QtCol(nameof(PoWord.Id))} ASC
{TW.SqlMkr.ParamLimOfst(out var pLim, out var pOfst)}
""";
		var arg = ArgDict.Mk(TW)
			.AddT(pOwner, Owner)
			.AddRaw(pHead, normalized)
			.AddPageQry(PageQry, pLim, pOfst);

		var countCmd = await SqlCmdMkr.Prepare(Ctx, countSql, Ct);
		Ctx.AddToDispose(countCmd);
		var totalCount = await GetCount(countCmd, arg, countAlias, Ct);
		if(totalCount == 0){
			return PageQry.ToPageAsyE(
				ToolAsyE.ToAsyE(Array.Empty<JnWord>()),
				HasTotCnt: true,
				TotCnt: 0
			);
		}

		var pageCmd = await SqlCmdMkr.Prepare(Ctx, pageSql, Ct);
		Ctx.AddToDispose(pageCmd);
		var poWords = Ctx.RunCmd(pageCmd, arg).AsyE1d(Ct)
			.Select(x=>TW.DbDictToEntity<PoWord>(x))
			.Where(x=>x is not null)
			.Select(x=>x!);
		var words = CollectJnWordsByBatch(Ctx, poWords, Ct);
		return PageQry.ToPageAsyE(words, HasTotCnt: true, TotCnt: totalCount);
	}

	/// 按詞頭搜尋，並保留供新搜尋流程使用的優先排序 API。
	/// 既有 PageWordsByOwnerAndHead 的名稱、參數與行為不受影響。
	public async Task<IPageAsyE<JnWord>> PageWordsByOwnerAndHeadPriority(
		IDbFnCtx Ctx,
		IdUser Owner,
		IPageQry PageQry,
		str Head,
		CT Ct
	){
		return await PageWordsByOwnerAndHead(Ctx, Owner, PageQry, Head, false, Ct);
	}

	public async IAsyncEnumerable<JnWord> GetWordsByOwnerByPreFilterSql(
		IDbFnCtx Ctx,
		IdUser Owner,
		PreFilter? PreFilter,
		[EnumeratorCancellation] CT Ct
	){
		if(PreFilter is null){
			await foreach(var word in GetWordsByOwner(Ctx, Owner, Ct).WithCancellation(Ct)){
				yield return word;
			}
			yield break;
		}

		var candidateIds = await QueryWordIdsByCoreFilter(Ctx, Owner, PreFilter, Ct);
		if(candidateIds.Count == 0){
			yield break;
		}

		var matchedIds = await FilterWordIdsByPropFilter(Ctx, candidateIds, PreFilter, Ct);
		if(matchedIds.Count == 0){
			yield break;
		}

		var words = RepoWord.OrdGetAggById<JnWord>(Ctx, ToolAsyE.ToAsyE(matchedIds), Ct);
		await foreach(var word in words.WithCancellation(Ct)){
			if(word is null){
				continue;
			}
			yield return word;
		}
	}

	async Task<List<IdWord>> QueryWordIdsByCoreFilter(
		IDbFnCtx Ctx,
		IdUser Owner,
		PreFilter PreFilter,
		CT Ct
	){
		var coreFilterSql = PreFilterSqlMkr.BuildCoreFilterWhere(TW, Owner, PreFilter);

		var sql =
$"""
SELECT {TW.QtCol(nameof(PoWord.Id))}
FROM {TW.Qt(TW.DbTblName)}
WHERE {coreFilterSql.WhereSql}
ORDER BY {TW.QtCol(nameof(PoWord.BizCreatedAt))} DESC
""";
		var cmd = await SqlCmdMkr.Prepare(Ctx, sql, Ct);
		Ctx.AddToDispose(cmd);
		var rows = Ctx.RunCmd(cmd, coreFilterSql.Arg).AsyE1d(Ct);

		var ids = new List<IdWord>();
		await foreach(var row in rows){
			if(!TryGetWordId(row, out var id)){
				continue;
			}
			ids.Add(id);
		}
		return ids;
	}

	async Task<List<IdWord>> FilterWordIdsByPropFilter(
		IDbFnCtx Ctx,
		IReadOnlyList<IdWord> CandidateIds,
		PreFilter PreFilter,
		CT Ct
	){
		if(PreFilter.PropFilter.Count == 0){
			return CandidateIds.ToList();
		}
		if(CandidateIds.Count == 0){
			return [];
		}

		var opt = new OptQry{
			InParamCnt = (u64)CandidateIds.Count,
			IncludeDeleted = false,
		};
		var propsById = await RepoWord.IncludeEntitysByKeys(
			Ctx,
			nameof(I_WordId.WordId),
			opt,
			CandidateIds,
			x=>x.WordId,
			TP,
			Ct
		);

		var r = new List<IdWord>(CandidateIds.Count);
		foreach(var id in CandidateIds){
			var props = propsById.GetValueOrDefault(id, []);
			if(!IsMatchedByPropFilters(props, PreFilter.PropFilter)){
				continue;
			}
			r.Add(id);
		}
		return r;
	}

	bool TryGetWordId(IStr_Obj Raw, out IdWord Id){
		var dbCol = TW.DbCol(x=>x.Id);
		if(!Raw.TryGetValue(dbCol, out var rawV) && !Raw.TryGetValue(nameof(PoWord.Id), out rawV)){
			Id = default;
			return false;
		}
		var upper = TW.RawToUpper(rawV, nameof(PoWord.Id));
		if(upper is IdWord id){
			Id = id;
			return true;
		}
		Id = default;
		return false;
	}

	static bool IsMatchedByPropFilters(IList<PoWordProp> Props, IList<FieldsFilter> FieldFilters){
		foreach(var ff in FieldFilters){
			if(IsMatchedByOnePropFieldsFilter(Props, ff)){
				continue;
			}
			return false;
		}
		return true;
	}

	static bool IsMatchedByOnePropFieldsFilter(IList<PoWordProp> Props, FieldsFilter FieldFilter){
		if(FieldFilter.Fields.Count == 0){
			return true;
		}
		foreach(var field in FieldFilter.Fields){
			var values = Props.Where(x=>x.KStr == field).Select(WordFilterValueUtil.GetPropValue).ToList();
			if(values.Count == 0){
				continue;
			}
			if(IsMatchedByPropFilterItems(values, FieldFilter.Filters)){
				return true;
			}
		}
		return false;
	}

	static bool IsMatchedByPropFilterItems(IReadOnlyList<obj?> CandidateValues, IList<FilterItem> FilterItems){
		foreach(var f in FilterItems){
			if(IsMatchedByOnePropFilterItem(CandidateValues, f)){
				continue;
			}
			return false;
		}
		return true;
	}

	static bool IsMatchedByOnePropFilterItem(IReadOnlyList<obj?> CandidateValues, FilterItem FilterItem){
		var filterValues = FilterItem.Values ?? [];
		return FilterItem.Operation switch{
			EFilterOperationMode.IncludeAny => filterValues.Any(v=>CandidateValues.Any(c=>WordFilterValueUtil.AreEqual(c, v, FilterItem.ValueType))),
			EFilterOperationMode.IncludeAll => filterValues.All(v=>CandidateValues.Any(c=>WordFilterValueUtil.AreEqual(c, v, FilterItem.ValueType))),
			EFilterOperationMode.ExcludeAll => filterValues.All(v=>CandidateValues.All(c=>!WordFilterValueUtil.AreEqual(c, v, FilterItem.ValueType))),
			EFilterOperationMode.Eq => CandidateValues.Any(c=>WordFilterValueUtil.AreEqual(c, filterValues.FirstOrDefault(), FilterItem.ValueType)),
			EFilterOperationMode.Ne => CandidateValues.All(c=>!WordFilterValueUtil.AreEqual(c, filterValues.FirstOrDefault(), FilterItem.ValueType)),
			EFilterOperationMode.Gt => CandidateValues.Any(c=>WordFilterValueUtil.CompareNumber(c, filterValues.FirstOrDefault()) > 0),
			EFilterOperationMode.Ge => CandidateValues.Any(c=>WordFilterValueUtil.CompareNumber(c, filterValues.FirstOrDefault()) >= 0),
			EFilterOperationMode.Lt => CandidateValues.Any(c=>WordFilterValueUtil.CompareNumber(c, filterValues.FirstOrDefault()) < 0),
			EFilterOperationMode.Le => CandidateValues.Any(c=>WordFilterValueUtil.CompareNumber(c, filterValues.FirstOrDefault()) <= 0),
			_ => true,
		};
	}

	IAsyncEnumerable<JnWord> CollectJnWordsByBatch(
		IDbFnCtx Ctx
		,IAsyncEnumerable<PoWord> PoWords
		,CT Ct
	){
		var Batch = new BatchCollector<PoWord, IList<JnWord>>(async(words, Ct)=>{
			var Ids = words.Select(x=>x.Id).ToList();
			if(Ids.Count == 0){
				return [];
			}
			var NWordId = nameof(I_WordId.WordId);
			var Opt = new OptQry{
				InParamCnt = (u64)Ids.Count,
				IncludeDeleted = false,
			};
			var PropsById = await RepoWord.IncludeEntitysByKeys(Ctx, NWordId, Opt, Ids, x=>x.WordId, TP, Ct);
			var LearnsById = await RepoWord.IncludeEntitysByKeys(Ctx, NWordId, Opt, Ids, x=>x.WordId, TL, Ct);

			IList<JnWord> R = [];
			foreach(var PoWord in words){
				var Props = PropsById.GetValueOrDefault(PoWord.Id, []);
				var Learns = LearnsById.GetValueOrDefault(PoWord.Id, []);
				R.Add(new JnWord(PoWord, Props, Learns));
			}
			return R;
		});
		var S2D = Batch.AddToEnd(PoWords, Ct);
		return S2D.Flat();
	}

	async Task<u64> GetCount(
		ISqlCmd Cmd,
		IArgDict Arg,
		str CountAlias,
		CT Ct
	){
		var row = await Cmd.Args(Arg).AsyE1d(Ct).FirstOrDefaultAsync(Ct);
		if(row is null){
			return 0;
		}
		if(row.TryGetValue(CountAlias, out var value) && value is not null){
			return Convert.ToUInt64(value);
		}
		var lower = CountAlias.ToLowerInvariant();
		if(row.TryGetValue(lower, out value) && value is not null){
			return Convert.ToUInt64(value);
		}
		return 0;
	}
}
