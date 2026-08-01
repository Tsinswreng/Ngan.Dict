using Ngaq.Core.Shared.Word.Models.Learn_;

namespace Ngaq.Core.Word.WeightAlgo.Models;


public partial interface IWeightWord
	:I_IdWord
	,I_Weight
	,I_StrKey_Props
	,I_LearnRecords
	,I_PrevTurnLearnRecords
{

}
