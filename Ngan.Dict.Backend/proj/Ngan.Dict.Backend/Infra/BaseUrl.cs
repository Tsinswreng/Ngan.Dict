namespace Ngan.Dict.Backend.Infra;

using Ngan.Dict.Core.Infra.Cfg;
using Ngan.Dict.Core.Infra.Url;
using Tsinswreng.CsCfg;
using Tsinswreng.CsCore;



public partial class BaseUrl(
	ICfgAccessor CfgAccessor
):I_GetBaseUrl{
	[Impl]
	public str GetBaseUrl(){
		var V = CfgAccessor.Get(KeysClientCfg.ServerBaseUrl);

		return V??"";
	}
}
