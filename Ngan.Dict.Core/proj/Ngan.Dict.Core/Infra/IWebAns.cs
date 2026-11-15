namespace Ngan.Dict.Core.Infra;

using System.Text.Json.Serialization.Metadata;
using Tsinswreng.CsErr;
using Tsinswreng.CsErr.Results;

[Doc($$"""
{{nameof(Tsinswreng.CsErr.Results.WebAns)}} 的便捷反序列化：把本應用的 {{nameof(AppJsonCtx)}} 綁進去。

*為何要有這一層*：庫那版需要調用方自己交出「錯誤視圖清單的 JsonTypeInfo」與「按型別取元資料」兩個委託，
而那兩樣都只有本應用的 JSON 上下文知道。這一層把它們固定住，業務側就只寫
`WebAns.Deserialize<T>(body)`。
""")]
public static class ExtnWebAns2{
	extension(WebAns z){
		[Doc($$"""
		把響應體反序列化成 {nameof(IWebAns<T>)}。

		*錯誤清單一定用源生成的元資料*：錯誤視圖是跨進程契約，不能因為呼叫方忘了登記而退回反射
		（AOT 下會直接失敗）；`Data` 那一段才按 `T` 現查元資料。
		""")]
		public static IWebAns<T> Deserialize<T>(str Json){
			return WebAns.Deserialize<T>(
				Json
				,AppJsonCtx.Inst.IListTypedErrView
				,AppJsonCtx.Inst.GetTypeInfo!
			);
		}
	}
}
