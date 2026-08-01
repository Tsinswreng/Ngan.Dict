namespace Ngaq.Core.Shared.Word.Models.Learn_;
using System.Diagnostics.CodeAnalysis;



#if false
public partial struct Learn
	:IEquatable<Learn>
{
	public Learn(str V){
		if(V == null){
			throw new ArgumentException("Learn can't be null");
		}
		this.Value = V;
	}
	public str Value{get;set;}

	public static implicit operator str(Learn e){
		return e.Value;
	}
	public static implicit operator Learn(str s){
		return new Learn(s);
	}

	public override bool Equals([NotNullWhen(true)] object? obj) {
		return obj is Learn learn && Value.Equals(learn.Value);
	}

	public bool Equals(Learn other) {
		return Value.Equals(other.Value);
	}

	public override int GetHashCode() {
		return Value.GetHashCode();
	}

}

#endif
