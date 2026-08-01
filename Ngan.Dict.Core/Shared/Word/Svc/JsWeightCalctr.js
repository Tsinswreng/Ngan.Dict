// Ngaq JS weight calculator sample.
// Evaluate this script in Jint; it returns a JSON string.

(function () {
	function safeJsonParse(text, fallback) {
		try {
			if (text === null || text === undefined || text === "") {
				return fallback;
			}
			return JSON.parse(text);
		} catch {
			return fallback;
		}
	}

	function toNumber(value, fallback) {
		var n = Number(value);
		return Number.isFinite(n) ? n : fallback;
	}

	var words = safeJsonParse(Ngaq.WordsJson ?? "[]", []);
	var arg = safeJsonParse(Ngaq.CalcArgJson ?? "{}", {});

	var baseWeight = toNumber(arg.BaseWeight, 0);
	var step = toNumber(arg.Step, 1);
	var learnCountBoost = toNumber(arg.LearnCountBoost, 0.25);
	var prevTurnPenalty = toNumber(arg.PrevTurnPenalty, -0.5);
	var weightBoost = toNumber(arg.WeightBoost, 1);

	var results = words.map(function (word, index) {
		var learnRecords = Array.isArray(word.LearnRecords) ? word.LearnRecords : [];
		var prevTurnLearnRecords = Array.isArray(word.PrevTurnLearnRecords) ? word.PrevTurnLearnRecords : [];
		var existingWeight = toNumber(word.Weight, 0);
		var computedWeight = baseWeight
			+ index * step
			+ learnRecords.length * learnCountBoost
			+ prevTurnLearnRecords.length * prevTurnPenalty
			+ existingWeight * weightBoost;

		return {
			StrId: String(word.StrId ?? word.Id ?? word.IdStr ?? ""),
			Weight: computedWeight,
			Index: index
		};
	});

	return JSON.stringify({
		Opt: {
			SortBy: "Weight",
			ResultType: "AsyEIWordWeightResult"
		},
		Results: results,
		Props: {
			Algo: "JsWeightCalctrDemoV1",
			WordCount: words.length,
			Args: arg
		}
	});
})();