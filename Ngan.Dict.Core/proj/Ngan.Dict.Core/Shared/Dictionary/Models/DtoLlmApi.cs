namespace Ngan.Dict.Core.Shared.Dictionary.Models;

using System.Text.Json;
using System.Text.Json.Serialization;
using Ngan.Dict.Core.Infra.IF;

/// 大模型 API 請求體（OpenAI 兼容）。
///
/// *為何是具體 DTO 而不是字典*：本應用是 NativeAOT，線上式型別必須登記在
/// `AppJsonCtx`（源生成）。`Dictionary<string, object?>` 裏的值是 `object`，
/// 源生成器不知道要按什麼型別寫，執行期直接拋
/// `NotSupportedException: JsonTypeInfo metadata ... was not provided`。
/// 這是 `TswgCsTpl` README 明寫的約束：「不要在線上式用 obj」。
///
/// 登記方式：實現 `IAppSerializable`，由 `GenAppJsonCtx` 掃描產出 `[JsonSerializable]`。
public class DtoLlmApiReq : IAppSerializable{
	/// 模型名，對應配置 `LlmDictionary.Model`。
	/// 模型名，對應配置 `LlmDictionary.Model`。
	/// *為何顯式寫線上式名*：本應用全域不轉命名（PascalCase 直接上線），
	/// 而 OpenAI 兼容 API 的欄位名固定是小寫，故逐欄標 `JsonPropertyName`。
	[JsonPropertyName("model")]
	public str? Model{get;set;}

	/// 對話訊息；第一條是系統提示詞，第二條是用戶查詞請求。
	[JsonPropertyName("messages")]
	public List<DtoLlmMessage>? Messages{get;set;}

	/// 是否流式輸出。詞典查詢固定用流式，故呼叫端一律設 true。
	[JsonPropertyName("stream")]
	public bool Stream{get;set;}

	/// provider 私有頂層參數（如 DeepSeek 的 `thinking`），由配置 `ExtraBodyJson` 原樣透傳。
	/// 值是 `JsonElement` 而非 `object`：前者有內建轉換器、不需要反射元資料，AOT 安全。
	[JsonExtensionData]
	public Dictionary<str, JsonElement>? Extra{get;set;}
}

/// 大模型對話訊息。
public class DtoLlmMessage : IAppSerializable{
	/// 角色，`system` 或 `user`。
	[JsonPropertyName("role")]
	public str? Role{get;set;}

	/// 訊息內容。
	[JsonPropertyName("content")]
	public str? Content{get;set;}
}
