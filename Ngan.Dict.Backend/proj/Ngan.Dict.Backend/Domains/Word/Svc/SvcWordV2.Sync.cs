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
using Tsinswreng.CsTextWithBlob;

public partial class SvcWordV2
{
	/// BizId 同步主入口：把入參 JnWord 流(Remote) 合入當前用戶詞庫(Local)。
	/// 匹配基準是 BizId(Owner+Head+Lang)、而非主鍵 Id；Id 衝突由差異分類交給對應分支處理。
	/// 形態：流式入口 = 整流一個事務(EnsureTxn)包住 SqlFlow.Batches 切批循環、
	/// 逐批調 IList 批核心(其內 EnsureTxn 加入外層事務)；批大小/切批由庫原語承擔。
	/// 出參流與入參流位置一一對應(按批順序攤平)，Dto 同時作審計憑據返回。
	/// 事務：整個寫操作一個事務——全批要麼全成、要麼全 rollback。
	public IAsyncEnumerable<DtoJnWordSyncResult> OrdSyncJnWordByBizId(
		IDbUserCtx Ctx,
		IAsyncEnumerable<JnWord> JnWords,
		CT Ct
	){
		return Impl(Ct);

		async IAsyncEnumerable<DtoJnWordSyncResult> Impl([EnumeratorCancellation] CT Ct){
			var all = await SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
				var localCtx = new DbUserCtx(Ctx.UserCtx, DbCtx);
				var ans = new List<DtoJnWordSyncResult>();
				await foreach(var batchR in SqlFlow.Batches<JnWord, DtoJnWordSyncResult>(
					JnWords, async(batch, ct2)=> (IEnumerable<DtoJnWordSyncResult>)(await OrdSyncJnWordByBizId(localCtx, batch, ct2)), Ct
				).WithCancellation(Ct)){
					ans.AddRange(batchR);
				}
				return ans;
			});
			foreach(var one in all){
				yield return one;
			}
		}
	}

	/// IList 批核心：把入參一批 JnWord(Remote) 合入當前用戶詞庫(Local)。
	/// 只聲明「批內要做什麼」(歸一化 → 查根/查聚合、位置對齊 → 算差異 → 整批落庫)；
	/// 整批在一次事務內完成(EnsureTxn 可嵌套、有外層只加入)。
	/// 出參 IList 與入參位置一一對應、Dto 兼作審計憑據。
	public Task<IList<DtoJnWordSyncResult>> OrdSyncJnWordByBizId(
		IDbUserCtx Ctx, IList<JnWord> Remotes, CT Ct
	){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			// 批核心內部調用其他 Svc 時一率用「重組後的 localCtx」，
			// 讓其內部 EnsureTxn 偵測到外層事務(Txn 非空)而只加入不另開——否則 sqlite 不支援嵌套事務。
			var localCtx = new DbUserCtx(Ctx.UserCtx, DbCtx);
			if(Remotes.Count == 0){
				return (IList<DtoJnWordSyncResult>)new List<DtoJnWordSyncResult>();
			}

			// step 1: 歸屬歸一化——無論入參寫了誰，一律強制為當前用戶；資產外鍵對齊到根的 Id
			foreach(var remote in Remotes){
				remote.Owner = Ctx.UserCtx.UserId;
				remote.EnsureForeignId();
			}

			// step 2: 按 BizId 批量查 Local 聚合根(含軟刪)。
			// 底層與 (Owner,Head,Lang) 複合鍵等值批量查同構：結果與入參位置嚴格對齊、查無補 null，
			// 所以 localPos[i] 就是 remotes[i] 的舊詞。
			// 含軟刪的原因:已刪的行仍佔用 BizId，必須交差異計算(CompareJnWord 會比較 DelAt)判衝突。
			var headLangs = new List<Head_Lang>(Remotes.Count);
			foreach(var r in Remotes){
				headLangs.Add(new Head_Lang(r.Head, r.Lang));
			}
			var localPos = await DaoWordV2.BatGetPoWordByOwnerHeadLangWithDel(
				DbCtx,
				Ctx.UserCtx.UserId,
				headLangs,
				Ct
			);

			// step 3: 拿 step 2 命中的根 Id 批量回讀完整聚合(根+Props+Learns、含軟刪)。
			// Ord 語義:locals[i] 與 ids[i] 位置對應、查無為 null，
			// 故可用普通字典把「根 Id → 聚合」建索引供 step 4 逐條取用。
			var ids = localPos.Where(x=>x is not null).Select(x=>x!.Id).Distinct().ToList();
			var localById = new Dictionary<IdWord, JnWord?>();
			if(ids.Count > 0){
				var locals = await DaoWordV2.BatGetJnWordByIdWithDel(DbCtx, ids, Ct);
				for(var i = 0; i < ids.Count; i++){
					localById[ids[i]] = locals[i];
				}
			}

			// step 4: 逐條算差異(純內存、不落庫)，產出 EDiffByBizIdResultForSync 五類分類的 Dto。
			// 邊界：localPo 命中但聚合取不到(理論上不該發生)時按 local=null 處理、即視為 LocalNotExist。
			var dtos = new List<DtoJnWordSyncResult>(Remotes.Count);
			for(var i = 0; i < Remotes.Count; i++){
				var localPo = localPos[i];
				JnWord? local = null;
				if(localPo is not null){
					local = localById.GetValueOrDefault(localPo.Id);
				}
				dtos.Add(SvcWordInMem.SyncJnWord(local, Remotes[i]));
			}

			// step 5: 整批落庫。OrdSyncByDto 內部再按 DiffResult 分桶派發、全程共用一個事務。
			await OrdSyncByDto(localCtx, dtos, Ct);
			return (IList<DtoJnWordSyncResult>)dtos;
		});
	}

	/// 分桶派發器：把整批 Dto 按 DiffResult 歸入五桶、逐桶交給 ISvcWordSync 對應處理器。
	/// 事務邊界：EnsureTxn 包住全部五次調用——本批所有分支的寫入共用同一事務、要麼全成要麼全rollback。
	/// 調用順序固定：NoChange → RemoteIsOlder → LocalNotExist → IdNotEqual → RemoteIsNewer，
	/// 其中 IdNotEqual 會改主鍵、與後續 RemoteIsNewer 的按 Id 更新存在先後依賴，勿隨意調整。
	// ■ 流式版(對外入口):整流一個事務,逐批調 IList 批核心。
	public Task<nil> OrdSyncByDto(IDbUserCtx Ctx, IAsyncEnumerable<DtoJnWordSyncResult> Dtos, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, DbCtx => {
			var localCtx = new DbUserCtx(Ctx.UserCtx, DbCtx);
			return SqlFlow.BatchesInOnly(Dtos, (batch, ct2)=> OrdSyncByDto(localCtx, batch, ct2), Ct);
		});
	}

	// ■ IList 批核心:函數只管一批——直接按分類落五桶、逐桶一次處理;整批一次事務(EnsureTxn 可嵌套)。
	public Task<nil> OrdSyncByDto(IDbUserCtx Ctx, IList<DtoJnWordSyncResult> Dtos, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			// 批核心內部調分支時一率用「重組後的 localCtx」,
			// 分支自己的 EnsureTxn 才偵測得到外層事務(Txn 非空)而只加入不另開。
			var localCtx = new DbUserCtx(Ctx.UserCtx, DbCtx);
			var noChange = new List<DtoJnWordSyncResult>();
			var remoteOlder = new List<DtoJnWordSyncResult>();
			var localNotExist = new List<DtoJnWordSyncResult>();
			var idNotEqual = new List<DtoJnWordSyncResult>();
			var remoteIsNewer = new List<DtoJnWordSyncResult>();

			// step 1: 直接按分類落入五桶(未知分類會被丟棄、目前枚舉無此情況)
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

			await OrdSync_NoChange(localCtx, noChange, Ct);
			await OrdSync_RemoteIsOlder(localCtx, remoteOlder, Ct);
			await OrdSync_LocalNotExist(localCtx, localNotExist, Ct);
			await OrdSync_IdNotEqual(localCtx, idNotEqual, Ct);
			await BatSync_RemoteIsNewer(localCtx, remoteIsNewer, Ct);
			return NIL;
		});
	}

	/// NoChange 分支：兩端完全一致，無需任何寫庫。
	// ■ IList 批核心(可直接測試):空操作,入參已是批、直接忽略。
	public Task<nil> OrdSync_NoChange(IDbUserCtx Ctx, IList<DtoJnWordSyncResult> Dtos, CT Ct){
		return Task.FromResult(NIL);
	}

	// ■ 流式版(對外入口):整流一個事務,逐批調 IList 批核心。
	public Task<nil> OrdSync_NoChange(IDbUserCtx Ctx, IAsyncEnumerable<DtoJnWordSyncResult> Dtos, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, DbCtx => {
			var localCtx = new DbUserCtx(Ctx.UserCtx, DbCtx);
			return SqlFlow.BatchesInOnly(Dtos, (batch, ct2)=> OrdSync_NoChange(localCtx, batch, ct2), Ct);
		});
	}

	/// RemoteIsOlder 分支：Local 較新、保持不動。同上、只消費不落庫(作審計的 Dto 由調用方持有)。
	// ■ IList 批核心(可直接測試):空操作。
	public Task<nil> OrdSync_RemoteIsOlder(IDbUserCtx Ctx, IList<DtoJnWordSyncResult> Dtos, CT Ct){
		return Task.FromResult(NIL);
	}

	// ■ 流式版(對外入口):整流一個事務,逐批調 IList 批核心。
	public Task<nil> OrdSync_RemoteIsOlder(IDbUserCtx Ctx, IAsyncEnumerable<DtoJnWordSyncResult> Dtos, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, DbCtx => {
			var localCtx = new DbUserCtx(Ctx.UserCtx, DbCtx);
			return SqlFlow.BatchesInOnly(Dtos, (batch, ct2)=> OrdSync_RemoteIsOlder(localCtx, batch, ct2), Ct);
		});
	}

	/// LocalNotExist 分支：本用戶詞庫無此 BizId 之詞、Remote 是全新詞。
	/// 前置保證 Owner/外鍵已歸一化後、OrdAddAgg 聚合級聯插入三表(PoWord+PoWordProp+PoWordLearn)、一次同構批量完成。
	// ■ IList 批核心:函數只管一批——整批一次 OrdAddAgg。
	public Task<nil> OrdSync_LocalNotExist(IDbUserCtx Ctx, IList<DtoJnWordSyncResult> Dtos, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			var neos = new List<JnWord>(Dtos.Count);
			foreach(var dto in Dtos){
				var remote = dto.Remote ?? throw KeysErr.Word.Word__And__SyncFailed.ToErr("RemoteNull");
				remote.Owner = Ctx.UserCtx.UserId;
				remote.EnsureForeignId();
				neos.Add(remote);
			}
			if(neos.Count > 0){
				await RepoWord.OrdAddAgg<JnWord>(DbCtx, neos, Ct);
			}
			return NIL;
		});
	}

	// ■ 流式版(對外入口):整流一個事務,逐批調 IList 批核心。
	public Task<nil> OrdSync_LocalNotExist(IDbUserCtx Ctx, IAsyncEnumerable<DtoJnWordSyncResult> Dtos, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, DbCtx => {
			var localCtx = new DbUserCtx(Ctx.UserCtx, DbCtx);
			return SqlFlow.BatchesInOnly(Dtos, (batch, ct2)=> OrdSync_LocalNotExist(localCtx, batch, ct2), Ct);
		});
	}

	/// IdNotEqual 分支：BizId 相同、Id 不同——兩端各自獨立添加過同 BizId 的詞(見枚舉文檔)。
	/// 收斂規則(與 ISvcWordV2.OrdUpdHeadLang/同步約定一致)：以 Id 較小者為基準 Id(ULID 單調、小者更早)。
	/// 理論上兩端無重合資產，故 Remote 資產直接經 upsert 併入即可、無需逐資產合併。
	// ■ IList 批核心:函數只管一批——整批遷主鍵+整批以 Remote 為新版本寫回。
	public Task<nil> OrdSync_IdNotEqual(IDbUserCtx Ctx, IList<DtoJnWordSyncResult> Dtos, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			// 批核心內部調 OrdChangeId 時用「重組後的 localCtx」,讓其 EnsureTxn 偵測到外層事務而只加入。
			var localCtx = new DbUserCtx(Ctx.UserCtx, DbCtx);
			if(Dtos.Count == 0){
				return NIL;
			}
			var moveIds = new List<(IdWord Old, IdWord New)>();
			var remotesToApply = new List<JnWord>(Dtos.Count);
			foreach(var dto in Dtos){
				// 前置校驗：Local/Remote 必須都在(分類為 IdNotEqual 卻缺邊界數據 = 上游計算錯誤，直接拋)
				var local = dto.Local ?? throw KeysErr.Word.Word__And__SyncFailed.ToErr("LocalNull");
				var remote = dto.Remote ?? throw KeysErr.Word.Word__And__SyncFailed.ToErr("RemoteNull");
				local.Word.CheckOwner(Ctx.UserCtx.UserId);
				remote.Owner = Ctx.UserCtx.UserId;
				remote.EnsureForeignId();

				// step 1: 定基準 Id；若庫中行的主鍵(local.Id)不是基準 Id，記入遷移清單待改主鍵
				var keepId = local.Id.Value <= remote.Id.Value ? local.Id : remote.Id;
				if(local.Id != keepId){
					moveIds.Add((local.Id, keepId));
				}
				// step 2: 把 Remote 聚合整體改掛到基準 Id 下(改根主鍵+全部資產外鍵)
				remote.SetIdEtEnsureFKey(keepId);
				remotesToApply.Add(remote);
			}
			// step 3: 先遷主鍵(連同庫內既有資產外鍵一併搬移，見 DaoWordV2.BatChangeWordId)。
			// 必須先於 step 4——否則 OrdUpd 按 keepId 更新會找不到行/找錯行(IList 版 OrdChangeId,EnsureTxn 加入)。
			if(moveIds.Count > 0){
				await OrdChangeId(localCtx, moveIds, Ct);
			}
			// step 4: 以 Remote 為新版本整批落庫(根覆蓋、資產 upsert)
			await ApplyRemoteWordsAsBatchUpdate(DbCtx, remotesToApply, Ct);
			return NIL;
		});
	}

	// ■ 流式版(對外入口):整流一個事務,逐批調 IList 批核心。
	public Task<nil> OrdSync_IdNotEqual(IDbUserCtx Ctx, IAsyncEnumerable<DtoJnWordSyncResult> Dtos, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, DbCtx => {
			var localCtx = new DbUserCtx(Ctx.UserCtx, DbCtx);
			return SqlFlow.BatchesInOnly(Dtos, (batch, ct2)=> OrdSync_IdNotEqual(localCtx, batch, ct2), Ct);
		});
	}

	/// RemoteIsNewer 分支：Id 相同、Remote 時間更新，直接把 Remote 整批作為新版本落庫。
	/// 與 IdNotEqual 的區別：主鍵未變、無需 OrdChangeId 遷移環節。
	// ■ IList 批核心(私有,不在接口):函數只管一批。
	Task<nil> BatSync_RemoteIsNewer(IDbUserCtx Ctx, IList<DtoJnWordSyncResult> Dtos, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			if(Dtos.Count == 0){
				return NIL;
			}
			var remotes = new List<JnWord>(Dtos.Count);
			foreach(var dto in Dtos){
				var remote = dto.Remote ?? throw KeysErr.Word.Word__And__SyncFailed.ToErr("RemoteNull");
				remote.Owner = Ctx.UserCtx.UserId;
				remote.EnsureForeignId();
				remotes.Add(remote);
			}
			await ApplyRemoteWordsAsBatchUpdate(DbCtx, remotes, Ct);
			return NIL;
		});
	}

	/// 把若乾 Remote 聚合以「更新既有行」語義寫回(調用前提：根行已存在且主鍵已對齊 Remote)：
	/// - 根：OrdUpd 整行覆蓋(Remote 的 BizUpdatedAt/DelAt 等直接生效)；
	/// - 資產：OrdUpsert——同 Id 覆蓋、Remote 獨有 Id 插入、Local 獨有資產不動(合併語義、非替換)；
	/// - 收尾：BatAltWordAfterUpd 統一觸碰根之 BizUpdatedAt(ISvcWordV2 約定：凡動過 JnWord 必觸碰)。
	async Task<nil> ApplyRemoteWordsAsBatchUpdate(IDbFnCtx DbCtx, IList<JnWord> Remotes, CT Ct){
		if(Remotes.Count == 0){
			return NIL;
		}

		await RepoWord.OrdUpd(DbCtx, Remotes.Select(x=>x.Word).ToList(), Ct);
		var props = Remotes.SelectMany(x=>x.Props).ToList();
		if(props.Count > 0){
			await RepoProp.OrdUpsert(DbCtx, props, Ct);
		}
		var learns = Remotes.SelectMany(x=>x.Learns).ToList();
		if(learns.Count > 0){
			await RepoLearn.OrdUpsert(DbCtx, learns, Ct);
		}
		await DaoWordV2.BatAltWordAfterUpd(DbCtx, Remotes.Select(x=>x.Id).Distinct().ToList(), Ct);
		return NIL;
	}

	/// 流式入口包裝：Stream(壓縮打包的詞庫) → UnpackJnWords 解包為 JnWord 流 → 直接復用 BizId 同步主流程。
	/// 惰性連接：解包與同步都是 IAsyncEnumerable，端到端邊解邊同步、不全量物化。
	public IAsyncEnumerable<DtoJnWordSyncResult> OrdSyncJnWordByBizIdFromStream(
		IDbUserCtx Ctx,
		Stream TextWithStream,
		CT Ct
	){
		// 先把流中的壓縮詞流解包成 JnWord 流，再走既有 BizId 同步主流程。
		var words = UnpackJnWords(TextWithStream, Ct);
		return OrdSyncJnWordByBizId(Ctx, words, Ct);
	}

	public IAsyncEnumerable<JnWord> GetAllWordWithDel(IDbUserCtx Ctx, CT Ct){
		Ctx.DbFnCtx ??= new DbFnCtx();
		// 先取當前用戶全部根詞（含軟刪），再按 Id 批量取聚合（含軟刪）。
		var ids = RepoWord.GetAllWithDel(Ctx.DbFnCtx, Ct)
			.Where(x=>x.Owner == Ctx.UserCtx.UserId)
			.Select(x=>x.Id);
		return RepoWord.OrdGetAggByIdWithDel<JnWord>(Ctx.DbFnCtx, ids, Ct)
			.Where(x=>x is not null)
			.Select(x=>x!);
	}

	public Task<Stream> PackAllWordWithDel(IDbUserCtx Ctx, CT Ct){
		var packer = new Packer<JnWord>{
			JsonS = JsonS,
		};
		var packInfo = new ObjPackInfo{
			PayloadTypeObj = nameof(GZipLinesUtf8),
			CreatedAt = UnixMs.Now(),
		};
		var allWords = GetAllWordWithDel(Ctx, Ct);
		var packed = packer.Pack(allWords, packInfo, Ct);
		return Task.FromResult(packed.ToStream());
	}

	public IAsyncEnumerable<JnWord> UnpackJnWords(Stream TextWithStream, CT Ct){
		return UnpackCore(TextWithStream, Ct);

		async IAsyncEnumerable<JnWord> UnpackCore(
			Stream textWithStream,
			[EnumeratorCancellation] CT ct
		){
			var unpacked = await Tsinswreng.CsTextWithBlob.TextWithStream.Unpack(textWithStream, ct);
			var packer = new Packer<JnWord>{
				JsonS = JsonS,
			};
			var ans = packer.Unpack(unpacked, ct);
			if(!ans.Ok || ans.Data is null){
				throw KeysErr.Word.Word__And__SyncFailed.ToErr(str.Join('\n', ans.ErrsToStrs()));
			}

			await foreach(var word in ans.Data.WithCancellation(ct)){
				yield return word;
			}
		}
	}
}
