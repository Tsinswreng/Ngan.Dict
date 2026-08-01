namespace Ngan.Dict.Core.Word.Svc;

using System.Collections;
using System.Runtime.CompilerServices;
using Ngan.Dict.Core.Shared.Word.Models.Learn_;
using Ngan.Dict.Core.Tools.Json;
using Ngan.Dict.Core.Word.Models;
using Ngan.Dict.Core.Word.Models.Weight;
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
