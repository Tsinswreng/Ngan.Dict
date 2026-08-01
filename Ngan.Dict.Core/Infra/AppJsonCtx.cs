namespace Ngaq.Core.Infra;

using System.Collections.Concurrent;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Shared.User.Models.Po;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Tools;
using Tsinswreng.CsIfaceGen;
using Tsinswreng.CsTools;

interface INull{

}

//JsonSerializable註解須寫在一起、不能散落在分散之partial class、否則json源生成器不輸出
//執行Ngaq.Core\GenAppJsonCtx.sh㕥自動生成帶註解ʹAppJsonCtx分部類
[IfaceGen(
	ParentType = typeof(IAppSerializable)
	//ParentType = typeof(INull)
	,Name = nameof(IAppSerializable)
	,OutDir = CfgIfaceGen.OutDir+nameof(IAppSerializable)
	,PhFullType = "TYPE"
	,PhIdentifierSafeFullType = "ID"
	,Template =
"""

[JsonSerializable(typeof(TYPE))]
[JsonSerializable(typeof(global::System.Collections.Generic.IList<TYPE>))]

"""

)]
public interface IIfaceGenCfg_AppJsonCtx{

}

/// ASP.net 內置Json序列化器專用。
/// 縱既配opt =>opt.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonCtx.Default);
/// 猶需手上 opt.SerializerOptions.Converters.Add(new MyConverter()......)
/// 佢不認AppJsonCtx中 用註解配˪ʹ Converters

[IfaceGen(
	ParentType = typeof(IRawValueSerializable)
	//ParentType = typeof(INull)
	,Name = nameof(IRawValueSerializable)+"List"
	,OutDir = CfgIfaceGen.OutDir+nameof(IRawValueSerializable)+"List"
	,PhFullType = "TYPE"
	,PhIdentifierSafeFullType = "ID"
	,Template =
"""
new global::Ngaq.Core.Tools.JsonConvtr<TYPE>(),

"""

)]
public interface IIfaceGenCfg_JsonCustomConverterList{

}


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
	// 	new global::Ngaq.Core.Tools.JsonConvtr<IdKv>(),
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


#if false //額外類型 自己手動複製粘貼到 Ngaq.Core/GenAppJsonCtx.sh

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
