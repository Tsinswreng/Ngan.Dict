using Ngaq.Core.Infra.IF;
using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.StudyPlan.Models.Po.StudyPlan;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightArg;
using Ngaq.Core.Shared.StudyPlan.Models.Po.WeightCalculator;
using Ngaq.Core.Shared.User.Models.Po.User;

namespace Ngaq.Core.Shared.StudyPlan.Models.Po.PreFilter;

[Doc($@"學習方案前置篩選器持久化實體")]
public class PoPreFilter
	:PoBaseBizTime
	,AppI_Id<IdPreFilter>
	,I_Owner
	,I_UniqName
{
	[Doc($@"主鍵")]
	public IdPreFilter Id{get;set;} = new();
	[Doc($@"擁有者")]
	public IdUser Owner{get;set;} = IdUser.Zero;
	[Doc($@"用戶側唯一名")]
	public str? UniqName{get;set;} = null;
	[Doc($@"描述")]
	public str Descr{get;set;} = "";
	[Doc($@"數據格式類型")]
	public EPreFilterType Type{get;set;}
	[Doc($@"{nameof(Text)} 的結構版本")]
	/// Text 載荷ʹ結構版本號。如若 Text 爲 Json，斯 Ver 即其 Json 結構ʹ版本號。
	public Version DataSchemaVer{get;set;} = new();
	[Doc($@"篩選器二進制載荷")]
	/// u8[] 便于存 字符以外之數據
	/// 後續可能支持 腳本/可執行文件/程序 形式之prefilter。今尚未用及。
	public u8[]? Binary{get;set;} = null;
	[Doc($@"文本載荷。當 Type=Json 旹、此字段存放 Json 字符串。")]
	public str? Text{get;set;}


}
