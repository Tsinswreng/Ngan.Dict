namespace Ngaq.Core.Shared.Word.Models.Po.Kv;
public static class ExtnPoKv{
	public static TSelf SetStrStr<TSelf>(this TSelf z, str K, str V)
		where TSelf : class, IPoKv
	{
		z.KType = EKvType.Str;
		z.VType = EKvType.Str;
		z.KStr = K;
		z.VStr = V;
		return z;
	}

	public static TSelf SetStrI64<TSelf>(this TSelf z, str K, i64 V)
		where TSelf : class, IPoKv
	{
		z.KType = EKvType.Str;
		z.VType = EKvType.I64;
		z.KStr = K;
		z.VI64 = V;
		return z;
	}

	public static str? GetVStr(this IPoKv z){
		if(z.VType == EKvType.Str){
			return z.VStr;
		}
		throw new InvalidOperationException("VType is not Str.");
	}

	public static i64 GetVI64(this IPoKv z){
		if(z.VType == EKvType.I64){
			return z.VI64;
		}
		throw new InvalidOperationException("VType is not i64.");
	}

	public static obj GetKey(this IPoKv z){
		obj Key = null!;
		if(z.KType == EKvType.Str){
			Key = z.KStr!;
		}else{
			Key = z.KI64;
		}
		return Key;
	}
}
