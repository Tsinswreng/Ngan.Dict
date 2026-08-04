using Ngan.Dict.Core.Shared.StudyPlan.Models;
using Ngan.Dict.Core.Shared.User.UserCtx;
using Ngan.Dict.Core.Shared.Word;
using Ngan.Dict.Core.Shared.Word.WeightAlgo;

namespace Ngan.Dict.Core.Shared.StudyPlan.Svc;

public interface IStudyPlanGetter{
	[Doc(@$"
	#See[{nameof(ExtnBoStudyPlan.FromJnStudyPlan)}]
	此接口專供 {nameof(MgrLearn)} 之用。
	{nameof(ISvcStudyPlan.GetCurBoStudyPlan)} 則側重于從數據庫讀取學習方案。
	要帶緩存、當學習方案未變時 則直接返回緩存的。不要每次都重新讀取並裝配。
	
	若用戶未配置 StudyPlan 則:
	{nameof(ISvcStudyPlan.EnsureCurStudyPlan)}
	")]
	public Task<BoStudyPlan> GetStudyPlan(
		IUserCtx User
		,CT Ct
	);
}
