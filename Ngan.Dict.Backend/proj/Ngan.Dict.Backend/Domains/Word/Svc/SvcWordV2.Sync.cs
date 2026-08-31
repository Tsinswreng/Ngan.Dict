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
	/// 形態：BatchCollector 每攢滿一批(默認 DfltBatchSize=100)執行一次完整的「查庫→算差異→寫庫」流水線；
	/// 出參流與入參流位置一一對應(AllFlat 按批順序攤平)，Dto 同時作審計憑據返回。
	/// 事務：本函數自身不包 EnsureTxn、讀走裸 Ctx、寫全部委託 OrdSyncByDto(其內部自行開事務)。
	public async IAsyncEnumerable<DtoJnWordSyncResult> OrdSyncJnWordByBizId(
		IDbUserCtx Ctx,
		IAsyncEnumerable<JnWord> JnWords,
		[EnumeratorCancellation] CT Ct
	){
		// 只讀與(經 OrdSyncByDto 的)寫都掛在這個惰性創建的 DbFnCtx 上；無外層事務時每次 EnsureTxn 會就地開新事務
		Ctx.DbFnCtx ??= new DbFnCtx();
		// 批處理單元：TRet = 本批的同步結果流(而非逐條結果)，交給外層 AllFlat 攤平輸出
		await using var batch = new BatchCollector<JnWord, IAsyncEnumerable<DtoJnWordSyncResult>>(async(remotes, Ct)=>{
			if(remotes.Count == 0){
				return ToAsyE(Array.Empty<DtoJnWordSyncResult>());
			}

			// step 1: 歸屬歸一化——無論入參寫了誰，一律強制為當前用戶；資產外鍵對齊到根的 Id
			foreach(var remote in remotes){
				remote.Owner = Ctx.UserCtx.UserId;
				remote.EnsureForeignId();
			}

			// step 2: 按 BizId 批量查 Local 聚合根(含軟刪)。
			// 底層 RunDupliSql 是同構批量：每個 (Head,Lang) 生成一條等值 SQL、一次往返執行，
			// 結果與入參位置嚴格對齊、查無補 null，所以 localPos[i] 就是 remotes[i] 的舊詞。
			// 含軟刪的原因：已刪的行仍佔用 BizId，必須交差異計算(CompareJnWord 會比較 DelAt)判衝突。
			var headLangs = remotes.Select(x=>new Head_Lang(x.Head, x.Lang)).ToList();
			var localPos = await DaoWordV2.BatGetPoWordByOwnerHeadLangWithDel(
				Ctx.DbFnCtx,
				Ctx.UserCtx.UserId,
				ToAsyE(headLangs),
				Ct
			).ToListAsync(Ct);

			// step 3: 拿 step 2 命中的根 Id 批量回讀完整聚合(根+Props+Learns、含軟刪)。
			// OrdGetAggByIdWithDel 保持 Ord 語義：locals[i] 與 ids[i] 位置對應、查無為 null，
			// 故可用普通字典把「根 Id → 聚合」建索引供 step 4 逐條取用。
			var ids = localPos.Where(x=>x is not null).Select(x=>x!.Id).Distinct().ToList();
			var localById = new Dictionary<IdWord, JnWord?>();
			if(ids.Count > 0){
				var locals = await DaoWordV2.BatGetJnWordByIdWithDel(Ctx.DbFnCtx, ToAsyE(ids), Ct).ToListAsync(Ct);
				for(var i = 0; i < ids.Count; i++){
					localById[ids[i]] = locals[i];
				}
			}

			// step 4: 逐條算差異(純內存、不落庫)，產出 EDiffByBizIdResultForSync 五類分類的 Dto。
			// 邊界：localPo 命中但聚合取不到(理論上不該發生)時按 local=null 處理、即視為 LocalNotExist。
			var dtos = new List<DtoJnWordSyncResult>(remotes.Count);
			for(var i = 0; i < remotes.Count; i++){
				var localPo = localPos[i];
				JnWord? local = null;
				if(localPo is not null){
					local = localById.GetValueOrDefault(localPo.Id);
				}
				dtos.Add(SvcWordInMem.SyncJnWord(local, remotes[i]));
			}
			// step 5: 整批落庫。OrdSyncByDto 內部再按 DiffResult 分桶派發、全程共用一個事務。
			await OrdSyncByDto(Ctx, ToAsyE(dtos), Ct);
			return ToAsyE(dtos);
		});

		// AllFlat = AddToEnd(攢批逐條喂入+結尾 End 收尾最後未滿批)再將各批結果流攤平
		var all = batch.AllFlat(JnWords, Ct);
		await foreach(var dto in all.WithCancellation(Ct)){
			yield return dto;
		}
	}

	/// 分桶派發器：把整批 Dto 按 DiffResult 歸入五桶、逐桶交給 ISvcWordSync 對應處理器。
	/// 前置物化的原因：入參是只能消費一次的 IAsyncEnumerable、且五個處理器都是批量接口，
	/// 故先 await foreach 完整消費一遍分流到 List、再以內存列表重啟各桶的流。
	/// 事務邊界：EnsureTxn 包住全部五次調用——本批所有分支的寫入共用同一事務、要麼全成要麼全rollback。
	/// localCtx：把 EnsureTxn 準備好的 DbCtx 與用戶上下文重組為 IDbUserCtx 傳給處理器，
	/// 處理器內部再 EnsureTxn 時會檢測到已有外層事務(hasOuterTxn)而只加入不另開、不提交。
	/// 調用順序固定：NoChange → RemoteIsOlder → LocalNotExist → IdNotEqual → RemoteIsNewer，
	/// 其中 IdNotEqual 會改主鍵、與後續 RemoteIsNewer 的按 Id 更新存在先後依賴，勿隨意調整。
	public Task<nil> OrdSyncByDto(IDbUserCtx Ctx, IAsyncEnumerable<DtoJnWordSyncResult> Dtos, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			var localCtx = new DbUserCtx(Ctx.UserCtx, DbCtx);
			var noChange = new List<DtoJnWordSyncResult>();
			var remoteOlder = new List<DtoJnWordSyncResult>();
			var localNotExist = new List<DtoJnWordSyncResult>();
			var idNotEqual = new List<DtoJnWordSyncResult>();
			var remoteIsNewer = new List<DtoJnWordSyncResult>();

			// step 1: 完整消費入參流、按分類落入五桶(未知分類會被丟棄、目前枚舉無此情況)
			await foreach(var dto in Dtos.WithCancellation(Ct)){
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

			await OrdSync_NoChange(localCtx, ToAsyE(noChange), Ct);
			await OrdSync_RemoteIsOlder(localCtx, ToAsyE(remoteOlder), Ct);
			await OrdSync_LocalNotExist(localCtx, ToAsyE(localNotExist), Ct);
			await OrdSync_IdNotEqual(localCtx, ToAsyE(idNotEqual), Ct);
			await BatSync_RemoteIsNewer(localCtx, ToAsyE(remoteIsNewer), Ct);
			return NIL;
		});
	}

	/// NoChange 分支：兩端完全一致，無需任何寫庫。
	/// 仍完整消費入參流：一是流式 API 的約定(消費即執行)，二是保持五分支調用形狀統一便于測試。
	public async Task<nil> OrdSync_NoChange(IDbUserCtx Ctx, IAsyncEnumerable<DtoJnWordSyncResult> Dtos, CT Ct){
		await foreach(var _ in Dtos.WithCancellation(Ct)){
		}
		return NIL;
	}

	/// RemoteIsOlder 分支：Local 較新、保持不動。同上、只消費不落庫(作審計的 Dto 由調用方持有)。
	public async Task<nil> OrdSync_RemoteIsOlder(IDbUserCtx Ctx, IAsyncEnumerable<DtoJnWordSyncResult> Dtos, CT Ct){
		await foreach(var _ in Dtos.WithCancellation(Ct)){
		}
		return NIL;
	}

	/// LocalNotExist 分支：本用戶詞庫無此 BizId 之詞、Remote 是全新詞。
	/// 前置保證 Owner/外鍵已歸一化後、OrdAddAgg 聚合級聯插入三表(PoWord+PoWordProp+PoWordLearn)、一次同構批量完成。
	public Task<nil> OrdSync_LocalNotExist(IDbUserCtx Ctx, IAsyncEnumerable<DtoJnWordSyncResult> Dtos, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			var neos = Dtos.Select(x=>{
				var remote = x.Remote ?? throw KeysErr.Word.Word__And__SyncFailed.ToErr("RemoteNull");
				remote.Owner = Ctx.UserCtx.UserId;
				remote.EnsureForeignId();
				return remote;
			});
			await RepoWord.OrdAddAgg<JnWord>(DbCtx, neos, Ct);
			return NIL;
		});
	}

	/// IdNotEqual 分支：BizId 相同、Id 不同——兩端各自獨立添加過同 BizId 的詞(見枚舉文檔)。
	/// 收斂規則(與 ISvcWordV2.OrdUpdHeadLang/同步約定一致)：以 Id 較小者為基準 Id(ULID 單調、小者更早)。
	/// 理論上兩端無重合資產，故 Remote 資產直接經 upsert 併入即可、無需逐資產合併。
	public Task<nil> OrdSync_IdNotEqual(IDbUserCtx Ctx, IAsyncEnumerable<DtoJnWordSyncResult> Dtos, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			// 批內步驟須按行成對處理(local↔remote 同下標)，故攢批後在迴圈內配對、而非按桶各自流式處理
			await using var batch = new BatchCollector<DtoJnWordSyncResult, nil>(async(dtoBatch, Ct)=>{
				var moveIds = new List<(IdWord Old, IdWord New)>();
				var remotesToApply = new List<JnWord>(dtoBatch.Count);
				foreach(var dto in dtoBatch){
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
				// step 3: 先遷主鍵(連同庫內既有資產外鍵一併搬移，見 DaoWordV2.BatChangeWordId)，
				// 必須先於 step 4——否則 OrdUpd 按 keepId 更新會找不到行/找錯行。
				if(moveIds.Count > 0){
					await OrdChangeId(new DbUserCtx(Ctx.UserCtx, DbCtx), ToAsyE(moveIds), Ct);
				}
				// step 4: 以 Remote 為新版本整批落庫(根覆蓋、資產 upsert)
				await ApplyRemoteWordsAsBatchUpdate(DbCtx, remotesToApply, Ct);
				return NIL;
			});
			await batch.ConsumeAll(Dtos, Ct);
			return NIL;
		});
	}

	/// RemoteIsNewer 分支：Id 相同、Remote 時間更新，直接把 Remote 整批作為新版本落庫。
	/// 與 IdNotEqual 的區別：主鍵未變、無需 OrdChangeId 遷移環節。
	Task<nil> BatSync_RemoteIsNewer(IDbUserCtx Ctx, IAsyncEnumerable<DtoJnWordSyncResult> Dtos, CT Ct){
		return SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async(DbCtx)=>{
			await using var batch = new BatchCollector<DtoJnWordSyncResult, nil>(async(dtoBatch, Ct)=>{
				var remotes = new List<JnWord>(dtoBatch.Count);
				foreach(var dto in dtoBatch){
					var remote = dto.Remote ?? throw KeysErr.Word.Word__And__SyncFailed.ToErr("RemoteNull");
					remote.Owner = Ctx.UserCtx.UserId;
					remote.EnsureForeignId();
					remotes.Add(remote);
				}
				await ApplyRemoteWordsAsBatchUpdate(DbCtx, remotes, Ct);
				return NIL;
			});
			await batch.ConsumeAll(Dtos, Ct);
			return NIL;
		});
	}

	/// 單條便捷入口：把一個 Remote 包裝成單元素列表走批量通道(庫的約定：批量 API 傳單元素是正常用法)。
	async Task<nil> ApplyRemoteWordAsUpdate(IDbFnCtx DbCtx, JnWord Remote, CT Ct){
		await ApplyRemoteWordsAsBatchUpdate(DbCtx, [Remote], Ct);
		return NIL;
	}

	/// 把若乾 Remote 聚合以「更新既有行」語義寫回(調用前提：根行已存在且主鍵已對齊 Remote)：
	/// - 根：OrdUpd 整行覆蓋(Remote 的 BizUpdatedAt/DelAt 等直接生效)；
	/// - 資產：OrdUpsert——同 Id 覆蓋、Remote 獨有 Id 插入、Local 獨有資產不動(合併語義、非替換)；
	/// - 收尾：BatAltWordAfterUpd 統一觸碰根之 BizUpdatedAt(ISvcWordV2 約定：凡動過 JnWord 必觸碰)。
	async Task<nil> ApplyRemoteWordsAsBatchUpdate(IDbFnCtx DbCtx, IList<JnWord> Remotes, CT Ct){
		if(Remotes.Count == 0){
			return NIL;
		}

		await RepoWord.OrdUpd(DbCtx, ToAsyE(Remotes.Select(x=>x.Word)), Ct);
		var props = Remotes.SelectMany(x=>x.Props).ToList();
		if(props.Count > 0){
			await RepoProp.OrdUpsert(DbCtx, ToAsyE(props), Ct);
		}
		var learns = Remotes.SelectMany(x=>x.Learns).ToList();
		if(learns.Count > 0){
			await RepoLearn.OrdUpsert(DbCtx, ToAsyE(learns), Ct);
		}
		await DaoWordV2.BatAltWordAfterUpd(DbCtx, ToAsyE(Remotes.Select(x=>x.Id).Distinct()), Ct);
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
