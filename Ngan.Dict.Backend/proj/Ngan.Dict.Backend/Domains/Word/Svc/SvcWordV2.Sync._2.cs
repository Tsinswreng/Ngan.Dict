namespace Ngan.Dict.Backend.Domains.Word.Svc;

using Ngan.Dict.Backend.Domains.Word.Dao;
using Ngan.Dict.Core.Infra;
using Ngan.Dict.Core.Infra.Errors;
using Ngan.Dict.Core.Shared.Base.Models.Po;
using Ngan.Dict.Core.Shared.Sync;
using Ngan.Dict.Core.Shared.Word.Models;
using Ngan.Dict.Core.Shared.Word.Models.Dto;
using Ngan.Dict.Core.Shared.Word.Models.Po.Word;
using Ngan.Dict.Core.Shared.Word.Svc;
using Tsinswreng.CsErr;
using Tsinswreng.CsSql;

/// OrdSyncJnWordByBizId 整條鏈路的 `_2` 版(2026-08-31 批原語方向的原型)。
/// 與原鏈路的對照:批編排從 BatchCollector 手搓換成 SqlFlow.Batches;批內兩次批量查
/// 直接收/回 IList;五分支直接吃已物化的批、不再各自包 BatchCollector;事務邊界
/// 仍由 OrdSyncByDto_2 顯式聲明(EnsureTxn 可嵌套,分支加入外層事務)。
/// 實驗性質、與原鏈路並存;不滿意可整文件刪除回滾。
public partial class SvcWordV2 : ISvcWordV2_2, ISvcWordSync_2
{
	/// BizId 同步主入口的 `_2` 版:把入參 JnWord 流(Remote)合入當前用戶詞庫(Local)。
	/// 只聲明「批內要做什麼」(歸一化 → 查根/查聚合、位置對齊 → 算差異 → 整批落庫),
	/// 批的切分/尾批/攤平由 SqlFlow.Batches 承擔,空間 O(批大小)、與總數 n 無關。
	/// 出參流與入參流位置一一對應、Dto 兼作審計憑據。
	/// 事務:由 OrdSyncByDto_2 聲明「本批一個事務」。
	/// <param name="Ctx">用戶上下文(含惰性創建的 DbFnCtx)</param>
	/// <param name="JnWords">遠端單詞流</param>
	/// <param name="Ct">取消令牌</param>
	public IAsyncEnumerable<DtoJnWordSyncResult> OrdSyncJnWordByBizId_2(
		IDbUserCtx Ctx,
		IAsyncEnumerable<JnWord> JnWords,
		CT Ct
	){
		// 唯一惰性創建的 DbFnCtx:讀與(經 OrdSyncByDto_2 的)寫共用它;無外層事務時每批 EnsureTxn 就地開新事務。
		var DbCtx = Ctx.DbFnCtx ??= new DbFnCtx();
		return SqlFlow.Batches<JnWord, DtoJnWordSyncResult>(JnWords, async(remotes, Ct)=>{
			// step 1: 歸屬歸一化——無論入參寫了誰、一律強制為當前用戶;資產外鍵對齊根的 Id。
			foreach(var remote in remotes){
				remote.Owner = Ctx.UserCtx.UserId;
				remote.EnsureForeignId();
			}

			// step 2: 按 BizId(Owner+Head+Lang)批量查 Local 聚合根(含軟刪)。
			// 批原語版直接收 IList、回 IList<PoWord?>、位置一一對應、查無補 null——批內不再 ToListAsync。
			var headLangs = new List<Head_Lang>(remotes.Count);
			foreach(var r in remotes){
				headLangs.Add(new Head_Lang(r.Head, r.Lang));
			}
			var localPos = await DaoWordV2.BatGetPoWordByOwnerHeadLangWithDel_2(
				DbCtx, Ctx.UserCtx.UserId, headLangs, Ct
			);

			// step 3: 拿命中的根 Id 批量回讀完整聚合(根+Props+Learns、含軟刪),建「根 Id → 聚合」索引。
			// Ord 語義:両列表位置對齊,故 localPos[i] 即 remotes[i] 的舊詞。
			var ids = localPos.Where(x=>x is not null).Select(x=>x!.Id).Distinct().ToList();
			var localById = new Dictionary<IdWord, JnWord?>();
			if(ids.Count > 0){
				var locals = await DaoWordV2.BatGetJnWordByIdWithDel_2(DbCtx, ids, Ct);
				for(var i = 0; i < ids.Count; i++){
					localById[ids[i]] = locals[i];
				}
			}

			// step 4: 逐條算差異(純內存、不落庫),產出 EDiffByBizIdResultForSync 五類分類的 Dto。
			// 邊界:localPo 命中但聚合取不到(理論上不該發生)時按 local=null 處理、即視為 LocalNotExist。
			var dtos = new List<DtoJnWordSyncResult>(remotes.Count);
			for(var i = 0; i < remotes.Count; i++){
				var localPo = localPos[i];
				JnWord? local = null;
				if(localPo is not null){
					local = localById.GetValueOrDefault(localPo.Id);
				}
				dtos.Add(SvcWordInMem.SyncJnWord(local, remotes[i]));
			}

			// step 5: 整批落庫——本批所有分支的寫入共用一個事務(OrdSyncByDto_2 內部 EnsureTxn)。
			await OrdSyncByDto_2(Ctx, dtos, Ct);
			return dtos;
		}, Ct);
	}

	/// 分桶派發器的 `_2` 版:入參已是批邊界物化好的 IList,直接按 DiffResult 歸入五桶、
	/// 逐桶交給 ISvcWordSync_2 對應處理器。相比原版:不再需要先完整消費一次入參流,也不再「批中批」。
	/// 事務邊界:EnsureTxn 包住全部五次調用——本批所有分支要麼全成、要麼全 rollback;
	/// 分支內部的 EnsureTxn 是加入式(檢測到已有外層事務就不另開),保證直接測試時也安全。
	/// 調用順序固定:NoChange → RemoteIsOlder → LocalNotExist → IdNotEqual → RemoteIsNewer
	/// (IdNotEqual 改主鍵、與 RemoteIsNewer 按 Id 更新存在先後依賴,勿調整)。
	/// <param name="Ctx">用戶上下文</param>
	/// <param name="Dtos">已物化的一批同步結果 Dto</param>
	/// <param name="Ct">取消令牌</param>
	public Task<nil> OrdSyncByDto_2(IDbUserCtx Ctx, IList<DtoJnWordSyncResult> Dtos, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			// localCtx:把 EnsureTxn 準備好的 DbCtx 與用戶上下文重組為 IDbUserCtx 傳給處理器。
			var localCtx = new DbUserCtx(Ctx.UserCtx, DbCtx);
			var noChange = new List<DtoJnWordSyncResult>();
			var remoteOlder = new List<DtoJnWordSyncResult>();
			var localNotExist = new List<DtoJnWordSyncResult>();
			var idNotEqual = new List<DtoJnWordSyncResult>();
			var remoteIsNewer = new List<DtoJnWordSyncResult>();

			// step 1: 按分類把整批 Dto 落入五桶(未知分類被丟棄,目前枚舉無此情況)。
			foreach(var dto in Dtos){
				if(dto.DiffResult == EDiffByBizIdResultForSync.NoChange){
					noChange.Add(dto);
				}else if(dto.DiffResult == EDiffByBizIdResultForSync.RemoteIsOlder){
					remoteOlder.Add(dto);
				}else if(dto.DiffResult == EDiffByBizIdResultForSync.LocalNotExist){
					localNotExist.Add(dto);
				}else if(dto.DiffResult == EDiffByBizIdResultForSync.IdNotEqual){
					idNotEqual.Add(dto);
				}else if(dto.DiffResult == EDiffByBizIdResultForSync.RemoteIsNewer){
					remoteIsNewer.Add(dto);
				}
			}

			// step 2: 逐桶落庫,全在一步事務內。
			await OrdSync_NoChange_2(localCtx, noChange, Ct);
			await OrdSync_RemoteIsOlder_2(localCtx, remoteOlder, Ct);
			await OrdSync_LocalNotExist_2(localCtx, localNotExist, Ct);
			await OrdSync_IdNotEqual_2(localCtx, idNotEqual, Ct);
			await BatSync_RemoteIsNewer_2(localCtx, remoteIsNewer, Ct);
			return NIL;
		});
	}

	/// NoChange 分支的 `_2` 版:兩端完全一致、無任何寫庫(入參已是 IList、無需消費流)。
	public Task<nil> OrdSync_NoChange_2(IDbUserCtx Ctx, IList<DtoJnWordSyncResult> Dtos, CT Ct){
		return Task.FromResult(NIL);
	}

	/// RemoteIsOlder 分支的 `_2` 版:Local 較新、保持不動(入參已是 IList、無需消費流)。
	public Task<nil> OrdSync_RemoteIsOlder_2(IDbUserCtx Ctx, IList<DtoJnWordSyncResult> Dtos, CT Ct){
		return Task.FromResult(NIL);
	}

	/// LocalNotExist 分支的 `_2` 版:本用戶詞庫無此 BizId 之詞、Remote 是全新詞。
	/// 前置保證 Owner/外鍵已歸一化後、OrdAddAgg 聚合級聯插入三表、一次同構批量完成。
	/// 直接測試時本方法自帶事務,經 OrdSyncByDto_2 調用時加入外層事務(嵌套 EnsureTxn)。
	public Task<nil> OrdSync_LocalNotExist_2(IDbUserCtx Ctx, IList<DtoJnWordSyncResult> Dtos, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			var neos = new List<JnWord>(Dtos.Count);
			foreach(var dto in Dtos){
				var remote = dto.Remote ?? throw KeysErr.Word.Word__And__SyncFailed.ToErr("RemoteNull");
				remote.Owner = Ctx.UserCtx.UserId;
				remote.EnsureForeignId();
				neos.Add(remote);
			}
			if(neos.Count > 0){
				await RepoWord.OrdAddAgg<JnWord>(DbCtx, ToAsyE(neos), Ct);
			}
			return NIL;
		});
	}

	/// IdNotEqual 分支的 `_2` 版:BizId 相同、Id 不同——兩端各自獨立添加過同 BizId 的詞。
	/// 收斂規則(與原版一致):以 Id 較小者為基準 Id;Remote 資產整體改掛到基準 Id 下、直接 upsert 併入。
	/// 相比原版:入參已是 IList,批內行對處理直接出結果、不再包 BatchCollector。
	public Task<nil> OrdSync_IdNotEqual_2(IDbUserCtx Ctx, IList<DtoJnWordSyncResult> Dtos, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			// step 1: 批內逐行成對處理(local↔remote 同下標),定基準 Id、收集主鍵遷移與待寫回聚合。
			var moveIds = new List<(IdWord Old, IdWord New)>();
			var remotesToApply = new List<JnWord>(Dtos.Count);
			foreach(var dto in Dtos){
				// 前置校驗:分類為 IdNotEqual 卻缺邊界數據 = 上游計算錯誤,直接拋。
				var local = dto.Local ?? throw KeysErr.Word.Word__And__SyncFailed.ToErr("LocalNull");
				var remote = dto.Remote ?? throw KeysErr.Word.Word__And__SyncFailed.ToErr("RemoteNull");
				local.Word.CheckOwner(Ctx.UserCtx.UserId);
				remote.Owner = Ctx.UserCtx.UserId;
				remote.EnsureForeignId();

				// 定基準 Id;若庫中行的主鍵(local.Id)不是基準 Id、記入遷移清單待改主鍵。
				var keepId = local.Id.Value <= remote.Id.Value ? local.Id : remote.Id;
				if(local.Id != keepId){
					moveIds.Add((local.Id, keepId));
				}
				// 把 Remote 聚合整體改掛到基準 Id 下(改根主鍵+全部資產外鍵)。
				remote.SetIdEtEnsureFKey(keepId);
				remotesToApply.Add(remote);
			}
			// step 2: 先遷主鍵(連同庫內既有資產外鍵一併搬移),必須先於 step 3——
			// 否則 OrdUpd 按 keepId 更新會找不到行/找錯行。
			if(moveIds.Count > 0){
				await OrdChangeId(new DbUserCtx(Ctx.UserCtx, DbCtx), ToAsyE(moveIds), Ct);
			}
			// step 3: 以 Remote 為新版本整批落庫(根覆蓋、資產 upsert;收尾統一觸碰 BizUpdatedAt)。
			if(remotesToApply.Count > 0){
				await ApplyRemoteWordsAsBatchUpdate(DbCtx, remotesToApply, Ct);
			}
			return NIL;
		});
	}

	/// RemoteIsNewer 分支的 `_2` 版:Id 相同、Remote 時間更新,直接把 Remote 整批作為新版本落庫。
	/// 與 IdNotEqual 的區別:主鍵未變、無需 OrdChangeId 遷移環節。
	/// 原版為類私有成員(不在 ISvcWordSync 接口),`_2` 版保持同級私有。
	Task<nil> BatSync_RemoteIsNewer_2(IDbUserCtx Ctx, IList<DtoJnWordSyncResult> Dtos, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			var remotes = new List<JnWord>(Dtos.Count);
			foreach(var dto in Dtos){
				var remote = dto.Remote ?? throw KeysErr.Word.Word__And__SyncFailed.ToErr("RemoteNull");
				remote.Owner = Ctx.UserCtx.UserId;
				remote.EnsureForeignId();
				remotes.Add(remote);
			}
			if(remotes.Count > 0){
				await ApplyRemoteWordsAsBatchUpdate(DbCtx, remotes, Ct);
			}
			return NIL;
		});
	}
}