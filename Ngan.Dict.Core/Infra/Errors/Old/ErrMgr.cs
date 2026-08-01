namespace Ngaq.Core.Infra.Errors;
[Obsolete]
public partial class ErrMgr{
	protected static ErrMgr? _Inst = null;
	public static ErrMgr Inst => _Inst??= new ErrMgr();

	//public IDictionary<TypedStatus, nil> ErrSet{get;} = new Dictionary<TypedStatus, nil>();
	public nil Register(str Status, str? StatusType){
		// var key = new TypedStatus{
		// 	Status = Status
		// 	,StatusType = StatusType??""
		// };
		// //內部類異常枚舉中 具體異常設計潙工廠函數、每次創建異常對象即註冊一次
		// // if(ErrSet.ContainsKey(key)){
		// // 	throw new FatalLogicErr("Error already exists: "+key.ToString());
		// // }
		// //ErrSet.Set(key, Nil);
		// ErrSet[key] = NIL;
		return NIL;
	}
}
