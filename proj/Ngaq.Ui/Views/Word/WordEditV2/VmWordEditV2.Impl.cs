namespace Ngaq.Ui.Views.Word.WordEditV2;

using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Model.Po.Kv;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Model.Po.Learn_;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Learn;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Shared.Word.Svc;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Views.Word.WordEditV2.PoWordEdit;
using Ngaq.Ui.Views.Word.WordEditV2.WordLearnEdit;
using Ngaq.Ui.Views.Word.WordEditV2.WordLearnPage;
using Ngaq.Ui.Views.Word.WordEditV2.WordPropEdit;
using Ngaq.Ui.Views.Word.WordEditV2.WordPropPage;
using Tsinswreng.CsTools;
using Tsinswreng.CsCore;
using Ctx = VmWordEditV2;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;
using Ngaq.Core.Tools;
using Tsinswreng.CsTempus;

/// 單詞編輯主頁 ViewModel。職責：協調三個分頁，統一保存/刪除。
public partial class VmWordEditV2{
	protected partial VmWordEditV2(){}
	public static partial Ctx Mk(){return new Ctx();}
	public partial VmWordEditV2(ISvcWordV2? SvcWordV2, IFrontendUserCtxMgr? UserCtxMgr){this.SvcWordV2 = SvcWordV2; this.UserCtxMgr = UserCtxMgr;}

	/// 由外部入口傳入單詞，初始化三個子頁狀態。
	public partial nil FromJnWord(IJnWord JnWord){
		Src = JnWord;
		Draft = JnWord.DeepClone().AsOrToJnWord();
		LoadFromDraft();
		return NIL;
	}

	/// 自由加詞入口：參照詞典保存新詞流程，直接建立可 Merge 的空白草稿。
	/// 同時補一條 Add 學習記錄，避免新詞缺少首次加入詞庫的事件。
	public partial nil InitFreeAddDraft(str Lang = ""){
		var now = UnixMs.Now();
		var draft = new JnWord{
			Word = new PoWord{
				Head = "",
				Lang = Lang,
				StoredAt = now,
				BizCreatedAt = now,
				BizUpdatedAt = now,
			},
			Props = [],
			Learns = [
				new PoWordLearn{
					Id = new IdWordLearn(),
					LearnResult = ELearn.Add,
					BizCreatedAt = now,
				}
			],
		};
		draft.EnsureForeignId();
		Src = null;
		Draft = draft;
		SaveMode = ESaveMode.Merge;
		LoadFromDraft();
		return NIL;
	}

	partial void LoadFromDraft(){
		if(Draft is null){
			return;
		}
		PoWordEdit.LoadFromPo(Draft.Word);
		WordPropPage.LoadFromPoProps(Draft.Props);
		WordLearnPage.LoadFromPoLearns(Draft.Learns);
		LastError = "";
		IsDirty = false;
	}

	public async partial Task<nil> Save(CT Ct){
		if(AnyNull(SvcWordV2, UserCtxMgr)){
			return NIL;
		}
		if(Draft is null){
			ShowDialog(I18n[K.NoDraft]);
			return NIL;
		}
		if(!TryApplyFormToDraft(out var err)){
			LastError = err;
			ShowDialog(err);
			return NIL;
		}
		try{
			Draft.EnsureForeignId();
			if(SaveMode == ESaveMode.Merge){
				await SaveByMerge(Ct);
			}else{
				await SaveByDetailOps(Ct);
			}
			Src = Draft.DeepClone().AsOrToJnWord();
			SaveMode = ESaveMode.DetailOps;
			LastError = "";
			IsDirty = false;
			ShowToast(I18n[K.Saved]);
		}catch(Exception ex){
			LastError = ex.Message;
			HandleErr(ex);
		}
		return NIL;
	}

	private async partial Task<nil> SaveByMerge(CT Ct){
		if(AnyNull(SvcWordV2, UserCtxMgr, Draft)){
			return NIL;
		}
		await SvcWordV2.MergeWord(
			UserCtxMgr.GetDbUserCtx(),
			ToolAsyE.ToAsyE([Draft]),
			Ct
		);
		return NIL;
	}

	private async partial Task<nil> SaveByDetailOps(CT Ct){
		if(AnyNull(SvcWordV2, UserCtxMgr, Draft)){
			return NIL;
		}
		var dbCtx = UserCtxMgr.GetDbUserCtx();
		var oldId = Draft.Word.Id;
		var finalId = await UpdRootAndGetFinalId(dbCtx, Ct);
		var hasMovedToOtherWord = oldId != finalId;
		if(hasMovedToOtherWord){
			Draft.SetIdEtEnsureFKey(finalId);
		}
		return NIL;
	}

	private async partial Task<IdWord> UpdRootAndGetFinalId(IDbUserCtx DbCtx, CT Ct){
		if(AnyNull(SvcWordV2, Draft)){
			return default;
		}
		var respAsyE = await SvcWordV2.OrdUpdPoWord(DbCtx, ToolAsyE.ToAsyE([Draft.Word]), Ct);
		var resp = await respAsyE.FirstOrDefaultAsync(Ct);
		if(resp is null){
			throw new InvalidOperationException("BatUpdPoWord returned empty response");
		}
		return resp.FinalId;
	}


	/// 單行已直接落庫刪除後，同步清理本地草稿與源數據，避免後續保存又把它帶回來。
	public partial void SyncDeletedPropToLocalState(IdWordProp Id){
		if(Draft is not null){
			Draft.Props = Draft.Props.Where(x=>x.Id != Id).ToList();
		}
		// 來源對象不一定是 JnWord 具體類，很多入口只保證是 IJnWord。
		// 直接按接口回寫，避免重新進編輯頁時仍讀到舊內存快照。
		if(Src is not null){
			Src.Props = Src.Props.Where(x=>x.Id != Id).ToList();
		}
	}

	/// Learn 的本地同步與 Prop 相同。
	public partial void SyncDeletedLearnToLocalState(IdWordLearn Id){
		if(Draft is not null){
			Draft.Learns = Draft.Learns.Where(x=>x.Id != Id).ToList();
		}
		// 同上：回寫接口對象本身，確保列表卡片/詳情頁再次打開時拿到的是新狀態。
		if(Src is not null){
			Src.Learns = Src.Learns.Where(x=>x.Id != Id).ToList();
		}
	}

	public async partial Task<nil> Delete(CT Ct){
		if(AnyNull(SvcWordV2, UserCtxMgr, Draft)){
			return NIL;
		}
		try{
			await SvcWordV2.SoftDelPoWordInId(
				UserCtxMgr.GetDbUserCtx(),
				ToolAsyE.ToAsyE([Draft.Word.Id]),
				Ct
			);
			LastError = "";
			IsDirty = false;
			ShowToast(I18n[K.Deleted]);
			ViewNavi?.Back();
		}catch(Exception ex){
			LastError = ex.Message;
			HandleErr(ex);
		}
		return NIL;
	}

	private partial bool TryApplyFormToDraft(out str Err){
		Err = "";
		if(Draft is null){
			Err = I18n[K.NoDraft];
			return false;
		}
		if(!PoWordEdit.TryApplyToPo(Draft.Word, out Err)){
			return false;
		}
		return true;
	}

	public partial VmWordPropEdit MkPropEdit(){
		return new VmWordPropEdit(SvcWordV2, UserCtxMgr);
	}

	public partial VmWordLearnEdit MkLearnEdit(){
		return new VmWordLearnEdit(SvcWordV2, UserCtxMgr);
	}
}
