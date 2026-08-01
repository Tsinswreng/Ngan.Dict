using Ngaq.Core.Shared.Word.Models;

namespace Ngaq.Core.Shared.Sync;
[Doc(@$"
適用于當{nameof(EDiffByBizIdResultForSync.RemoteIsNewer)}時。
聚合類的比較情況、用于同步。
設把Remote合入Local。

兩個聚合類有不同時、默認Remote更加新。

合併操作 只能對Local做改動。

Remote和Local的BizId(業務層唯一標識、非Id字段)須一致、
Id可能不一致。

下面的條目未必是互斥的、可能有同時滿足的項目。

名詞解釋:
- 資產(Assets): 指聚合對象中 除了聚合根之外的實體(或實體列表)
	例:對于{nameof(JnWord)}而言、其資產即 {nameof(JnWord.Props)} 和 {nameof(JnWord.Learns)};
- 對于單個資產、軟刪除也屬于Change。

比較一對資產實體時是按Id比較。資產實體可能沒有BizId。
")]
public interface IAggDiffCaseForSync{
	[Doc(@$"
	Remote中有新增之資產 which is Local中沒有的。
	需把Local缺少的部分入庫。
	")]
	public bool RemoteHasNewAssets{get;set;}
	
	[Doc(@$"Remote和Local 資產數相同 但對應Id的資產中有的是 Remote的更加新。
	資產被軟刪除也算Change。
	需把有改變的部分在庫中對應更改。
	")]
	public bool RemoteHasChangedAssets{get;set;}
	
	[Doc(@$"需把Local也同步軟刪除。資產若有不同也要合併。")]
	public bool RemoteIsSoftDeleted{get;set;}
	
	[Doc(@$"Local被軟刪除。需把Local的軟刪除狀態去除。")]
	public bool LocalIsSoftDeleted{get;set;}
	
	[Doc(@$"聚合根有變化。
	對 {nameof(JnWord)}而言、 其聚合根即 {nameof(JnWord.Word)}
	")]
	public bool AggRootIsChanged{get;set;}
	
}

public class AggDiffCaseForSync:IAggDiffCaseForSync{
	public bool RemoteHasNewAssets{get;set;}
	public bool RemoteHasChangedAssets{get;set;}
	public bool RemoteIsSoftDeleted{get;set;}
	public bool LocalIsSoftDeleted{get;set;}
	public bool AggRootIsChanged{get;set;}
}

