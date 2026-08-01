using Ngaq.Core.Infra.IF;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.User.Models.Po.User;
using Tsinswreng.CsSql;
using Tsinswreng.CsTempus;

namespace Ngaq.Core.Shared.RecentUse.Models.Po.RecentUse;

/// 通用「最近使用」記錄。
/// 一條記錄表示：某用戶在某業務場景下，最近一次使用了某個實體鍵。
[Doc(@$"
通用 recent-use 表。
用{nameof(BizUpdatedAt)}儲存某實體最近被用ʹ時間。
")]
public partial class PoRecentUse
	: PoBaseBizTime
	, I_Id<IdRecentUse>
	, I_Owner
{
	public IdRecentUse Id { get; set; } = new();

	/// 所屬用戶。
	public IdUser Owner { get; set; } = IdUser.Zero;

	/// 業務場景鍵。示例：SelectSourceLang、SelectTargetLang。
	public str Scene { get; set; } = "";
	[Doc(@$"實體類型。
	用`nameof(PoXxx)`、不用 {nameof(ITable.DbTblName)}
	")]
	public str EntityType { get; set; } = "";
	
	public u8[] EntityId { get; set; } = [];

	/// 累計使用次數。可用於同時間戳下的次級排序或分析。
	public u64 UsedCount { get; set; } = 0;
	public override UnixMs BizUpdatedAt{get;set;} = UnixMs.Now();
}

