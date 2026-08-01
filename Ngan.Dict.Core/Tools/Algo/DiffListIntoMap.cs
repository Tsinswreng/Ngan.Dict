namespace Ngaq.Core.Tools.Algo;
public partial class Algo{

/*
取差集、前者有洏後者無
arr1=[
	{text:'a', num:1}
	,{text:'b', num:2}
	,{text:'c', num:3}
]
arr2=[
	{text:'a', num:1}
	,{text:'b', num:2}
	,{text:'d', num:4}
]
fn(arr1, arr2, (e)=>e.num) -> Map{3=>{text:'c', num:3}}
*/
	public static Dictionary<Fld, IList<ArrEle>> DiffListIntoDict<ArrEle, Fld>(
		IList<ArrEle> arr1
		,IList<ArrEle> arr2
		,Func<ArrEle, Fld> fn
	)where Fld:notnull
	{
		var map1 = Classify(arr1, fn);
		var map2 = Classify(arr2, fn);
		var ans = DiffMapByKey(map1, map2);
		return ans;
	}
}
