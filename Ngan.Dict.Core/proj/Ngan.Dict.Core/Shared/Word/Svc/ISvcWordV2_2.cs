using Ngan.Dict.Core.Infra;
using Ngan.Dict.Core.Shared.Sync;
using Ngan.Dict.Core.Shared.Word.Models;
using Ngan.Dict.Core.Shared.Word.Models.Dto;

namespace Ngan.Dict.Core.Shared.Word.Svc;

[Doc(@$"
{nameof(ISvcWordV2.OrdSyncJnWordByBizId)} 的重構原型(`_2` 後綴 = 新寫法)。

按 2026-08-31 討論(批原語方向)重寫「讓業務手搓批的膠水」這一層:

- 批的編排權收回庫原語(流切批器 `SqlFlow.Batches`):業務只聲明「一批做什麼」,
  批的切分/時機/尾批由原語承擔,批大小取庫的默認值、業務不再出現批大小數字;
- 批量查原語:批內直接回位置對齊的 `IList`、查無補 null,消滅批內 `ToListAsync`;
- 消滅批中批:五個差異分支直接吃已物化的批(不再各自包 BatchCollector);
- 事務邊界:由 {nameof(OrdSyncByDto_2)} 顯式聲明(業務層),分支只加入不另開。

入參出參仍是 IAsyncEnumerable(流是形狀),與原有鏈路等價、僅內部編排不同。
實驗性質:如果效果不達預期,刪除 `_2` 相關文件即可回滾,不影響原鏈路。
#See[{nameof(ISvcWordV2.OrdSyncJnWordByBizId)}]
")]
public interface ISvcWordV2_2{
	[Doc(@$"
	等價於 {nameof(ISvcWordV2.OrdSyncJnWordByBizId)}、只是內部改用批原語編排。
	#See[`SqlFlow.Batches`]
	#See[{nameof(OrdSyncByDto_2)}]
	#Rtn[與入參流位置一一對應的審計 Dto、按批攤平輸出]
	")]
	public IAsyncEnumerable<DtoJnWordSyncResult> OrdSyncJnWordByBizId_2(
		IDbUserCtx Ctx, IAsyncEnumerable<JnWord> JnWords, CT Ct
	);

	[Doc(@$"
	等價於 {nameof(ISvcWordV2.OrdSyncByDto)}、但入參已於批邊界物化(IList)——
	原實現要先完整消費一次入參流再分桶,`_2` 版直接分桶。
	事務邊界:本方法聲明「這一整批的寫入在一個事務裏」(EnsureTxn 可嵌套、外層已有事務時只加入)。
	#See[{nameof(ISvcWordSync_2)}]
	")]
	public Task<nil> OrdSyncByDto_2(
		IDbUserCtx Ctx, IList<DtoJnWordSyncResult> Dtos, CT Ct
	);
}

[Doc(@$"對應 {nameof(ISvcWordSync)} 的 `_2` 版:各分支直接吃已物化的 `IList`、不再內部切批。
只作內部派發與直接測試用、不是公共 API。
#See[{nameof(ISvcWordV2_2.OrdSyncByDto_2)}]
")]
public interface ISvcWordSync_2{
	[Doc(@$"{nameof(EDiffByBizIdResultForSync.NoChange)}:兩端一致、無任何寫。")]
	public Task<nil> OrdSync_NoChange_2(
		IDbUserCtx Ctx, IList<DtoJnWordSyncResult> Dtos, CT Ct
	);

	[Doc(@$"{nameof(EDiffByBizIdResultForSync.RemoteIsOlder)}:Local 較新、保持不動。")]
	public Task<nil> OrdSync_RemoteIsOlder_2(
		IDbUserCtx Ctx, IList<DtoJnWordSyncResult> Dtos, CT Ct
	);

	[Doc(@$"{nameof(EDiffByBizIdResultForSync.LocalNotExist)}:本用戶詞庫無此 BizId、直接整聚合插入。")]
	public Task<nil> OrdSync_LocalNotExist_2(
		IDbUserCtx Ctx, IList<DtoJnWordSyncResult> Dtos, CT Ct
	);

	[Doc(@$"{nameof(EDiffByBizIdResultForSync.IdNotEqual)}:BizId 同而 Id 不同,以 Id 更小者為基準收斂(遷主鍵+資產外鍵)。")]
	public Task<nil> OrdSync_IdNotEqual_2(
		IDbUserCtx Ctx, IList<DtoJnWordSyncResult> Dtos, CT Ct
	);
}