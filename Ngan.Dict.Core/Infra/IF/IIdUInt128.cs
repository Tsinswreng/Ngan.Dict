namespace Ngaq.Core.Infra.IF;

using Ngaq.Core.Infra.IF;
//using Tsinswreng.CsTools;
using Tsinswreng.CsU128Id;

public partial interface IStrongId<T>: I_Value<T>{

}

public partial interface IIdUInt128
	:IStrongId<UInt128>
	,IAppSerializable
{

}



public static class ExtnIIIdUInt128{
	extension<T>(T z)
		where T:struct, IIdUInt128
	{
		public static T Zero => default!;
		public u8[] ToByteArr(){
			return ToolU128Id.ToByteArr(z.Value);
		}
		public static T FromByteArr(byte[] bytes){
			var Num = ToolU128Id.ByteArrToUInt128(bytes);
			return new T(){Value = Num};
		}
		public static Tsinswreng.CsSql.IUpperTypeMapFnT<u8[], T> MkTypeMapFn(){
			return Tsinswreng.CsSql.UpperTypeMapFnT<u8[], T>.Mk(
				//(raw)=>T.FromByteArr(raw)
				(raw)=>FromByteArr<T>(raw)
				,(upper)=>upper.Value.ToByteArr()
			);
		}
		public static T FromLow64Base(string Low64Base){
			var Num = ToolU128Id.Low64BaseToUInt128(Low64Base);
			return new T(){Value = Num};
		}
		public static bool operator ==(T left, T right)
				=> left.Value == right.Value;
				
		public static bool operator !=(T left, T right)
				=> !(left == right);
				
		/// 尝试将Low64Base编码的字符串解析为PLACEHOLDERID
		/// <param name="S">Low64Base编码的字符串</param>
		/// <param name="R">解析成功时返回的PLACEHOLDERID，失败时为默认值</param>
		/// <returns>解析成功返回true，失败返回false</returns>
		public static bool TryParse(string? S, out T R){
			if(S is null){
				R = default!;
				return false;
			}
			if(ToolU128Id.TryLow64BaseToUInt128(S, out var Num)){
				R = new T(){Value = Num};
				return true;
			}
			R = default!;
			return false;
		}
		// 在FromLow64Base方法后添加以下代码
		/// 将Low64Base编码的字符串解析为PLACEHOLDERID
		/// <param name="s">Low64Base编码的字符串</param>
		/// <returns>解析得到的PLACEHOLDERID</returns>
		/// <exception cref="ArgumentException">当输入字符串无效时抛出</exception>
		public static T Parse(string S){
			return FromLow64Base<T>(S);
		}
	}
}
