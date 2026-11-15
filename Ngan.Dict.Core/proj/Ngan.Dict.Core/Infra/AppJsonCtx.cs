namespace Ngan.Dict.Core.Infra;

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

[Doc($$"""
本應用的 JSON 序列化上下文（System.Text.Json 源生成）。

*為何要源生成*：本應用以 NativeAOT 發佈，運行期反射式序列化會被裁剪掉，
故所有上線的型別都必須在編譯期登記。登記分兩處：

1. `[JsonSerializable]` 與轉換器清單——由 `Ngan.Dict.Scripts` 的 `GenAppJsonCtx` 掃描
   `IAppSerializable`（業務型別）與 `IIdU128`（強類型 Id）自動生成，
   輸出到 `Tsinswreng.SrcGen/Ngan.Dict.Gen.g.cs`。*勿手動編輯*，改完 DTO／實體就重跑生成器。
2. 本檔的運行期選項 {{nameof(Opt)}}——字符轉義、命名策略、註釋容忍度這類行為，
   以及把生成檔裏的 {{nameof(JsonConverters)}} 掛上去。

*兩個坑（踩過）*：
- `TypeInfoResolver` 必須指向同一份上下文（{{nameof(AppJsonCtx.Default)}}），否則源生成的元資料不會被用到；
  指向 null 時 JIT 下退回反射、一切正常，AOT 下才炸。
- 生成期的 `[JsonSourceGenerationOptions]`（在生成檔裏）與運行期的 {{nameof(Opt)}} 是兩份獨立配置，
  兩邊不一致時同一個型別走不同路徑會寫出不同結果；改一處要同改另一處。
""")]
public partial class AppJsonCtx : JsonSerializerContext {
	protected static AppJsonCtx? _Inst = null;

	[Doc("全局唯一實例；延遲建立，避免靜態初始化期就碰生成檔裏的 `Default`。")]
	public static AppJsonCtx Inst => _Inst??= new AppJsonCtx(Opt);

	static AppJsonCtx(){
		// enum 一律寫成名字（與舊行為一致）；其二進制安全性由源生成器負責。
		Opt.Converters.Add(new JsonStringEnumConverter());
		Opt.Converters.AddRange(JsonConverters);
	}

	[Doc($$"""
	運行期選項。與生成檔的 `[JsonSourceGenerationOptions]` 必須保持一致的行為：
	不忽略 null、容忍註釋、不轉義非 ASCII。

	*為何不轉義*：中文文案與生僻字在線上式裡保持可讀，便於抓包排查；
	代價是輸出必須以 UTF-8 傳輸（本應用全部是 HTTP／文件，均為 UTF-8）。
	""")]
	public static JsonSerializerOptions Opt { get; } = new JsonSerializerOptions{
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		,PropertyNamingPolicy = null
		,TypeInfoResolver = AppJsonCtx.Default
		,ReadCommentHandling = JsonCommentHandling.Skip
	};
}
