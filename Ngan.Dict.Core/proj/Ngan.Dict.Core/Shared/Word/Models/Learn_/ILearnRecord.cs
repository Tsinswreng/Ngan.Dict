namespace Ngan.Dict.Core.Shared.Word.Models.Learn_;

public  partial interface ILearnRecord{
	
	/// 見ConstLearn
	
	public ELearn Learn{get;set;}
	public i64 UnixMs{get;set;}
}
