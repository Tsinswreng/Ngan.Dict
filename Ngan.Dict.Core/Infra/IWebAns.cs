namespace Ngaq.Core.Infra;

using System.Text.Json;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Infra.IF;
using Tsinswreng.CsErr;

public static class ExtnWebAns2{
	extension(WebAns z){
		public static IWebAns<T> Deserialize<T>(str Json){
			return WebAns.Deserialize<T>(
				Json
				,AppJsonCtx.Inst.IListAppErrView
				,AppJsonCtx.Inst.GetTypeInfo
			);
		}
	}

}
