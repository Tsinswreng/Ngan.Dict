// namespace Tsinswreng.CsTempus;

// using System.Diagnostics.CodeAnalysis;
// using System.Globalization;
// using TPrimitive = i64;
// using TStruct = Tempus;

// public partial struct Tempus(TPrimitive V)
// 	: IEquatable<TStruct>
// 	//,I_Value<TPrimitive>
// 	//,IRawValueSerializable
// 	, IComparable {
// 	public static TStruct Zero = new(0);
// 	public TPrimitive Value { get; set; } = V;

// 	public Tempus() : this(DateTimeOffset.Now.ToUnixTimeMilliseconds()) {

// 	}

// 	// obj? I_ToSerialized.ToSerialized(obj? Obj){
// 	// 	if(Obj is Tempus T){
// 	// 		return T.Value;
// 	// 	}
// 	// 	return Obj;
// 	// }

// 	// obj? I_ToDeSerialized.ToDeSerialized(obj? Obj) {
// 	// 	if(Obj is i64 I64){
// 	// 		return new Tempus(I64);
// 	// 	}
// 	// 	if(Obj is DateTime Dt){
// 	// 		return FromDateTime(Dt);
// 	// 	}
// 	// 	return Obj;
// 	// }

// 	public static TStruct FromUnixMs(i64 Ms) {
// 		return new(Ms);
// 	}

// 	public static TStruct FromDateTime(DateTime dt) {
// 		long ms = new DateTimeOffset(dt).ToUnixTimeMilliseconds();
// 		return FromUnixMs(ms);
// 	}

// 	public static TStruct Now() {
// 		return new TStruct();
// 	}

// 	public static TStruct FromIso(str Iso) {
// 		return DateTimeOffset.ParseExact(
// 			Iso
// 			, "yyyy-MM-ddTHH:mm:ss.fffzzz", CultureInfo.InvariantCulture
// 		).ToUnixTimeMilliseconds();
// 	}

// 	public static implicit operator TPrimitive(TStruct e) {
// 		return e.Value;
// 	}
// 	public static implicit operator TStruct(TPrimitive s) {
// 		return new TStruct(s);
// 	}
// 	public static implicit operator DateTime(TStruct Tempus) {
// 		return DateTimeOffset.FromUnixTimeMilliseconds(Tempus.Value).LocalDateTime;
// 	}

// 	public static implicit operator TStruct(DateTime DateTime) {
// 		return new DateTimeOffset(DateTime).ToUnixTimeMilliseconds();
// 	}

// 	public override bool Equals([NotNullWhen(true)] object obj) {
// 		return obj is TStruct learn && Value.Equals(learn.Value);
// 	}

// 	public bool Equals(TStruct other) {
// 		return Value.Equals(other.Value);
// 	}

// 	public static bool operator ==(TStruct left, TStruct right) {
// 		return left.Equals(right);
// 	}

// 	public static bool operator !=(TStruct left, TStruct right) {
// 		return !left.Equals(right);
// 	}

// 	public static bool operator >(TStruct left, TStruct right) {
// 		return left.Value > right.Value;
// 	}

// 	public static bool operator <(TStruct left, TStruct right) {
// 		return left.Value < right.Value;
// 	}

// 	public override int GetHashCode() {
// 		return Value.GetHashCode();
// 	}

// 	public int CompareTo(object obj) {
// 		if (obj is TStruct t) {
// 			return Value.CompareTo(t.Value);
// 		}
// 		return Value.CompareTo(obj);
// 	}
// }

// public class InMillisecond {
// 	public const i64 Second = 1000;
// 	public const i64 Minute = 60 * Second;
// 	public const i64 Hour = 60 * Minute;
// 	public const i64 Day = 24 * Hour;
// 	public const i64 Week = 7 * Day;
// 	public const i64 Month = 30 * Day;
// 	public const i64 Year = 365 * Day;
// }

// public static class ExtnTempus {
// 	public static str ToIso(this TStruct z) {
// 		return DateTimeOffset.FromUnixTimeMilliseconds(z.Value)
// 		.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz", CultureInfo.InvariantCulture);
// 	}

// 	extension(TStruct z) {
// 		public static bool TryFromIso(str Iso, out TStruct R) {
// 			R = default;
// 			if (string.IsNullOrWhiteSpace(Iso)) return false;
// 			const string fmt = "yyyy-MM-ddTHH:mm:ss.fffzzz";   // 含毫秒與時區
// 			if (
// 				DateTimeOffset.TryParseExact(
// 					Iso, fmt,
// 					CultureInfo.InvariantCulture,
// 					DateTimeStyles.None,
// 					out var dto
// 				)
// 			) {
// 				R = dto.ToUnixTimeMilliseconds();
// 				return true;
// 			}
// 			return false;
// 		}
// 	}

// }
