namespace Ngaq.Core.Shared.Word.Models.Learn_;

using Ngaq.Core.Shared.Word.Models.Po.Learn;
using Tsinswreng.CsTools;


public partial interface I_Learn_Records{
	
	/// 值 按日期排序
	
	public IDictionary<ELearn, IList<ILearnRecord>> Learn_Records{get;set;}
	#if Impl
	= new Dictionary<Learn, IList<ILearnRecord>>();
	#endif
}


public static class ExtnI_Learn_Records{

	public static IDictionary<ELearn, IList<ILearnRecord>> AddFromLearnRecords(
		this IDictionary<ELearn, IList<ILearnRecord>> z
		,IEnumerable<ILearnRecord> LearnRecords
	){
		foreach(var LearnRecord in LearnRecords){
			var record = LearnRecord;
			z.AddInValues(
				record.Learn
				,record
			);
		}
		return z;
	}

	public static IDictionary<ELearn, IList<ILearnRecord>> AddFromPoLearns(
		this IDictionary<ELearn, IList<ILearnRecord>> z
		,IEnumerable<PoWordLearn> PoLearns
	){
		foreach(var PoLearn in PoLearns){
			var record = PoLearn.ToLearnRecord();
			z.AddInValues(
				record.Learn
				,record
			);
		}
		return z;
	}
}


