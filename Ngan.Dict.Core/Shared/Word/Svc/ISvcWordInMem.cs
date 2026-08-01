using System.Diagnostics.Contracts;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.Sync;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Learn;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Tools;
using Tsinswreng.CsErr;
using Tsinswreng.CsTextWithBlob;

namespace Ngaq.Core.Shared.Word.Svc;

[Doc(@$"針對 {nameof(JnWord)}。
純內存操作相關API 不涉及數據庫。
")]
public interface ISvcWordInMem{
	[Doc(@$"確保兩詞之{nameof(PoWord.Owner)},{nameof(PoWord.Head)},{nameof(PoWord.Lang)}相同。
	此函數中則不再重複校驗。
	")]
	[Pure]
	public EDiffByBizIdResultForSync CompareJnWord(JnWord Local, JnWord Remote){
		var X = Local;
		var Y = Remote;
		if(!X.Word.IsSameUserWord(Y.Word)){
			throw KeysErr.Word.__And__IsNotSameUserWord.ToErr(X.Id,Y.Id);
		}
		if(X.Id != Y.Id){
			return EDiffByBizIdResultForSync.IdNotEqual;
		}
		
		if(X.BizUpdatedAt == Y.BizUpdatedAt
			&& X.BizCreatedAt == Y.BizCreatedAt
			&& X.DelAt == Y.DelAt
			//資產變更時 聚合根之 BizUpdatedAt 也會變、只比較資產數量已足夠
			&& X.Props.Count == Y.Props.Count
			&& X.Learns.Count == Y.Learns.Count
		){
			return EDiffByBizIdResultForSync.NoChange;
		}
		var xUpd = X.Word.GetNewestBizUpdOrDelTime();
		var yUpd = Y.Word.GetNewestBizUpdOrDelTime();
		if(yUpd > xUpd){
			return EDiffByBizIdResultForSync.RemoteIsNewer;
		}else{
			return EDiffByBizIdResultForSync.RemoteIsOlder;
		}
		// 兩單詞 Id相同但 BizCreatedAt 不同。
		//不應該進入此分支。
		//throw ItemsErr.Word.Word__And__SyncFailed.ToErr(X.Id, Y.Id);
	}
	
	public int DiffWordAssetByTime(PoWord Local, PoWord Remote){
		IEntitySyncerInMem<PoWord> syncer = new EntitySyncerInMem<PoWord>();
		return syncer.DiffPoByTime(Local, Remote);
	}
	
	[Doc(@$"
把{nameof(Remote)}合入{nameof(Local)}、純函數、返回新的合併後的對象。

若({nameof(PoWord.Owner)},{nameof(PoWord.Head)},{nameof(PoWord.Lang)})不同則拒絕合併、
拋出 {nameof(KeysErr.Word.__And__IsNotSameUserWord)};
		
返回值的{nameof(JnWord.Word)}按{nameof(SyncPoWord)}處理。

	對其餘 {nameof(JnWord.Props)} 和 {nameof(JnWord.Learns)}的每項:
	- 按Id尋匹配者、然後各自用 {nameof(SyncProp)}和{nameof(SyncLearn)}
	- 對于 {nameof(Remote)} 比 {nameof(Local)} 多出來的資產、則要合入。
	- 對于 {nameof(Local)} 比 {nameof(Remote)} 多出來的資產、不管。
	
	合併時、除了可能把Local的{nameof(PoWord.BizUpdatedAt)}改成和Remote一樣之外、
	本身不會再另外修改 業務更新時間。
	")]
	[Pure]
	public DtoJnWordSyncResult SyncJnWord(JnWord? Local, JnWord Remote);
	
	[Doc(@$"
	若{nameof(PoWordProp.Id)}不同則拒絕合併。
	以{nameof(DiffWordAssetByTime)}決定誰最新。若Remote不比Local更加新則不動。
	")]
	[Pure]
	public DtoEntityDiffEtSync<PoWordProp> SyncProp(PoWordProp Local, PoWordProp Remote);
	
	[Doc(@$"見{nameof(SyncProp)} for {nameof(PoWordProp)}")]
	[Pure]
	public DtoEntityDiffEtSync<PoWordLearn> SyncLearn(PoWordLearn Local, PoWordLearn Remote);
	
	[Doc(@$"
	若({nameof(PoWord.Owner)},{nameof(PoWord.Head)},{nameof(PoWord.Lang)})不同則拒絕合併、
	拋出 {nameof(KeysErr.Word.__And__IsNotSameUserWord)};
	
	以{nameof(DiffWordAssetByTime)}決定誰最新。若Remote不比Local更加新則不動。
	若{nameof(PoWord.Id)}不同 則同步結果爲 {nameof(PoWord.BizCreatedAt)}更小者。
	")]
	[Pure]
	public DtoEntityDiffEtSync<PoWord> SyncPoWord(PoWord Local, PoWord Remote);
	
	
	
	
	public class EMergeResult{
		[Doc(@$"該實體的BizId不在數據庫中")]
		public static readonly EMergeResult New = new();
		
		[Doc(@$"該實體的BizId在數據庫中")]
		public static readonly EMergeResult Changed = new();
	}
	
	[Doc(@$"
	合併兩單詞。
	注意 合併(Merge) 與 同步(Sync) 語義不同。
	合併是把 {nameof(Remote)}有 而 {nameof(Local)}無 的資產合入 {nameof(Local)}。
	若{nameof(Local)} 之聚合根已經被軟刪除 則要把他恢復爲未刪除狀態(只動聚合根 不管資產)
	")]
	[Pure]
	public IJnWordMergeResult Merge(JnWord? Local, JnWord Remote);
	

}

public enum EJnWordMergeResult{
	[Doc(@$"Remote沒有比Local多出資產。
	即 不存在 Remote有洏Local無 之 資產。
	")]
	NoChange,
	[Doc("同BizId的Local不存在數據庫")]
	LocalNotExist,
	[Doc(@$"存在 Remote有洏Local無 之 資產")]
	Changed,
}

public interface IJnWordMergeResult{
	public EJnWordMergeResult Result{get;set;}
	
	[Doc(@$"Remote有而Local無 的新資產。")]
	public JnWord? NewAssets{get;set;}
	
	[Doc(@$"完整的 合併後的對象。
	若{nameof(Result)}=={nameof(EJnWordMergeResult.Changed)}
	則 {nameof(JnWord.Word.BizCreatedAt)} 應取 Local 與 Remote 中之最小者。
	且 {nameof(JnWord.Word.BizUpdatedAt)} 要設爲當前時間。
	若只是把已軟刪的 Local 恢復爲未刪除狀態，則 {nameof(NewAssets)} 允許爲 null。
	")]
	public JnWord Merged{get;set;}
}

public class JnWordMergeResult : IJnWordMergeResult {
	public EJnWordMergeResult Result {get;set;}
	public JnWord? NewAssets {get;set;}
	public JnWord Merged {get;set;}
}
