namespace Ngaq.Ui.Views.Word.WordEditV2.WordLearnEdit;

using Ngaq.Core.Frontend.User;
using Ngaq.Core.Shared.Word.Svc;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Views.Word.WordEditV2.WordLearnPage;

/// 單行學習記錄編輯頁 ViewModel。自行注入後端服務，原子落庫。
public partial class VmWordLearnEdit: ViewModelBase, IMk<VmWordLearnEdit>{
	protected VmWordLearnEdit(){}
	public static VmWordLearnEdit Mk(){return new VmWordLearnEdit();}

	ISvcWordV2? SvcWordV2;
	IFrontendUserCtxMgr? UserCtxMgr;

	public partial VmWordLearnEdit(
		ISvcWordV2? SvcWordV2,
		IFrontendUserCtxMgr? UserCtxMgr
	);

	/// 刪除成功後觸發，供宿主同步列表狀態。
	public event Action<VmWordLearnRow>? Deleted;
	/// 保存成功後觸發。
	public event Action<VmWordLearnRow>? Saved;

	/// 正在編輯的學習記錄行。
	public VmWordLearnRow Row{
		get;
		set{SetProperty(ref field, value);}
	} = VmWordLearnRow.NewRow();

	/// 直接調後端刪除。
	public partial Task<bool> Delete(CT Ct);
	/// 直接調後端保存（Add→OrdAddWordLearn / Modified→OrdUpdWordLearn）。
	public partial Task<bool> Save(CT Ct);
	/// 由 View 層直接調用的後端刪除。
	public partial Task DelDirect();
	/// 由 View 層直接調用的後端保存。
	public partial Task SaveDirect();
	/// View 層通知父級同步刪除後的列表狀態。
	public partial void OnDeletedByView();
}
