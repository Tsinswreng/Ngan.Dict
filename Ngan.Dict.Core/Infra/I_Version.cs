namespace Ngaq.Core.Infra;

public interface I_ClassVer{
	public static abstract Version ClassVer{get;}
}


public interface I_VerApp{
	#if false
	//默認實現 序列化旹無此字段
	public Version Version{get=>AppVersion.Inst.Version; set{}}
	#endif

	public Version? VerApp{get; set;}
	#if Impl
	= AppVer.Inst.Ver;
	#endif
}
