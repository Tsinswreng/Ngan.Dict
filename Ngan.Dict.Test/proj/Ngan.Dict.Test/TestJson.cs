using Ngan.Dict.Core.Infra;
using Ngan.Dict.Core.Infra.Errors;
using Ngan.Dict.Core.Shared.Kv.Models;
using Ngan.Dict.Core.Tools;
using Tsinswreng.CsErr;

namespace Ngan.Dict.Test;

public class TestJson
{
	public void Test(){
		var Kv = new PoKv();
		Kv.Id = new();
		System.Console.WriteLine(
			JSON.Stringify(Kv)
		);

		// IWebAns<PoKv> Ans = new WebAns<PoKv>();
		// Ans.Data = Kv;
		IWebAns<obj> Ans = new WebAns();
		Ans.Data = Kv;
		Ans.Errors??=new List<IAppErrView>();
		Ans.Errors.Add(AppErr.Mk(KeysErr.Common.ArgErr, ["UserName"]));
		var AnsJson = JSON.Stringify(Ans);
		System.Console.WriteLine(
			AnsJson
		);

		var ReParse = WebAns.Deserialize<PoKv>(AnsJson);
		System.Console.WriteLine(
			ReParse
		);
	}
}
