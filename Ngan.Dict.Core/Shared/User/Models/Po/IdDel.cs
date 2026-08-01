namespace Ngaq.Core.Shared.User.Models.Po;

using Ngaq.Core.Infra;
using StronglyTypedIds;


using TStruct = IdDel;
using TPrimitive = i64;
using System.Diagnostics.CodeAnalysis;

public partial struct IdDel(TPrimitive V)
	:IEquatable<IdDel>
	,I_Value<TPrimitive>
	,IRawValueSerializable
{
	public TPrimitive Value{get;set;} = V;

	public IdDel():this(DateTimeOffset.Now.ToUnixTimeMilliseconds()){

	}

	obj? I_ToSerialized.ToSerialized(obj? Obj){
		if(Obj is IdDel T){
			return T.Value;
		}
		return Obj;
	}

	obj? I_ToDeSerialized.ToDeSerialized(obj? Obj) {
		if(Obj is i64 I64){
			return new IdDel(I64);
		}
		if(Obj is DateTime Dt){
			return FromDateTime(Dt);
		}
		return Obj;
	}

	public static IdDel FromUnixMs(i64 Ms ){
		return new IdDel(Ms);
	}

	public static IdDel FromDateTime(DateTime dt){
		long ms = new DateTimeOffset(dt).ToUnixTimeMilliseconds();
		return FromUnixMs(ms);
	}

	public static IdDel Now(){
		return new IdDel();
	}

	public static implicit operator TPrimitive(TStruct e){
		return e.Value;
	}
	public static implicit operator TStruct(TPrimitive s){
		return new TStruct(s);
	}


	public bool Equals(TStruct other) {
		return Value.Equals(other.Value);
	}

	public override int GetHashCode() {
		return Value.GetHashCode();
	}

}



#if false

/// 軟刪除標記 blobʹ ULID
/// 不用時間戳蔿防撞ⁿ觸唯一約束

[StronglyTypedId(ConstStrongTypeIdTemplate.UInt128)]
public partial struct IdDel {

}
#endif
