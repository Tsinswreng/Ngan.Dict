using Ngaq.Core.Shared.StudyPlan.Models.Po.PreFilter;
using Ngaq.Core.Shared.StudyPlan.Models.PreFilter;

namespace Ngaq.Core.Shared.StudyPlan.Models;

public class BoPreFilter{
	public PoPreFilter PoPreFilter { get; set; } = new();
	public PreFilter.PreFilter PreFilter{get;set;} = new();
	
	[Doc(@$"原地賦值 要初始化 {nameof(PoPreFilter)} 和 {nameof(PreFilter)}.
	#See[{nameof(ExtnPreFilter.FromPo)}]
	")]
	public void FromPoPreFilter(PoPreFilter PoPreFilter){
		this.PoPreFilter = PoPreFilter;
		this.PreFilter = new PreFilter.PreFilter();
		this.PreFilter.FromPo(PoPreFilter);
	}
}
