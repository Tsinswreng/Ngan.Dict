#define Impl
namespace Ngan.Dict.Core.Models.Sys.Po.Role;

using Ngan.Dict.Core.Shared.Base.Models.Po;
using Ngan.Dict.Core.Model.Po.Role;



public partial class PoRole
	:PoBase
	,AppI_Id<IdRole>
{

	public enum ERoleStatus{
		Normal = 0
	}

	public IdRole Id { get; set; }
	
	/// 全局唯一標識 如ADMIN
	
	public str Code {get;set;} = "";

	public str Name{get;set;} = "";

	public ERoleStatus Status{get;set;} = ERoleStatus.Normal;

}
