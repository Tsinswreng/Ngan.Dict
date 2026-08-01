namespace Ngaq.Core.Tools;

public static class ExtnStruct{
	public static bool IsNullOrDefault<T>(this T? z)
		where T:struct, IEquatable<T>
	{
		return z is null || z.Value.Equals(default(T));
	}

	public static bool IsNullOrDefault<T>(this T z)
		where T:struct, IEquatable<T>
	{
		return z.Equals(default(T));
	}
}
