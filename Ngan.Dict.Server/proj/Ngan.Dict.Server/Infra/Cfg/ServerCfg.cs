namespace Ngan.Dict.Server.Infra.Cfg;
using Ngan.Dict.Core.Infra.Cfg;
using Tsinswreng.CsCfg;

public partial class ServerCfg : JsonFileCfgAccessor, ICfgAccessor{
	protected static ServerCfg? _Inst = null;
	public static ServerCfg Inst => _Inst??= new ServerCfg();

}
