#define Impl
namespace Ngaq.Core.Shared.Word.Models.Dto;

using Ngaq.Core.Infra;
using Ngaq.Core.Infra.IF;
using Tsinswreng.CsTempus;

public class DtoCompressedWords
	:IWordsPackInfo
	,IAppSerializable
{
	public u8[]? Data{get;set;}
	public UnixMs CreatedAt{get;set;} = UnixMs.Now();
	public EWordsPack Type{get;set;} = EWordsPack.None;
	public Version? VerJnWord{get;set;} = JnWord.ClassVer;
	public Version? Ver{get;set;}
	#if Impl
	= IWordsPackInfo.ClassVer;
	#endif
	public Version? VerApp{get; set;}
	#if Impl
	= AppVer.Inst.Ver;
	#endif
}

public static class ExtnDtoCompressedWords{
	public static IWordsPackInfo ToOrAssWordsPackInfo(this DtoCompressedWords z){
		return z;
	}
}
