using Ngaq.Core.Infra.IF;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.StudyPlan.Models.Po.PreFilter;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightArg;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightCalculator;
using Ngaq.Core.Shared.User.Models.Po.User;
using Tsinswreng.CsSql;

namespace Ngaq.Core.Shared.StudyPlan.Models.Po.StudyPlan;

[Doc($@"學習方案持久化實體")]
public class PoStudyPlan
	:PoBaseBizTime
	,AppI_Id<IdStudyPlan>
	,IAggRoot
	,I_Owner
	,I_UniqName
{
	[Doc($@"主鍵")]
	public IdStudyPlan Id{get;set;} = new();
	[Doc($@"擁有者")]
	public IdUser Owner{get;set;} = IdUser.Zero;
	[Doc($@"用戶側唯一名")]
	public str? UniqName{get;set;} = null;
	[Doc($@"描述")]
	public str Descr{get;set;} = "";
	[Doc($@"關聯的前置篩選器ID(非獨占)")]
	public IdPreFilter PreFilterId{get;set;} = IdPreFilter.Zero;
	[Doc($@"關聯的權重算法ID(非獨占)")]
	public IdWeightCalculator WeightCalculatorId{get;set;} = IdWeightCalculator.Zero;
	[Doc($@"關聯的權重參數ID(非獨占)")]
	public IdWeightArg WeightArgId{get;set;} = IdWeightArg.Zero;

}
