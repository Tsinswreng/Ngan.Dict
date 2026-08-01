//#define Impl
namespace Ngan.Dict.Core.Shared.Word.Models.Po.Word;

using Ngan.Dict.Core.Shared.Base.Models.Po;
using Ngan.Dict.Core.Shared.User.Models.Po.User;

public partial interface IHeadLangWord
	:I_Owner
{

	#region IHeadLangWord

	
	/// 詞形標識
	
	public str Head{get;set;}
	#if Impl
		="";
	#endif


	public str Lang{get;set;}
	#if Impl
		="";
	#endif


	#endregion IHeadLangWord
}
