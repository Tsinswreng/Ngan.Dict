namespace Ngaq.Ui.Icons;
using TStruct = Svg;
public record struct Svg(str V){
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
	
}
