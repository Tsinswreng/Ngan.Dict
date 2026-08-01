namespace Ngaq.Core.Word.Svc;

using System.Collections;
using System.Runtime.CompilerServices;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Tools.Json;
using Ngaq.Core.Word.Models;
using Ngaq.Core.Word.Models.Weight;
using Tsinswreng.CsTools;

[Doc(@$"
Words weight calculator interface.
")]

public partial interface IWeightCalctr{
	// [Obsolete(@$"use {nameof(Calc)} instead")]
	// public Task<IWeightResult> Calc(
	// 	IAsyncEnumerable<IWordForLearn> Word
	// 	,IJsonNode? CalcArg
	// 	,CT Ct
	// );
	
	public Task<IWeightResult> Calc(
		IAsyncEnumerable<IWordForLearn> Word
		,IDictionary<str, obj?>? CalcArg
		,CT Ct
	);

}
