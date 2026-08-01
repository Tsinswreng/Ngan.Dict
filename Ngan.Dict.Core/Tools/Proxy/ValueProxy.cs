namespace Ngaq.Core.Tools.Proxy;
public class ValueProxy:IFnProxy{
	public obj? ValueObj{get;set;} = null;
	public Type? Type = null;
	public Func<obj?> FnGet{get;set;}
	public Func<obj?, obj?> FnSet{get;set;}
	// public UiText? DisplayName{get;set;} = null;
	// public UiText? Descr{get;set;} = null;
	public ValueProxy(){
		FnGet = ()=>ValueObj;
		FnSet = (obj? NeoData)=>ValueObj = NeoData;
	}
}
