namespace Ngan.Dict.Core.Models.Sys.Po.RolePermission;

using Ngan.Dict.Core.Shared.Base.Models.Po;
using Ngan.Dict.Core.Model.Po.Role;
using Ngan.Dict.Core.Model.Sys.Po.RolePermission;
using Ngan.Dict.Core.Models.Sys.Po.Permission;

public class PoRolePermission
	:PoBase
	,AppI_Id<IdRolePermission>
{
	public IdRolePermission Id{get;set;}
	public IdRole RoleId{get;set;}
	public IdPermission PermissionId{get;set;}


}
