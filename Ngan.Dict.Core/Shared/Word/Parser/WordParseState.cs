namespace Ngaq.Core.Service.Parser;

 // S_ : start
public enum WordParseState{
	Start
	,End
	,Metadata
	,TopSpace
	,S_Bracket
	,E_Bracket
	,S_Brace
	,E_Brace
	,S_AngleBracket
	,E_AngleBracket

	,DateBlock
	,DateBlock_date // [
	,Prop // [[
	,PropKey
	,PropValue
	,DateBlock_TopSpace
	,WordBlocks //{{
	,WordBlock
	,HeadOfWordDelimiter
	,WordBlock_TopSpace
	,WordBlockFirstLine
	,RestOfWordBlock //除去首行之后的部分
	,FirstLeftSquareBracketInWordBlockProp // [ 詞屬性之首中括號
	,WordBlockEnd // ````
	,DateBlockEnd
}