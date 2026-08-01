namespace Ngaq.Core.Infra;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
[JsonSourceGenerationOptions(
	//PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
	ReadCommentHandling = JsonCommentHandling.Skip,
	UseStringEnumConverter = true
)]
public partial class AppJsonCtx{}
