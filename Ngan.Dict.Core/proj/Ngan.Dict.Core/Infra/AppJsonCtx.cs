namespace Ngan.Dict.Core.Infra;

using System.Collections.Concurrent;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Ngan.Dict.Core.Infra.IF;
using Ngan.Dict.Core.Shared.User.Models.Po;
using Ngan.Dict.Core.Shared.User.Models.Po.User;
using Ngan.Dict.Core.Tools;
using Tsinswreng.CsTools;

interface INull{

}

//JsonSerializable註解須寫在一起、不能散落在分散之partial class、否則json源生成器不輸出
//執行 Ngan.Dict.Scripts 的 GenAppJsonCtx 以自動生成帶註解ʹAppJsonCtx分部類（輸出至 Tsinswreng.SrcGen/Ngan.Dict.Gen.g.cs）
/// ASP.net 內置Json序列化器專用。
/// 縱既配opt =>opt.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonCtx.Default);
/// 猶需手上 opt.SerializerOptions.Converters.Add(new MyConverter()......)
/// 佢不認AppJsonCtx中 用註解配˪ʹ Converters

//此註解已加在 IRawValueSerializableListʃ生成ʹ代碼中、勿複手動添
// [JsonSourceGenerationOptions(
// 	//PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
// 	DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
// 	// ,Converters = [
// 	// 	,typeof(JsonConvtr<Tempus>)
// 	// ]

// )]
public partial class AppJsonCtx : JsonSerializerContext {
	// public static IList<JsonConverter> JsonConverters = [
	// 	new global::Ngan.Dict.Core.Tools.JsonConvtr<IdKv>(),
	// ];
	protected static AppJsonCtx? _Inst = null;
	public static AppJsonCtx Inst => _Inst??= new AppJsonCtx(Opt);

	static AppJsonCtx(){
		Opt.Converters.Add(new JsonStringEnumConverter());
		Opt.Converters.AddRange(JsonConverters);
	}

	//public AppJsonCtx(JsonSerializerOptions options) : base(options) { }
	static JsonSerializerOptions Opt = new JsonSerializerOptions{
		//WriteIndented = true
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // 允許原樣輸出
		,PropertyNamingPolicy = null  // 关闭命名策略
		,TypeInfoResolver = AppJsonCtx.Default
		,ReadCommentHandling = JsonCommentHandling.Skip
		//,Converters = { new CustomJsonConvtrFctry() }
		// ,Converters = {
		// }
	};
}


#if false //額外類型 已搬入 Ngan.Dict.Scripts 的 GenAppJsonCtx 之 ExtraJsonTypes

[JsonSerializable(typeof(global::Tsinswreng.CsErr.IWebAns<obj>))]
[JsonSerializable(typeof(global::Tsinswreng.CsErr.IAppErrView))]
[JsonSerializable(typeof(global::System.Collections.Generic.IList<global::Tsinswreng.CsErr.IAppErrView>))]

[JsonSerializable(typeof(global::Tsinswreng.CsErr.AppErrView))]
[JsonSerializable(typeof(global::System.Collections.Generic.IList<global::Tsinswreng.CsErr.AppErrView>))]

[JsonSerializable(typeof(IList<i8>))]
[JsonSerializable(typeof(IList<u8>))]
[JsonSerializable(typeof(IList<i16>))]
[JsonSerializable(typeof(IList<u16>))]
[JsonSerializable(typeof(IList<i32>))]
[JsonSerializable(typeof(IList<i64>))]
[JsonSerializable(typeof(IList<u32>))]
[JsonSerializable(typeof(IList<u64>))]
[JsonSerializable(typeof(IList<f32>))]
[JsonSerializable(typeof(IList<f64>))]
[JsonSerializable(typeof(IList<bool>))]
[JsonSerializable(typeof(IList<char>))]
[JsonSerializable(typeof(IList<str>))]
file class Test{

}
#endif
