using Ngan.Dict.Core.Infra;
using Tsinswreng.CsJson;

namespace Ngan.Dict.Core.Tools.Json;

/// 公共庫 IJsonSerializer 的業務默認實例。
/// 默認 options 直接複用 AppJsonCtx.Inst.Options——與舊 JSON.Stringify 行為一致
/// （源生成 resolver、強類型轉換器、enum 字符串轉換器、不轉義、PascalCase）。
/// 供非 DI 場景（如 Packer 默認值、前端 VM 默認值）使用。
public static class AppJsonS{
	public static Tsinswreng.CsJson.IJsonSerializer Inst => field??= new JsonSerializer(AppJsonCtx.Inst.Options);
}