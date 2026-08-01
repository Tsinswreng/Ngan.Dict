namespace Ngaq.Core.Shared.StudyPlan.Models.Po.WeightCalculator;


[Doc($@"權重算法實現載體類型")]
[DoNotRenameMembers]
public enum EWeightCalculatorType{
	[Doc($@"未知")]
	Unknown,
	[Doc(@$"內置。
	使用內置權重算法時、 {nameof(PoWeightCalculator.UniqName)} 應當以 `__Builtin_` 開頭。
	")]
	Builtin,
	[Doc($@"JavaScript 腳本 版本1")]
	JsV1,
	
	// [Doc($@"命令行程序(暫不支持)")]
	// Cli,
	// [Doc($@"動態鏈接庫(暫不支持)")]
	// Dll,

}
