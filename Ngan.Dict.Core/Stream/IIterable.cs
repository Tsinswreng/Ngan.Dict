namespace Ngaq.Core.Iter;

public partial interface IIterable<out T> {
	bool HasNext();
	
	/// 不必判斷有無下一個元素、若用者欲判斷則先用HasNext()
	
	/// <returns></returns>
	T Next();
}
