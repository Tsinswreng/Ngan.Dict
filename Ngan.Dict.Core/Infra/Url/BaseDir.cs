namespace Ngaq.Core.Infra.Url;

public class BaseDirMgr:I_GetBaseDir{
	protected static BaseDirMgr? _Inst = null;
	public static BaseDirMgr Inst => _Inst??= new BaseDirMgr();
	public str _BaseDir{get;set;} = Directory.GetCurrentDirectory();
	public str GetBaseDir(){
		return _BaseDir;
	}
}

public static class ExtnBaseDirMgr{
	public static str Combine(
		this I_GetBaseDir z
		,params str[] Segs
	){
		return Path.Combine([z.GetBaseDir(), ..Segs]);
	}
}
