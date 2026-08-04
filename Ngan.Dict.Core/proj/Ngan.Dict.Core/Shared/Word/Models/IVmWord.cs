namespace Ngan.Dict.Core.Word.Models;

using Ngan.Dict.Core.Shared.Base.Models.Po;
using Ngan.Dict.Core.Shared.Word.Models.Po.Word;

public  partial interface IVmWord
	:AppI_Id<str>
	,IHeadLangWord
{

	// public ObservableCollection<str> Summarys{get;set;}
	// #if Impl
	// = new();
	// #endif


}
