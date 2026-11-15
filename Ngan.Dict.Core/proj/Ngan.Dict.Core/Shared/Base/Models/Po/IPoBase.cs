namespace Ngan.Dict.Core.Shared.Base.Models.Po;

using Ngan.Dict.Core.Infra.IF;
using Tsinswreng.CsCore;
using Tsinswreng.CsEntity;

[Doc($$"""
所有持久化實體（Po）的共同形狀。

*形狀來自身共用的庫*：{{nameof(Tsinswreng.CsEntity.IBasePoBase)}} 提供三共同列
（{{nameof(IBasePoBase.DbCreatedAt)}}／{{nameof(IBasePoBase.DbUpdatedAt)}}／{{nameof(IBasePoBase.DelAt)}}）、
淺複製與軟刪語義；本介面只額外要求「可被應用的 JSON 源生成器登記」（本專案的生成器按此標記掃描）。

*與本項目舊版的差異*：舊版自己聲明了一份 `I_DelAt` 並用 `IdDel`（自製 i64 包裝）表示刪除時間。
現改用 CsEntity 的 {{nameof(Tsinswreng.CsEntity.I_DelAt)}}（型別 {{nameof(Tsinswreng.CsTempus.UnixMs)}}）——
兩者的數據庫型別與線上表示完全一致（毫秒時間戳、0 表示未刪），故既有數據不受影響。
""")]
public partial interface IPoBase
	: IBasePoBase
	, IAppSerializable
{
}
