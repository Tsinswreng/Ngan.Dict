using Ngaq.Core.Shared.Base.Models.Po;

namespace Ngaq.Core.Models.Sys.Po.Permission;


public class PoPermission
	:PoBase
	,AppI_Id<IdPermission>
{
	public IdPermission Id{get;set;}
	public str Code{get;set;} = "";
	public str Name{get;set;} = "";
	public str? Descr{get;set;} = "";

}
