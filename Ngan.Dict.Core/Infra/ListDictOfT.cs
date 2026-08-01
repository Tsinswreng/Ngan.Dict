namespace Ngaq.Core.Infra;

using System;
using System.Collections.Generic;

public static class ListDictOfT {
	// 缓存 open type，避免每次都解析
	private static readonly Type? IListOpenType = typeof(IList<>);

	
	/// 若 <paramref name="t"/> 是 IList&lt;T&gt; 或其子类型，则返回具体的 T；
	/// 否则返回 null。兼容 AOT。
	
	public static Type? GetTOfIList(Type t) {
		if (t is null) return null;

		// 1. 本身就是 IList<T>
		if (t.IsGenericType && t.GetGenericTypeDefinition() == IListOpenType)
			return t.GetGenericArguments()[0];

		// 2. 在实现的接口里找
		foreach (var iface in t.GetInterfaces()) {
			if (iface.IsGenericType && iface.GetGenericTypeDefinition() == IListOpenType)
				return iface.GetGenericArguments()[0];
		}

		// 3. 递归看基类
		var baseType = t.BaseType;
		if (baseType is not null)
			return GetTOfIList(baseType);

		return null;
	}

	// 缓存 open type，避免每次都解析
	private static readonly Type? IDictOpenType = typeof(IDictionary<,>);

	
	/// 若 <paramref name="t"/> 是 IDictionary&lt;K,V&gt; 或其子类型，则返回 (K,V)；
	/// 否则返回 null。兼容 AOT。
	
	public static (Type key, Type value)? GetKvOfIDict(Type t) {
		if (t is null) return null;

		// 1. 本身就是 IDictionary<K,V>
		if (t.IsGenericType && t.GetGenericTypeDefinition() == IDictOpenType) {
			var args = t.GetGenericArguments();
			return (args[0], args[1]);
		}

		// 2. 在实现的接口里找
		foreach (var iface in t.GetInterfaces()) {
			if (iface.IsGenericType && iface.GetGenericTypeDefinition() == IDictOpenType) {
				var args = iface.GetGenericArguments();
				return (args[0], args[1]);
			}
		}

		// 3. 递归看基类
		var baseType = t.BaseType;
		if (baseType is not null)
			return GetKvOfIDict(baseType);

		return null;
	}
}
