#define Impl
namespace Ngan.Dict.Core.Shared.Word.Models.Dto;

using Ngan.Dict.Core.Infra;
using Ngan.Dict.Core.Shared.Base.Models.Req;
using Tsinswreng.CsTempus;

public class ReqPackWords : BaseReq, IWordsPackInfo{
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
