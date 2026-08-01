namespace Ngaq.Core.Models.Sys.Po.RolePermission;

using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Model.Po.Role;
using Ngaq.Core.Model.Sys.Po.RolePermission;
using Ngaq.Core.Models.Sys.Po.Permission;

public class PoRolePermission
	:PoBase
	,AppI_Id<IdRolePermission>
{
	public IdRolePermission Id{get;set;}
	public IdRole RoleId{get;set;}
	public IdPermission PermissionId{get;set;}


}
