namespace Ngaq.Core.Shared.Base.Models;

public class Neo_Changed<T>{
	public IList<T> NeoPart{get;set;} = new List<T>();
	public IList<T> ChangedPart{get;set;} = new List<T>();
}
