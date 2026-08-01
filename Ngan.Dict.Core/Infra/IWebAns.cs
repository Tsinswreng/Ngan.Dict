namespace Ngan.Dict.Core.Infra;

using System.Text.Json;
using Ngan.Dict.Core.Infra.Errors;
using Ngan.Dict.Core.Infra.IF;
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
