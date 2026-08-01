namespace Ngaq.Core.Tools;
using Tsinswreng.CsTools;
using TStruct = SlashSepKey;

///開頭不帶斜槓
public record struct SlashSepKey(str V){
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
		if(Parent is null){
			return new (ToolPath.SlashTrimEtJoin(Path));
		}
		return ToolPath.SlashTrimEtJoin([Parent, ..Path]);
	}
}
