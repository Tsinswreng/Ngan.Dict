namespace Ngan.Dict.Core.Shared.Word.Models.Learn_;

using System.ComponentModel;
using Ngan.Dict.Core.Infra.IF;
using Ngan.Dict.Core.Shared.Base.Models.Po;
using Ngan.Dict.Core.Shared.Word.Models.Po.Word;


public partial interface IWordForLearn
	:
	IAppSerializable
	,I_IdWord
	,IPoBase
	,IHeadLangWord
	,I_Index
	,I_Weight
	,I_Learn_Records
	,I_StrKey_Props
	,I_LearnRecords
	,I_PrevTurnLearnRecords
	,I_UnsavedLearnRecords
	,INotifyPropertyChanged
	,I_OnPropertyChanged //在類外觸發事件
	,I_ShallowCloneSelf
	,IBizCreateUpdateTime
{
	public IJnWord JnWord{get;set;}
}
