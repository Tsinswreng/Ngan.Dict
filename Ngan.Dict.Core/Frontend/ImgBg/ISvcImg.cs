namespace Ngaq.Core.Frontend.ImgBg;

public class ReqImg{}

public enum EImgDataType{
	Unknown = 0,
	Stream = 1
}

public class DtoImg{
	public str Path{get;set;}
	public EImgDataType ImgDataType{get;set;}
	public obj? Data{get;set;}
}

public interface ISvcImg{

}
