namespace Ngaq.Core.Shared.Word.Svc;

using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Shared.Sync;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Learn;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Tsinswreng.CsErr;
using Tsinswreng.CsTempus;

/// 單詞同步的純內存實現，不直接操作數據庫。
public class SvcWordInMem:ISvcWordInMem{

	/// 比較兩個屬性資產；僅當 Remote 更新時返回需要寫回的實體。
	public DtoEntityDiffEtSync<PoWordProp> SyncProp(PoWordProp Local, PoWordProp Remote){
		IEntitySyncerInMem<PoWordProp> syncer = new EntitySyncerInMem<PoWordProp>();
		var diff = syncer.DiffPoByTime(Local, Remote);
		return new DtoEntityDiffEtSync<PoWordProp>{
			LocalCompareToRemote = diff,
			SyncedEntity = diff < 0 ? Remote : null,
		};
	}

	/// 比較兩個學習記錄資產；僅當 Remote 更新時返回需要寫回的實體。
	public DtoEntityDiffEtSync<PoWordLearn> SyncLearn(PoWordLearn Local, PoWordLearn Remote){
		IEntitySyncerInMem<PoWordLearn> syncer = new EntitySyncerInMem<PoWordLearn>();
		var diff = syncer.DiffPoByTime(Local, Remote);
		return new DtoEntityDiffEtSync<PoWordLearn>{
			LocalCompareToRemote = diff,
			SyncedEntity = diff < 0 ? Remote : null,
		};
	}

	/// 比較聚合根 PoWord；僅當 Remote 更新時返回需要寫回的實體。
	public DtoEntityDiffEtSync<PoWord> SyncPoWord(PoWord Local, PoWord Remote){
		if(Local.IsSameUserWord(Remote) == false){
			throw KeysErr.Word.__And__IsNotSameUserWord.ToErr(Local.Id, Remote.Id);
		}
		if(Local.Id != Remote.Id){
			var keep = Local.BizCreatedAt <= Remote.BizCreatedAt ? Local : Remote;
			return new DtoEntityDiffEtSync<PoWord>{
				LocalCompareToRemote = Local.BizCreatedAt.CompareTo(Remote.BizCreatedAt),
				SyncedEntity = keep,
			};
		}
		IEntitySyncerInMem<PoWord> syncer = new EntitySyncerInMem<PoWord>();
		var diff = syncer.DiffPoByTime(Local, Remote);
		return new DtoEntityDiffEtSync<PoWord>{
			LocalCompareToRemote = diff,
			SyncedEntity = diff < 0 ? Remote : null,
		};
	}

	/// 計算 Remote 合入 Local 的同步結果，包含差異分類與待寫回內容。
	public DtoJnWordSyncResult SyncJnWord(JnWord? Local, JnWord Remote){
		var r = new DtoJnWordSyncResult{
			Local = Local,
			Remote = Remote,
		};
		if(Local is null){
			r.DiffResult = EDiffByBizIdResultForSync.LocalNotExist;
			r.SyncedRoot = Remote;
			return r;
		}

		var diff = ((ISvcWordInMem)this).CompareJnWord(Local, Remote);
		r.DiffResult = diff;
		if(diff == EDiffByBizIdResultForSync.NoChange || diff == EDiffByBizIdResultForSync.RemoteIsOlder){
			return r;
		}

		var (newProps, newLearns, changedProps, changedLearns) = DiffAssets(Local, Remote);
		var rootSync = SyncPoWord(Local.Word, Remote.Word);
		var diffCase = new AggDiffCaseForSync{
			RemoteHasNewAssets = newProps.Count > 0 || newLearns.Count > 0,
			RemoteHasChangedAssets = changedProps.Count > 0 || changedLearns.Count > 0,
			RemoteIsSoftDeleted = Remote.DelAt.Value != 0,
			LocalIsSoftDeleted = Local.DelAt.Value != 0,
			AggRootIsChanged = rootSync.SyncedEntity is not null,
		};
		r.DiffCase = diffCase;

		r.NewAssets = new JnWord{
			Word = Local.Word,
			Props = newProps,
			Learns = newLearns,
		};
		r.ChangedAssets = new JnWord{
			Word = Local.Word,
			Props = changedProps,
			Learns = changedLearns,
		};
		if(rootSync.SyncedEntity is not null){
			r.SyncedRoot = new JnWord{
				Word = rootSync.SyncedEntity,
				Props = [],
				Learns = [],
			};
		}
		return r;
	}

	/// 對比資產列表，拆分爲「Remote 新增」與「Remote 變更」兩部分。
	(
		List<PoWordProp> NewProps,
		List<PoWordLearn> NewLearns,
		List<PoWordProp> ChangedProps,
		List<PoWordLearn> ChangedLearns
	) DiffAssets(JnWord Local, JnWord Remote){
		var newProps = new List<PoWordProp>();
		var newLearns = new List<PoWordLearn>();
		var changedProps = new List<PoWordProp>();
		var changedLearns = new List<PoWordLearn>();

		var localPropById = Local.Props.ToDictionary(x=>x.Id, x=>x);
		foreach(var remoteProp in Remote.Props){
			if(localPropById.TryGetValue(remoteProp.Id, out var localProp)){
				var diff = SyncProp(localProp, remoteProp);
				if(diff.SyncedEntity is not null){
					changedProps.Add(diff.SyncedEntity);
				}
				continue;
			}
			newProps.Add(remoteProp);
		}

		var localLearnById = Local.Learns.ToDictionary(x=>x.Id, x=>x);
		foreach(var remoteLearn in Remote.Learns){
			if(localLearnById.TryGetValue(remoteLearn.Id, out var localLearn)){
				var diff = SyncLearn(localLearn, remoteLearn);
				if(diff.SyncedEntity is not null){
					changedLearns.Add(diff.SyncedEntity);
				}
				continue;
			}
			newLearns.Add(remoteLearn);
		}

		return (newProps, newLearns, changedProps, changedLearns);
	}

	public IJnWordMergeResult Merge(JnWord? Local, JnWord Remote) {
		var remoteClone = CloneJnWord(Remote);
		if(Local is null){
			remoteClone.EnsureForeignId();
			return new JnWordMergeResult{
				Result = EJnWordMergeResult.LocalNotExist,
				NewAssets = CloneJnWord(remoteClone),
				Merged = remoteClone,
			};
		}
		if(!Local.Word.IsSameUserWord(Remote.Word)){
			throw KeysErr.Word.__And__IsNotSameUserWord.ToErr(Local.Id, Remote.Id);
		}

		var localClone = CloneJnWord(Local);
		var localWasSoftDeleted = Local.DelAt.Value != 0;
		var localPropIds = new HashSet<Ngaq.Core.Model.Po.Kv.IdWordProp>(localClone.Props.Select(x=>x.Id));
		var localLearnIds = new HashSet<Ngaq.Core.Model.Po.Learn_.IdWordLearn>(localClone.Learns.Select(x=>x.Id));
		var newProps = new List<PoWordProp>();
		var newLearns = new List<PoWordLearn>();

		foreach(var remoteProp in Remote.Props){
			if(localPropIds.Contains(remoteProp.Id)){
				continue;
			}
			var neoProp = CloneProp(remoteProp, localClone.Id);
			localClone.Props.Add(neoProp);
			newProps.Add(neoProp);
		}
		foreach(var remoteLearn in Remote.Learns){
			if(localLearnIds.Contains(remoteLearn.Id)){
				continue;
			}
			var neoLearn = CloneLearn(remoteLearn, localClone.Id);
			localClone.Learns.Add(neoLearn);
			newLearns.Add(neoLearn);
		}

		var hasNewAssets = newProps.Count > 0 || newLearns.Count > 0;
		if(!hasNewAssets && !localWasSoftDeleted){
			return new JnWordMergeResult{
				Result = EJnWordMergeResult.NoChange,
				Merged = localClone,
			};
		}

		var mergedWord = CloneWord(localClone.Word);
		// 只恢復聚合根；資產本身保持原狀，避免把已刪資產誤恢復。
		if(localWasSoftDeleted){
			mergedWord.DelAt = default;
		}
		mergedWord.BizCreatedAt = mergedWord.BizCreatedAt <= Remote.BizCreatedAt
			? mergedWord.BizCreatedAt
			: Remote.BizCreatedAt;
		mergedWord.BizUpdatedAt = UnixMs.Now();
		localClone.Word = mergedWord;
		localClone.EnsureForeignId();
		return new JnWordMergeResult{
			Result = EJnWordMergeResult.Changed,
			NewAssets = hasNewAssets ? new JnWord{
				Word = CloneWord(localClone.Word),
				Props = newProps,
				Learns = newLearns,
			} : null,
			Merged = localClone,
		};
	}

	static JnWord CloneJnWord(JnWord Src){
		return new JnWord{
			Word = CloneWord(Src.Word),
			Props = Src.Props.Select(x=>CloneProp(x, Src.Id)).ToList(),
			Learns = Src.Learns.Select(x=>CloneLearn(x, Src.Id)).ToList(),
		};
	}

	static PoWord CloneWord(PoWord Src){
		return new PoWord{
			Id = Src.Id,
			Owner = Src.Owner,
			Head = Src.Head,
			Lang = Src.Lang,
			StoredAt = Src.StoredAt,
			DbCreatedAt = Src.DbCreatedAt,
			BizCreatedAt = Src.BizCreatedAt,
			BizUpdatedAt = Src.BizUpdatedAt,
			DbUpdatedAt = Src.DbUpdatedAt,
			DelAt = Src.DelAt,
		};
	}

	static PoWordProp CloneProp(PoWordProp Src, IdWord WordId){
		return new PoWordProp{
			Id = Src.Id,
			WordId = WordId,
			KType = Src.KType,
			KStr = Src.KStr,
			KI64 = Src.KI64,
			VType = Src.VType,
			VStr = Src.VStr,
			VI64 = Src.VI64,
			VF64 = Src.VF64,
			VBinary = Src.VBinary is null ? null : [..Src.VBinary],
			DbCreatedAt = Src.DbCreatedAt,
			BizCreatedAt = Src.BizCreatedAt,
			BizUpdatedAt = Src.BizUpdatedAt,
			DbUpdatedAt = Src.DbUpdatedAt,
			DelAt = Src.DelAt,
		};
	}

	static PoWordLearn CloneLearn(PoWordLearn Src, IdWord WordId){
		return new PoWordLearn{
			Id = Src.Id,
			WordId = WordId,
			LearnResult = Src.LearnResult,
			DbCreatedAt = Src.DbCreatedAt,
			BizCreatedAt = Src.BizCreatedAt,
			BizUpdatedAt = Src.BizUpdatedAt,
			DbUpdatedAt = Src.DbUpdatedAt,
			DelAt = Src.DelAt,
		};
	}
}
