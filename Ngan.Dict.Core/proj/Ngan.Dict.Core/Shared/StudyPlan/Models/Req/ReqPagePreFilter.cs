using Ngan.Dict.Core.Shared.User.Models.Po.User;
using Tsinswreng.CsPage;

namespace Ngan.Dict.Core.Shared.StudyPlan.Models.Req;
public class ReqPagePreFilter{
	public IPageQry PageQry{get;set;}
	[Doc(@$"若空則不用此字段作篩選")]
	public str? UniqNameSearch{get;set;} = null;
	
}
