namespace Ngan.Dict.Core.Infra;
using System.Text.Json.Serialization;
public partial class AppJsonCtx : JsonSerializerContext {
	public static IList<JsonConverter> JsonConverters = [
	new global::Ngan.Dict.Core.Tools.JsonConvtr_Tempus(),
new global::Ngan.Dict.Core.Tools.JsonConvtr<global::Ngan.Dict.Core.Shared.User.Models.Po.Device.IdClient>(),
new global::Ngan.Dict.Core.Tools.JsonConvtr<global::Ngan.Dict.Core.Shared.User.Models.Po.IdDel>(),
new global::Ngan.Dict.Core.Tools.JsonConvtr<global::Ngan.Dict.Core.Shared.Kv.Models.IdKv>(),
new global::Ngan.Dict.Core.Tools.JsonConvtr<global::Ngan.Dict.Core.Shared.Dictionary.Models.Po.NormLang.IdNormLang>(),
new global::Ngan.Dict.Core.Tools.JsonConvtr<global::Ngan.Dict.Core.Shared.Word.Models.Po.NormLangToUserLang.IdNormLangToUserLang>(),
new global::Ngan.Dict.Core.Tools.JsonConvtr<global::Ngan.Dict.Core.Model.Sys.Po.Password.IdPassword>(),
new global::Ngan.Dict.Core.Tools.JsonConvtr<global::Ngan.Dict.Core.Models.Sys.Po.Permission.IdPermission>(),
new global::Ngan.Dict.Core.Tools.JsonConvtr<global::Ngan.Dict.Core.Shared.StudyPlan.Models.Po.PreFilter.IdPreFilter>(),
new global::Ngan.Dict.Core.Tools.JsonConvtr<global::Ngan.Dict.Core.Shared.RecentUse.Models.Po.RecentUse.IdRecentUse>(),
new global::Ngan.Dict.Core.Tools.JsonConvtr<global::Ngan.Dict.Core.Model.Sys.Po.RefreshToken.IdRefreshToken>(),
new global::Ngan.Dict.Core.Tools.JsonConvtr<global::Ngan.Dict.Core.Model.Po.Role.IdRole>(),
new global::Ngan.Dict.Core.Tools.JsonConvtr<global::Ngan.Dict.Core.Model.Sys.Po.RolePermission.IdRolePermission>(),
new global::Ngan.Dict.Core.Tools.JsonConvtr<global::Ngan.Dict.Core.Shared.StudyPlan.Models.Po.StudyPlan.IdStudyPlan>(),
new global::Ngan.Dict.Core.Tools.JsonConvtr<global::Ngan.Dict.Core.Shared.User.Models.Po.User.IdUser>(),
new global::Ngan.Dict.Core.Tools.JsonConvtr<global::Ngan.Dict.Core.Shared.Word.Models.Po.UserLang.IdUserLang>(),
new global::Ngan.Dict.Core.Tools.JsonConvtr<global::Ngan.Dict.Core.Models.Sys.Po.UserRole.IdUserRole>(),
new global::Ngan.Dict.Core.Tools.JsonConvtr<global::Ngan.Dict.Core.Shared.StudyPlan.Models.Po.WeightArg.IdWeightArg>(),
new global::Ngan.Dict.Core.Tools.JsonConvtr<global::Ngan.Dict.Core.Shared.StudyPlan.Models.Po.WeightCalculator.IdWeightCalculator>(),
new global::Ngan.Dict.Core.Tools.JsonConvtr<global::Ngan.Dict.Core.Shared.Word.Models.Po.Word.IdWord>(),
new global::Ngan.Dict.Core.Tools.JsonConvtr<global::Ngan.Dict.Core.Model.Po.Learn_.IdWordLearn>(),
new global::Ngan.Dict.Core.Tools.JsonConvtr<global::Ngan.Dict.Core.Model.Po.Kv.IdWordProp>(),
new global::Ngan.Dict.Core.Tools.JsonConvtr<global::Ngan.Dict.Core.Models.Po.PoStatus>(),
	];
}
