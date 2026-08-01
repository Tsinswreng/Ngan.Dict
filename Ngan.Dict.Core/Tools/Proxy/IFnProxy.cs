namespace Ngan.Dict.Core.Tools.Proxy;

public interface IFnProxy{
	public Func<obj?> FnGet{get;set;}
	public Func<obj?, obj?> FnSet{get;set;}
}

