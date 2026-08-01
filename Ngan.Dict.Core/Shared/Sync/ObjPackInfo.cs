using Ngaq.Core.Infra;
using Ngaq.Core.Infra.IF;
using Tsinswreng.CsTempus;
using Tsinswreng.CsTextWithBlob;

namespace Ngaq.Core.Shared.Sync;



[Doc(@$"對象打包 元數據。
適用于作{nameof(ITextWithStream.Text)}
")]
public interface IObjPackInfo:IAppSerializable{
	public static Version ClassVer = new Version(1,0,0);
	
	[Doc(@$"{nameof(ITextWithStream.Payload)}。
	建議用字符串作此字段之實現類型。
	")]
	public obj? PayloadTypeObj{get;set;}
	
	public UnixMs CreatedAt{get;set;}
	
	[Doc(@"DTO版本")]
	public Version? Ver{get;set;}
	#if Impl
	= IObjPackInfo.ClassVer;
	#endif
	
	[Doc(@$"對象版本。
	#See[{nameof(I_ClassVer)}]
	")]
	public Version? ObjVer{get;set;}
}

public class ObjPackInfo:IObjPackInfo{
	public UnixMs CreatedAt{get;set;}
	public obj? PayloadTypeObj{get;set;}
	public Version? Ver{get;set;} = IObjPackInfo.ClassVer;
	public Version? ObjVer{get;set;}
}
