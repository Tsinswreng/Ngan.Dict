namespace Ngaq.Core.Shared.Word.Models.Dto;

using Ngaq.Core.Infra;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Tools;
using Tsinswreng.CsTempus;

public class WordsPackInfo
	:IWordsPackInfo
	,IAppSerializable
{
	public EWordsPack Type{get;set;}
	public UnixMs CreatedAt{get;set;} = UnixMs.Now();
	public Version? Ver{get;set;} = IWordsPackInfo.ClassVer;
	public Version? VerJnWord{get;set;} = JnWord.ClassVer;
	public Version? VerApp{get;set;} = AppVer.Inst.Ver;
}

public static class ExtnWordsPackInfo{

	public static DtoCompressedWords ToDtoCompressedWords(
		this WordsPackInfo z
		,u8[] Bytes
	){
		var R = new DtoCompressedWords();
		R.Type = z.Type;
		R.Data = Bytes;
		return R;
	}
}
