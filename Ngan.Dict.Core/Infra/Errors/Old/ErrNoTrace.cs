namespace Ngaq.Core.Infra.Errors;
[Obsolete]
public partial class OldAppErr
	:Exception
{
	public str? Key{get;set;}
	public str? Msg{get;set;}
	public str? Namespace{get;set;}
	public IList<object?> Errors{get;set;} = new List<object?>();
	public IList<obj?> Args { get; set; }

	public override string ToString(){
		return Namespace+":"+Key+"\n"+Msg+str.Join("\n", Errors.Select(x=>x?.ToString()??""));
	}
	public OldAppErr ToErrBase(){
		return this;
	}
}
