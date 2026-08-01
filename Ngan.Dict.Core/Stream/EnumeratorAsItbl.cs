namespace Ngaq.Core.Iter;
public partial class EnumeratorAsItbl<T> : IIterable<T> {
	private readonly IEnumerator<T> _enumerator;
	private bool _hasNext;

	public EnumeratorAsItbl(IEnumerator<T> enumerator) {
		_enumerator = enumerator ?? throw new ArgumentNullException();
		_hasNext = enumerator.MoveNext(); // 预加载第一个元素
	}

	public bool HasNext() => _hasNext;

	public T Next() {
		//if (!_hasNext) throw new InvalidOperationException();
		var current = _enumerator.Current;
		_hasNext = _enumerator.MoveNext(); // 预加载下一个元素

		return current;
	}
}
