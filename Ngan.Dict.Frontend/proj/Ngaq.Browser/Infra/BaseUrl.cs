namespace Ngaq.Browser.Infra;
using Ngaq.Core.Infra.Url;
using Tsinswreng.CsCore;

public  partial class BaseUrl: I_GetBaseUrl{
	[Impl]
	public str GetBaseUrl(){
		//todo試JsImport調window.location
		return "http://localhost:5000";
	}
}
