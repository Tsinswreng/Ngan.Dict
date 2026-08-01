#define Impl
namespace Ngaq.Core.Word.WeightAlgo.Models;

using System.Runtime.CompilerServices;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Shared.Word.Models.Po.Word;

public  partial class WeightWord
	: IWeightWord
{

	public static IWeightWord FromWordForLearn(IWordForLearn Word){
		return Unsafe.As<IWeightWord>(Word);
		//return (IWeightWord)Word.ShallowCloneSelf();
	}

	public IdWord Id {get;set;}
	public f64? Weight {get;set;}
	public IDictionary<str, IList<IProp>> StrKey_Props{get;set;}
	#if Impl
	= new Dictionary<str, IList<IProp>>();
	#endif
	public IList<ILearnRecord> LearnRecords{get;set;}
	#if Impl
	= new List<ILearnRecord>();
	#endif
	public IList<ILearnRecord> PrevTurnLearnRecords{get;set;}
	#if Impl
	= new List<ILearnRecord>();
	#endif

	// void test(){
	// 	IWeightWord w = new WordForLearn(null);
	// }
}
