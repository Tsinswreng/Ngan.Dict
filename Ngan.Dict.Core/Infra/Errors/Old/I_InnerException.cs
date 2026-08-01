namespace Ngaq.Core.Infra.Errors;

public partial interface I_InnerException{
	public Exception? InnerException { get; set;}
}
