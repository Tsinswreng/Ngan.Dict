//#define Impl
namespace Ngaq.Core.Shared.Word.Models.Po.Word;

using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.User.Models.Po.User;

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
