namespace Ngan.Dict.Backend.Domains.Word.Svc;

using System.Runtime.CompilerServices;
using Ngan.Dict.Core.Frontend.Kv;
using Ngan.Dict.Core.Infra;
using Ngan.Dict.Core.Infra.Errors;
using Ngan.Dict.Core.Model.Po.Kv;
using Ngan.Dict.Core.Model.Po.Learn_;
using Ngan.Dict.Core.Tools;
using Ngan.Dict.Core.Shared.Sync;
using Ngan.Dict.Core.Shared.StudyPlan.Models.PreFilter;
using Ngan.Dict.Core.Shared.StudyPlan.Models.Po.PreFilter;
using Ngan.Dict.Core.Shared.StudyPlan.Models.Po.StudyPlan;
using Ngan.Dict.Core.Shared.StudyPlan.Svc;
using Ngan.Dict.Core.Shared.User.Models.Po.User;
using Ngan.Dict.Core.Shared.Word.Models;
using Ngan.Dict.Core.Shared.Word.Models.Learn_;
using Ngan.Dict.Core.Shared.Word.Models.Po.Kv;
using Ngan.Dict.Core.Shared.Word.Models.Po.Learn;
using Ngan.Dict.Core.Shared.Word.Models.Po.Word;
using Ngan.Dict.Core.Shared.Word.Svc;
using Ngan.Dict.Backend.Db.TswG;
using Tsinswreng.CsErr;
using Tsinswreng.CsCore;
using Tsinswreng.CsSql;
using Tsinswreng.CsTools;
using Ngan.Dict.Core.Tools.Json;
using Ngan.Dict.Core.Shared.Dictionary.Models;
using Ngan.Dict.Core.Shared.Word.Models.Dto;
using Ngan.Dict.Core.Shared.Base.Models.Po;
using Ngan.Dict.Backend.Domains.Word;
using Ngan.Dict.Backend.Domains.Word.Dao;
using Tsinswreng.CsTempus;

public partial class SvcWordV2
{
	/// 先按 BizId(Owner+Head+Lang) 查庫，再計算 Remote 合入 Local 的結果。純讀、不落庫、無事務。
	/// ■ 流式版(對外入口):SqlFlow.Batches 切批逐批調 IList 批核心、按批順序攤平輸出。
	public IAsyncEnumerable<IJnWordMergeResult> GetWordMergeResult(
		IDbUserCtx Ctx,
		IAsyncEnumerable<JnWord> Words,
		CT Ct
	){
		return SqlFlow.Batches<JnWord, IJnWordMergeResult>(
			Words, async(batch, ct2)=> (IEnumerable<IJnWordMergeResult>)(await GetWordMergeResult(Ctx, batch, ct2)), Ct
		);
	}

	/// ■ IList 批核心:函數只管一批——歸一化 → 查根/查聚合、位置對齊 → 逐條算合併結果。
	public async Task<IList<IJnWordMergeResult>> GetWordMergeResult(
		IDbUserCtx Ctx,
		IList<JnWord> Words,
		CT Ct
	){
		if(Words.Count == 0){
			return [];
		}

		// 純讀、無事務:惰性建裸 DbFnCtx(與原流式版一致),後續 Dao/Repo 直接使用。
		Ctx.DbFnCtx ??= new DbFnCtx();

		foreach(var remote in Words){
			remote.Owner = Ctx.UserCtx.UserId;
			remote.EnsureForeignId();
		}

		var headLangs = new List<Head_Lang>(Words.Count);
		foreach(var r in Words){
			headLangs.Add(new Head_Lang(r.Head, r.Lang));
		}
		var localPos = await DaoWordV2.BatGetPoWordByOwnerHeadLangWithDel(
			Ctx.DbFnCtx,
			Ctx.UserCtx.UserId,
			headLangs,
			Ct
		);
		var ids = localPos.Where(x=>x is not null).Select(x=>x!.Id).Distinct().ToList();
		var localById = new Dictionary<IdWord, JnWord?>();
		if(ids.Count > 0){
			var locals = await DaoWordV2.BatGetJnWordByIdWithDel(Ctx.DbFnCtx, ids, Ct);
			for(var i = 0; i < ids.Count; i++){
				localById[ids[i]] = locals[i];
			}
		}

		var ans = new List<IJnWordMergeResult>(Words.Count);
		for(var i = 0; i < Words.Count; i++){
			JnWord? local = null;
			var localPo = localPos[i];
			if(localPo is not null){
				local = localById.GetValueOrDefault(localPo.Id);
			}
			ans.Add(SvcWordInMem.Merge(local, Words[i]));
		}
		return ans;
	}

	/// 實際把合併結果落庫：新增整詞、或把新增資產追加到已有單詞。
	/// ■ 流式版(對外入口):整流一個事務,逐批調 IList 批核心。
	public Task<nil> MergeWord(IDbUserCtx Ctx, IAsyncEnumerable<IJnWordMergeResult> Words, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, DbCtx => {
			var localCtx = new DbUserCtx(Ctx.UserCtx, DbCtx);
			return SqlFlow.BatchesInOnly(Words, (batch, ct2)=> MergeWord(localCtx, batch, ct2), Ct);
		});
	}

	/// ■ IList 批核心:函數只管一批——五堆分流後各一次同構批量寫,整批一次事務。
	public Task<nil> MergeWord(IDbUserCtx Ctx, IList<IJnWordMergeResult> Words, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			if(Words.Count == 0){
				return NIL;
			}

			var neoAggs = new List<JnWord>();
			var updRoots = new List<PoWord>();
			var neoProps = new List<PoWordProp>();
			var neoLearns = new List<PoWordLearn>();
			var touchIds = new List<IdWord>();
			foreach(var item in Words){
				if(item.Result == EJnWordMergeResult.NoChange){
					continue;
				}
				var merged = item.Merged;
				merged.Owner = Ctx.UserCtx.UserId;
				merged.EnsureForeignId();
				if(item.Result == EJnWordMergeResult.LocalNotExist){
					neoAggs.Add(merged);
					continue;
				}

				updRoots.Add(merged.Word);
				var newAssets = item.NewAssets;
				if(newAssets is null){
					continue;
				}
				foreach(var p in newAssets.Props){
					p.WordId = merged.Id;
					neoProps.Add(p);
				}
				foreach(var l in newAssets.Learns){
					l.WordId = merged.Id;
					neoLearns.Add(l);
				}
				touchIds.Add(merged.Id);
			}

			if(neoAggs.Count > 0){
				await RepoWord.OrdAddAgg<JnWord>(DbCtx, neoAggs, Ct);
			}
			if(updRoots.Count > 0){
				await RepoWord.OrdUpd(DbCtx, updRoots, Ct);
			}
			if(neoProps.Count > 0){
				await RepoProp.OrdAdd(DbCtx, neoProps, Ct);
			}
			if(neoLearns.Count > 0){
				await RepoLearn.OrdAdd(DbCtx, neoLearns, Ct);
			}
			if(touchIds.Count > 0){
				await DaoWordV2.BatAltWordAfterUpd(DbCtx, touchIds.Distinct().ToList(), Ct);
			}
			return NIL;
		});
	}

	/// 先算合併結果，再把結果寫入數據庫。
	/// ■ 流式版(對外入口):整流一個事務,其內 GetWordMergeResult(純讀、批)後 MergeWord(批)合為一個事務。
	public Task<nil> MergeWord(IDbUserCtx Ctx, IAsyncEnumerable<JnWord> Words, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, DbCtx => {
			var localCtx = new DbUserCtx(Ctx.UserCtx, DbCtx);
			return SqlFlow.BatchesInOnly(Words, async(batch, ct2)=>{
				await MergeWord(localCtx, await GetWordMergeResult(localCtx, batch, ct2), ct2);
				return NIL;
			}, Ct);
		});
	}

	/// ■ IList 批核心:函數只管一批——先算結果(純讀)再整批落庫,整批一次事務(EnsureTxn 可嵌套)。
	public Task<nil> MergeWord(IDbUserCtx Ctx, IList<JnWord> Words, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			var localCtx = new DbUserCtx(Ctx.UserCtx, DbCtx);
			var mergeResults = await GetWordMergeResult(localCtx, Words, Ct);
			await MergeWord(localCtx, mergeResults, Ct);
			return NIL;
		});
	}

	/// 在 Merge 的基礎上，將「Remote 新增的 description」轉成 ELearn.Add 記錄。
	/// ■ 流式版(對外入口):整流一個事務,逐批調 IList 批核心。
	public Task<nil> MergeWord_NewDescrAsAdd(IDbUserCtx Ctx, IAsyncEnumerable<JnWord> Words, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, DbCtx => {
			var localCtx = new DbUserCtx(Ctx.UserCtx, DbCtx);
			return SqlFlow.BatchesInOnly(Words, (batch, ct2)=> MergeWord_NewDescrAsAdd(localCtx, batch, ct2), Ct);
		});
	}

	/// ■ IList 批核心:函數只管一批——先算結果、收集 description 轉成的 Add 學習記錄,再同批落庫,整批一次事務。
	public Task<nil> MergeWord_NewDescrAsAdd(IDbUserCtx Ctx, IList<JnWord> Words, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			var localCtx = new DbUserCtx(Ctx.UserCtx, DbCtx);
			var mergeResults = await GetWordMergeResult(localCtx, Words, Ct);
			var addLearns = new List<PoWordLearn>();
			foreach(var one in mergeResults){
				var wordId = one.Merged.Id;
				foreach(var p in one.NewAssets?.Props ?? []){
					if(!IsDescription(p)){
						continue;
					}
					addLearns.Add(MkAddLearn(wordId, p.BizCreatedAt));
				}
			}

			await MergeWord(localCtx, mergeResults, Ct);
			if(addLearns.Count > 0){
				await RepoLearn.OrdAdd(DbCtx, addLearns, Ct);
				await DaoWordV2.BatAltWordAfterUpd(
					DbCtx,
					addLearns.Select(x=>x.WordId).Distinct().ToList(),
					Ct
				);
			}
			return NIL;
		});
	}
}
