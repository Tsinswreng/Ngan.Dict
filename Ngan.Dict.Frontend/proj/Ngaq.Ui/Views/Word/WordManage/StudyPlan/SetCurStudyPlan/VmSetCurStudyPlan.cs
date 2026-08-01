namespace Ngaq.Ui.Views.Word.WordManage.StudyPlan.SetCurStudyPlan;

using System.Collections.ObjectModel;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Shared.StudyPlan.Models;
using Ngaq.Core.Shared.StudyPlan.Models.Po.StudyPlan;
using Ngaq.Core.Shared.StudyPlan.Svc;
using Ngaq.Ui.Infra;

using Ctx = VmSetCurStudyPlan;using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

/// 「設置當前學習方案」頁面的 ViewModel。
/// 負責讀取當前學習方案，並提供恢復內置方案的操作。
public partial class VmSetCurStudyPlan: ViewModelBase, IMk<Ctx>{
	protected VmSetCurStudyPlan(){}

	public static Ctx Mk(){
		return new Ctx();
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmSetCurStudyPlan(){
		#if DEBUG
		{
			var o = new Ctx();
			Samples.Add(o);
		}
		#endif
	}

	ISvcStudyPlan? SvcStudyPlan{get;set;}
	IFrontendUserCtxMgr? UserCtxMgr{get;set;}

	/// 依賴注入構造器。
	public VmSetCurStudyPlan(
		ISvcStudyPlan? SvcStudyPlan
		,IFrontendUserCtxMgr? UserCtxMgr
	){
		this.SvcStudyPlan = SvcStudyPlan;
		this.UserCtxMgr = UserCtxMgr;
	}

	public str CurId{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public str CurUniqName{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public str CurDescr{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	/// 当前已加载/已选中的 StudyPlan。
	/// 供 View 层跳转到编辑页时复用，避免重复查询。
	public PoStudyPlan? CurPoStudyPlan{get;private set;}

	/// 讀取後端「當前學習方案」。
	/// 成功後刷新當前方案的展示字段。
	public async Task<nil> LoadCurStudyPlan(CT Ct = default){
		if(AnyNull(SvcStudyPlan, UserCtxMgr)){
			return NIL;
		}
		try{
			var dbCtx = UserCtxMgr.GetDbUserCtx();
			var jn = await SvcStudyPlan.GetCurJnStudyPlan(dbCtx, Ct);
			BoStudyPlan? bo = null;
			if(jn is not null){
				bo = new BoStudyPlan{
					PoStudyPlan = jn.StudyPlan,
					PoPreFilter = jn.PreFilter,
					PoWeightCalculator = jn.WeightCalculator,
					PoWeightArg = jn.WeightArg,
				};
			}
			AssignCurFields(bo?.PoStudyPlan);
		}catch(Exception e){
			HandleErr(e);
		}
		return NIL;
	}

	/// 把所选學習方案設為當前方案，設置成功後刷新展示字段。
	public async Task<nil> ApplySelectedStudyPlan(PoStudyPlan? PoStudyPlan, CT Ct = default){
		if(AnyNull(SvcStudyPlan, UserCtxMgr) || PoStudyPlan is null){
			return NIL;
		}
		try{
			await SvcStudyPlan.SetCurStudyPlanId(UserCtxMgr.GetDbUserCtx(), PoStudyPlan.Id, Ct);
			AssignCurFields(PoStudyPlan);
		}catch(Exception e){
			HandleErr(e);
		}
		return NIL;
	}

	/// 在「選擇」流程中暫存候選 StudyPlan，暫不調用後端接口。
	public nil SelectCandidateStudyPlan(PoStudyPlan? poStudyPlan){
		AssignCurFields(poStudyPlan);
		return NIL;
	}

	/// 把當前候選方案提交為「當前學習方案」。此函數給 OpBtn 綁定。
	public async Task<nil> CommitSelectedStudyPlan(CT Ct = default){
		return await ApplySelectedStudyPlan(CurPoStudyPlan, Ct);
	}

	/// 把內置學習方案恢復回數據庫，然後重新載入當前學習方案。
	public async Task<nil> RestoreBuiltin(CT Ct = default){
		if(AnyNull(SvcStudyPlan, UserCtxMgr)){
			return NIL;
		}
		try{
			await SvcStudyPlan.RestoreBuiltinStudyPlan(UserCtxMgr.GetDbUserCtx(), Ct);
			ShowDialog(I18n[K.RestoreBuiltinDone]);
			return await LoadCurStudyPlan(Ct);
		}catch(Exception e){
			HandleErr(e);
		}
		return NIL;
	}

	void AssignCurFields(PoStudyPlan? poStudyPlan){
		CurPoStudyPlan = poStudyPlan;
		CurId = poStudyPlan?.Id.ToString() ?? "";
		CurUniqName = poStudyPlan?.UniqName ?? "";
		CurDescr = poStudyPlan?.Descr ?? "";
	}
}

