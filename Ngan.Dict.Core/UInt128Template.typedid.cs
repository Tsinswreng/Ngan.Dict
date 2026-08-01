#if false//改完後勿忘複製入UInt128Template.typedid
//不能寫using語句 否則源生成 謬 //不能用 record struct 只能写 struct
//叵續ˣ定義無參構造器、蓋源生成器生成旹默認已給構造器。


partial struct PLACEHOLDERID
	:Ngaq.Core.Infra.IF.IIdUInt128
	,IEquatable<PLACEHOLDERID>
	,IRawValueSerializable
{

	object? I_ToDeSerialized.ToDeSerialized(object? Obj) {
		if(Obj is str s){
			return global::Ngaq.Core.Infra.IF.ExtnIIIdUInt128.FromLow64Base<PLACEHOLDERID>(s);
		}
		return Obj;
	}

	object? I_ToSerialized.ToSerialized(object? Obj) {
		if(Obj is PLACEHOLDERID){
			return Obj.ToString();
		}
		return Obj;
	}

	public PLACEHOLDERID():this(0){
		Value = Tsinswreng.CsU128Id.ToolU128Id.NewUlid<
			Tsinswreng.CsU128Id.U128Id
		>().Value;
	}

	public UInt128 Value { get; set; }
	public PLACEHOLDERID(UInt128 value) => Value = value;

	// public static explicit operator UInt128(PLACEHOLDERID id) => id.Value;
	// public static explicit operator PLACEHOLDERID(UInt128 value) => new(value);

	public static implicit operator UInt128(PLACEHOLDERID id) => id.Value;
	public static implicit operator PLACEHOLDERID(UInt128 value) => new(value);

	public override string ToString(){
		return Tsinswreng.CsU128Id.ToolU128Id.ToLow64Base(Value);
	}

	public static bool operator ==(PLACEHOLDERID left, PLACEHOLDERID right)
		=> left.Value == right.Value;

	public static bool operator !=(PLACEHOLDERID left, PLACEHOLDERID right)
		=> !(left == right);

	// 重写 object.Equals
	public override bool Equals(object? obj)
		=> obj is PLACEHOLDERID other && Equals(other);

	// 重写 GetHashCode
	public override int GetHashCode()
		=> Value.GetHashCode();

	public bool Equals(PLACEHOLDERID other) {
		return Value.Equals(other.Value);
	}

}

#endif
