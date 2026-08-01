namespace Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Core.Infra;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Tools;
using Tsinswreng.CsTempus;

public enum EWordsPack{
	
	None = 0,
	[Doc($$"""
	按行分隔之獨立JnWord Json、非Json數組。
	#Examples([
	```
		{"Id": "xxx",...}
		{"Id": "yyy",...}
	```
	])
	""")]
	LineSepJnWordJsonGZip,
	[Doc($$"""
	Json數組
	#Examples([
	```
		[
			{"Id": "xxx",...},
			{"Id": "yyy",...},
			...
		]
	```
	])
	""")]
	JnWordArrJsonGZip
}

public interface IWordsPackInfo
	:IAppSerializable
	,I_VerApp
{
	public static Version ClassVer = new Version(1,0,0);
	public EWordsPack Type{get;set;}
	public UnixMs CreatedAt{get;set;}
	[Doc(@"DTO版本")]
	public Version? Ver{get;set;}
	#if Impl
	= IWordsPackInfo.ClassVer;
	#endif
	
	[Doc(@$"{nameof(JnWord.ClassVer)}")]
	public Version? VerJnWord{get;set;}


}
