using Ngaq.Core.Infra.IF;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.StudyPlan.Models.Po.PreFilter;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightCalculator;
using Ngaq.Core.Shared.User.Models.Po.User;

namespace Ngaq.Core.Shared.StudyPlan.Models.Po.WeightArg;

[Doc($@"權重算法參數持久化實體")]
public class PoWeightArg
	:PoBaseBizTime
	,AppI_Id<IdWeightArg>
	,I_Owner
	,I_UniqName
{
	[Doc($@"主鍵")]
	public IdWeightArg Id{get;set;} = new();
	[Doc($@"擁有者")]
	public IdUser Owner{get;set;} = IdUser.Zero;
	[Doc($@"用戶側唯一名")]
	public str? UniqName{get;set;} = null;
	[Doc($@"參數數據格式")]
	public EWeightArgType Type{get;set;} = EWeightArgType.Json;
	
	[Doc($@"字符串參數載荷。
	權重算法參數與權重算法相關、
	故無需如 `{nameof(PoPreFilter)}.{nameof(PoPreFilter.DataSchemaVer)}`
	之存其結構ʹ版本號
	")]
	public str? Text{get;set;}

	[Doc($@"二進制參數載荷(今未用及 預留備用)")]
	public u8[]? Binary{get;set;} = null;
	
	
	[Doc($@"關聯算法Id，參見 {nameof(PoWeightCalculator.Id)}")]
	public IdWeightCalculator WeightCalculatorId{get;set;} = IdWeightCalculator.Zero;

	[Doc($@"描述")]
	public str Descr{get;set;} = "";
}

