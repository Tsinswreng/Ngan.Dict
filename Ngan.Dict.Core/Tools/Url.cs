namespace Ngaq.Core.Tools;
using Tsinswreng.CsTools;
using TStruct = Url;


/// 專用于 R.MapPost 等

/// <param name="V"></param>
public record struct Url(str V){
	public str Value => V;
	public static implicit operator str(TStruct e){
		return e.Value;
	}
	public static implicit operator TStruct(str s){
		return new(s);
	}
	public override string ToString() {
		return Value;
	}
	public static TStruct Mk(
		TStruct? Parent
		,IList<str> Path
		//,str DfltValue = default!
	){
		Parent??= ""; //當Parent潙null旹 整ʹUrl字串即以"/"開頭、合ʴ要求芝`R.MapPost`首ʹUrlˉ參數ʹ
		return ToolPath.SlashTrimEtJoin([Parent, ..Path]);
		// if(Parent is null){
		// 	return new Url(ToolPath.SlashTrimEtJoin(Path));
		// }
		// return ToolPath.SlashTrimEtJoin([Parent, ..Path]);
	}
}
