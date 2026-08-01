
namespace Ngaq.Core.Shared.Base.Models;

public partial struct Existing_Dupli<T>{
	public Existing_Dupli(
		T Existing
		,T Duplication
	){
		this.Existing = Existing;
		this.Dupli = Duplication;
	}

	/// 既存于數據源中者
	public T Existing { get; set; }
	/// 與[既存]者褈者
	public T Dupli { get; set; }
}

public partial class DupliGroupList<T>{
	public IList<Existing_Dupli<T>> Existing_Dupli{get;set;}
	// /// 原已存在(當是得從數據源)
	// public IEnumerable<T>? Existings { get; set; }

	// /// 與[原已存在]褈者
	// public IEnumerable<T>? Duplicates{get;set;}

	/// 未存在(待添加)
	public IList<T>? NonExistings { get; set; }
}


public partial class DupliGroupAsyE<T>{
	public IAsyncEnumerable<Existing_Dupli<T>> Existing_Dupli{get;set;}
	public IAsyncEnumerable<T>? NonExistings { get; set; }
}
