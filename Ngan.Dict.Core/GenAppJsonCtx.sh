# 蔿䀬ʹ叶˪IAppSerializable之類型 生成StrAccType註解與JsonSerializable註解
# 需賴Tsinswreng.CsIfaceGen.Cli.exe 欲自編譯㞢則珩../CompileIfaceGenCli.sh
# JsonSerializable註解須寫在一起、不能散落在分散之partial class、否則json源生成器不輸出

mkdir -p Tsinswreng.SrcGen
../Tsinswreng.CsIfaceGen.Cli Ngaq.Core.csproj  Tsinswreng.SrcGen
# > Ngaq.Core/Tsinswreng.SrcGen/AppJsonCtx.g.cs

# 把分散之json序列化註解 集到一個文件中
mkdir -p Tsinswreng.SrcGen/Tsinswreng.CsIfaceGen/
cd Tsinswreng.SrcGen/Tsinswreng.CsIfaceGen/

##
IAppSerializable=IAppSerializable.g.cs

cat > $IAppSerializable <<'EOF'
namespace Ngaq.Core.Infra;
using System.Text.Json.Serialization;
EOF

cat IAppSerializable/* >> $IAppSerializable

cat >> $IAppSerializable <<'EOF'
//額外類型
[JsonSerializable(typeof(global::Tsinswreng.CsTempus.UnixMs))]
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
public partial class AppJsonCtx{}
EOF

rm IAppSerializable/*
##~

##IRawValueSerializable
IRawValueSerializable=IRawValueSerializable.g.cs

cat > $IRawValueSerializable <<'EOF'
namespace Ngaq.Core.Infra;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
[JsonSourceGenerationOptions(
	//PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
	ReadCommentHandling = JsonCommentHandling.Skip,
	UseStringEnumConverter = true
EOF

cat >> $IRawValueSerializable <<'EOF'
)]
public partial class AppJsonCtx{}
EOF
##~IRawValueSerializable



##IRawValueSerializableList
IRawValueSerializableList=IRawValueSerializableList.g.cs

cat > $IRawValueSerializableList <<'EOF'
namespace Ngaq.Core.Infra;
using System.Text.Json.Serialization;
public partial class AppJsonCtx : JsonSerializerContext {
	public static IList<JsonConverter> JsonConverters = [
	new global::Ngaq.Core.Tools.JsonConvtr_Tempus(),
EOF

cat IRawValueSerializableList/* >> $IRawValueSerializableList

cat >> $IRawValueSerializableList <<'EOF'
	];
}
EOF

rm IRawValueSerializableList/*
##~IRawValueSerializableList



rm CoreDictMapper.g.cs
cat CoreDictMapper/* >>  CoreDictMapper.g.cs
rm CoreDictMapper/*
