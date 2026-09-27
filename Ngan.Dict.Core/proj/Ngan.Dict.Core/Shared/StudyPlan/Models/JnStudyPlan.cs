using Ngan.Dict.Core.Infra;
using Ngan.Dict.Core.Infra.IF;
using Ngan.Dict.Core.Shared.StudyPlan.Models.Po.PreFilter;
using Ngan.Dict.Core.Shared.StudyPlan.Models.Po.StudyPlan;
using Ngan.Dict.Core.Shared.StudyPlan.Models.Po.WeightArg;
using Ngan.Dict.Core.Shared.StudyPlan.Models.Po.WeightCalculator;
using Ngan.Dict.Core.Word.Svc;
using Tsinswreng.CsSql;
using Tsinswreng.CsTools;
using Tsinswreng.CsRefl;
namespace Ngan.Dict.Core.Shared.StudyPlan.Models;

[Doc($@"學習方案聚合")]
public class JnStudyPlan
	:IAppSerializable
	,I_ClassVer
	,IAgg
{
	public static Version ClassVer{get;set;} = new Version(1,0);
	[Doc($@"學習方案主體")]
	public PoStudyPlan StudyPlan{get;set;}
	[Doc($@"前置篩選器；可空表示未配置")]
	public PoPreFilter? PreFilter{get;set;} = null;
	[Doc($@"權重算法；可空表示未配置")]
	public PoWeightCalculator? WeightCalculator{get;set;} = null;
	[Doc($@"權重參數；可空表示未配置")]
	public PoWeightArg? WeightArg{get;set;} = null;
}

public static class ExtnJnStudyPlan{
	extension(JnStudyPlan z){
		public IDictionary<str, obj> ToDict(
			ITypeInfoSrc? TypeInfoSrc
		){
			TypeInfoSrc??=AppTypeInfoSrc.Inst;
			var R = new Dictionary<str, obj>();
			R[nameof(JnStudyPlan.StudyPlan)] = TypeInfoSrc.ToInstViewDict(z.StudyPlan);
			// 可空之部件：無物件則無字典視圖（ToInstDict 不接 null）。
			R[nameof(JnStudyPlan.PreFilter)] = z.PreFilter is null ? null! : TypeInfoSrc.ToInstViewDict(z.PreFilter);
			R[nameof(JnStudyPlan.WeightCalculator)] = z.WeightCalculator is null ? null! : TypeInfoSrc.ToInstViewDict(z.WeightCalculator);
			R[nameof(JnStudyPlan.WeightArg)] = z.WeightArg is null ? null! : TypeInfoSrc.ToInstViewDict(z.WeightArg);
			return R;
		}
	}
}