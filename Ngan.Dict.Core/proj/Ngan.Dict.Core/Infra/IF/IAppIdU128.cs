namespace Ngan.Dict.Core.Infra.IF;

using Tsinswreng.CsCore;
using Tsinswreng.CsEntity;

[Doc($$"""
本應用所有強類型 Id 的共同父介面：UInt128 為底（ULID 填充、天然有序）、對外走 Low64Base 字串。

*形狀都在 CsEntity*：{{nameof(Tsinswreng.CsEntity.IIdU128)}} 給的是 `Value` 成員、
比較運算符與字符串轉換的約定；本介面只把「可被應用的 JSON 源生成器登記」這一件事加上去，
故生成器掃 `IAppSerializable` 就能一次收齊全部 Id。

*為何不再自己聲明一份*：舊版這裡有一份 `IIdUInt128` 與 `ExtnIIIdUInt128`，被抽去 CsEntity 之後就成了
兩份同義實作——庫升版時會分岔。現一律用 CsEntity 的 {{nameof(Tsinswreng.CsEntity.IIdU128Extn)}}，
調用寫法不變（`IdWord.FromLow64Base(s)`、`IdWord.MkTypeMapFn()` 等靜態擴展成員）。
""")]
public interface IAppIdU128
	: Tsinswreng.CsEntity.IIdU128
	, IAppSerializable
{
}
