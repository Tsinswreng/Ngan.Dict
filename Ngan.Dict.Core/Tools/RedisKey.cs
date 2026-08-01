namespace Ngaq.Core.Tools;
using Tsinswreng.CsTools;
using TStruct = RedisKey;


/// 專用于 R.MapPost 等

/// <param name="V"></param>
public record struct RedisKey(str V){
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
			return new TStruct(str.Join(":", Path));
		}
		return str.Join(":",[Parent, ..Path]);
	}
}
