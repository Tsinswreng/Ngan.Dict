namespace Ngaq.Core.Word.Models;

using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.Word.Models.Po.Word;

public  partial interface IVmWord
	:AppI_Id<str>
	,IHeadLangWord
{

	// public ObservableCollection<str> Summarys{get;set;}
	// #if Impl
	// = new();
	// #endif


}
