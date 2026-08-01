namespace Ngaq.Ui.Views.Word.WordEditV2.WordPropEdit;

using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Shared.Word.Svc;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Views.Word.WordEditV2.WordPropPage;
using Tsinswreng.CsTools;
using K = Ngaq.Ui.Infra.I18n.KeysUiI18nCommon;

public partial class VmWordPropEdit{
	public partial VmWordPropEdit(
		ISvcWordV2? SvcWordV2,
		IFrontendUserCtxMgr? UserCtxMgr
	){
		this.SvcWordV2 = SvcWordV2;
		this.UserCtxMgr = UserCtxMgr;
	}

	/// 直接調後端軟刪除，成功後觸發 Deleted 事件通知宿主同步列表。
	public async partial Task<bool> Delete(CT Ct){
		if(Row.DmlState == EDmlState.Added){
			Deleted?.Invoke(Row);
			return true;
		}
		if(AnyNull(SvcWordV2, UserCtxMgr)){
			return false;
		}
		try{
			await SvcWordV2.DelWordPropInId(
				UserCtxMgr.GetDbUserCtx(),
				ToolAsyE.ToAsyE([Row.Raw.Id]),
				Ct
			);
			Deleted?.Invoke(Row);
			ShowToast(I18n[K.Deleted]);
			return true;
		}catch(Exception ex){
			HandleErr(ex);
			return false;
		}
	}

	/// 按 DmlState 原子落庫：Added → OrdAddWordProp / Modified → OrdUpdWordProp。
	public async partial Task<bool> Save(CT Ct){
		if(Row.DmlState != EDmlState.Added && Row.DmlState != EDmlState.Modified){
			return true;
		}
		if(AnyNull(SvcWordV2, UserCtxMgr)){
			return false;
		}
		try{
			if(!Row.TryToPo(Row.Raw.WordId, out var po, out var err)){
				ShowDialog(err);
				return false;
			}
			var dbCtx = UserCtxMgr.GetDbUserCtx();
			if(Row.DmlState == EDmlState.Added){
				await SvcWordV2.OrdAddWordProp(dbCtx, ToolAsyE.ToAsyE([po]), Ct);
			}else{
				await SvcWordV2.OrdUpdWordProp(dbCtx, ToolAsyE.ToAsyE([po]), Ct);
			}
			Row.DmlState = EDmlState.Unchanged;
			Saved?.Invoke(Row);
			ShowToast(I18n[K.Saved]);
			return true;
		}catch(Exception ex){
			HandleErr(ex);
			return false;
		}
	}

	/// View 層直調後端刪除，不捕獲異常。
	public async partial Task DelDirect(){
		await SvcWordV2!.DelWordPropInId(
			UserCtxMgr!.GetDbUserCtx(),
			ToolAsyE.ToAsyE([Row.Raw.Id]),
			default
		);
	}

	/// View 層直調後端保存，不捕獲異常。
	public async partial Task SaveDirect(){
		Row.TryToPo(Row.Raw.WordId, out var po, out _);
		var dbCtx = UserCtxMgr!.GetDbUserCtx();
		if(Row.DmlState == EDmlState.Added){
			await SvcWordV2!.OrdAddWordProp(dbCtx, ToolAsyE.ToAsyE([po]), default);
		}else{
			await SvcWordV2!.OrdUpdWordProp(dbCtx, ToolAsyE.ToAsyE([po]), default);
		}
		Row.DmlState = EDmlState.Unchanged;
		Saved?.Invoke(Row);
	}

	/// View 層通知宿主同步列表。
	public partial void OnDeletedByView(){
		Deleted?.Invoke(Row);
	}
}
