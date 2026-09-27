using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Tsinswreng.CsSh;
using static Tsinswreng.CsSh.ShGlobal;

namespace Ngan.Dict.Scripts;

/// Implements the AppJsonCtx registration generation entry point.
internal static partial class GenAppJsonCtx{
	// 目標專案與輸出文件之相對路徑（相對於 Ngan 工作區根）
	const str CsprojRelPath = "Ngan.Dict/Ngan.Dict.Core/proj/Ngan.Dict.Core/Ngan.Dict.Core.csproj";
	const str OutRelPath = "Ngan.Dict/Ngan.Dict.Core/proj/Ngan.Dict.Core/Tsinswreng.SrcGen/Ngan.Dict.Gen.g.cs";
	// 標記接口：實現 IAppSerializable 者註冊 JsonSerializable；
	// 實現 IRawValueSerializable 者註冊自訂 JsonConverter（強類型值類型扁平化）。
	const str MarkerIAppSerializable = "Ngan.Dict.Core.Infra.IF.IAppSerializable";
	const str MarkerIRawValueSerializable = "Tsinswreng.CsCore.IRawValueSerializable";
	const str Ns = "Ngan.Dict.Core.Infra";

	// 手動額外類型：非本專案類型無法實現標記接口，須在此顯式列出（替代舊 sh 腳本中之「額外類型」塊）。
	static readonly str[] ExtraJsonTypes = [
		"global::Tsinswreng.CsTempus.UnixMs",
		"global::Tsinswreng.CsErr.IWebAns<global::System.Object>",
		"global::Tsinswreng.CsErr.IAppErrView",
		"global::System.Collections.Generic.IList<global::Tsinswreng.CsErr.IAppErrView>",
		"global::Tsinswreng.CsErr.AppErrView",
		"global::System.Collections.Generic.IList<global::Tsinswreng.CsErr.AppErrView>",
		"global::System.Collections.Generic.IList<global::System.SByte>",
		"global::System.Collections.Generic.IList<global::System.Byte>",
		"global::System.Collections.Generic.IList<global::System.Int16>",
		"global::System.Collections.Generic.IList<global::System.UInt16>",
		"global::System.Collections.Generic.IList<global::System.Int32>",
		"global::System.Collections.Generic.IList<global::System.Int64>",
		"global::System.Collections.Generic.IList<global::System.UInt32>",
		"global::System.Collections.Generic.IList<global::System.UInt64>",
		"global::System.Collections.Generic.IList<global::System.Single>",
		"global::System.Collections.Generic.IList<global::System.Double>",
		"global::System.Collections.Generic.IList<global::System.Boolean>",
		"global::System.Collections.Generic.IList<global::System.Char>",
		"global::System.Collections.Generic.IList<global::System.String>",
	];

	internal static async partial Task Main(string Root, CancellationToken Ct) {
		// step 1: 定位本機 MSBuild 並以 MSBuildWorkspace 載入 Ngan.Dict.Core 專案
		//（與 dotnet build 同一套解析邏輯：ProjectReference、NuGet 與框架引用）
		MSBuildLocator.RegisterDefaults();
		// 本腳本進程因 PublishAot 配置禁用了反射 JSON（runtimeconfig 之
		// JsonSerializer.IsReflectionEnabledByDefault=false），而 SDK 的 workload
		// manifest 解析依賴反射反序列化；Ngan.Dict.Core 不需要 workload，故禁用它以繞過。
		Environment.SetEnvironmentVariable("MSBuildEnableWorkloadResolver", "false");
		using var Workspace = MSBuildWorkspace.Create(new Dictionary<str, str>{
			["MSBuildEnableWorkloadResolver"] = "false",
		});
		Workspace.WorkspaceFailed += (_, E) => Console.Error.WriteLine($"[workspace] {E.Diagnostic.Message}");
		var Project = await Workspace.OpenProjectAsync(Root/CsprojRelPath, null, Ct);
		var Compilation = await Project.GetCompilationAsync(Ct);
		if(Compilation is null){
			throw new InvalidOperationException("GetCompilationAsync 返回 null");
		}

		// step 2: 收集標記接口之全部實現者（含接口自身、排除標記接口本身）
		var AppTypes = FindImplementers(Compilation, MarkerIAppSerializable).ToArray();
		var RawTypes = FindImplementers(Compilation, MarkerIRawValueSerializable).ToArray();

		// step 3: 組裝單文件並覆寫輸出
		// 統一換行為 LF：模板（raw string）換行隨源碼文件、Join 產出為 \n，避免生成文件混用。
		var Code = BuildCode(AppTypes, RawTypes).Replace("\r\n", "\n");
		var OutPath = Root/OutRelPath;
		Write(OutPath, Code);
		Echo($"GenAppJsonCtx: {AppTypes.Length} app types, {RawTypes.Length} raw types -> {OutPath}");
	}

	/// 依全限定名找標記接口，收集編譯單元源代碼中所有實現/繼承它的命名類型。
	/// 結果按全限定名排序，保證生成文件內容穩定、可 diff。
	static IEnumerable<INamedTypeSymbol> FindImplementers(Compilation Compilation, str MarkerFullName) {
		var Marker = Compilation.GetTypeByMetadataName(MarkerFullName)
			?? throw new InvalidOperationException($"marker not found: {MarkerFullName}");
		return Compilation.SourceModule.GlobalNamespace
			.GetAllNamedTypes()
			.Where(T => !SymbolEqualityComparer.Default.Equals(T, Marker) && IsSubType(T, Marker))
			.OrderBy(T => T.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), StringComparer.Ordinal);
	}

	/// 判斷 Candidate 是否為 Marker 之子類型：實現 Marker 接口或繼承 Marker 基類。
	static bool IsSubType(INamedTypeSymbol Candidate, INamedTypeSymbol Marker) {
		if(Candidate.AllInterfaces.Contains(Marker, SymbolEqualityComparer.Default)){
			return true;
		}
		for(var Base = Candidate.BaseType; Base is not null; Base = Base.BaseType){
			if(SymbolEqualityComparer.Default.Equals(Base, Marker)){
				return true;
			}
		}
		return false;
	}

	/// 遞歸收集命名空間下所有命名類型（含嵌套命名空間與嵌套類型）。
	static IEnumerable<INamedTypeSymbol> GetAllNamedTypes(this INamespaceSymbol Ns) {
		foreach(var Member in Ns.GetMembers()){
			switch(Member){
				case INamespaceSymbol NestedNs:
					foreach(var T in NestedNs.GetAllNamedTypes()){
						yield return T;
					}
					break;
				case INamedTypeSymbol Type:
					yield return Type;
					foreach(var Nested in Type.GetTypeMembers()){
						foreach(var T in Nested.GetAllNestedTypes()){
							yield return T;
						}
					}
					break;
			}
		}
	}

	/// 遞歸收集嵌套類型。
	static IEnumerable<INamedTypeSymbol> GetAllNestedTypes(this INamedTypeSymbol Type) {
		foreach(var Nested in Type.GetTypeMembers()){
			yield return Nested;
			foreach(var Deeper in Nested.GetAllNestedTypes()){
				yield return Deeper;
			}
		}
	}

	/// 組裝單文件內容：AppJsonCtx 三個 partial 聲明。
	/// 所有 [JsonSerializable] 集中在單一聲明——STJ 源生成器不支持分散於多個 partial 聲明。
	/// 模板用 raw string literal，避免對生成代碼逐行轉義。
	static str BuildCode(IReadOnlyList<INamedTypeSymbol> AppTypes, IReadOnlyList<INamedTypeSymbol> RawTypes) {
		var JsonLines = string.Join('\n', AppTypes.Select(T => JsonSerializablePair(FullName(T))));
		var ExtraLines = string.Join('\n', ExtraJsonTypes.Select(T => $"[JsonSerializable(typeof({T}))]"));
		var ConverterLines = string.Join('\n', RawTypes.Select(T => $"\tnew global::Ngan.Dict.Core.Tools.JsonConvtr<{FullName(T)}>(),"));

		return $$"""
		// <auto-generated/>
		// 由 Ngan.Dict.Scripts 的 GenAppJsonCtx 生成；勿手動編輯。
		namespace {{Ns}};
		using System.Text.Encodings.Web;
		using System.Text.Json;
		using System.Text.Json.Serialization;

		// ===== AppJsonCtx: JsonSerializable 註冊（集中於此 partial 聲明）=====
		{{JsonLines}}
		{{ExtraLines}}
		public partial class AppJsonCtx { }

		// ===== AppJsonCtx: JsonSourceGenerationOptions =====
		[JsonSourceGenerationOptions(
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			ReadCommentHandling = JsonCommentHandling.Skip,
			UseStringEnumConverter = true
		)]
		public partial class AppJsonCtx { }

		// ===== AppJsonCtx: JsonConverters（強類型值類型之轉換器）=====
		public partial class AppJsonCtx : JsonSerializerContext {
			public static IList<JsonConverter> JsonConverters = [
			new global::Ngan.Dict.Core.Tools.JsonConvtr_Tempus(),
		{{ConverterLines}}
			];
		}
		""";
	}

	/// 全限定名（含 global:: 前綴），與舊生成物格式一致。
	static str FullName(INamedTypeSymbol T) {
		return T.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
	}

	/// 一個類型的 JsonSerializable 註冊對：本體 + IList<>。
	static str JsonSerializablePair(str Full) {
		return $"""
		[JsonSerializable(typeof({Full}))]
		[JsonSerializable(typeof(global::System.Collections.Generic.IList<{Full}>))]
		""";
	}
}
