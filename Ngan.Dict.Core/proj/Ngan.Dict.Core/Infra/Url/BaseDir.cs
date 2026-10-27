namespace Ngan.Dict.Core.Infra.Url;

public class BaseDirMgr:I_GetBaseDir{
	public static BaseDirMgr Inst =>field??= new BaseDirMgr();
	public str _BaseDir{get;set;} = Directory.GetCurrentDirectory();
	public str GetBaseDir(){
		return _BaseDir;
	}
}

