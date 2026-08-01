namespace Ngaq.Ui.Views.Word.WordEditV2.WordPropEdit;

using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Shared.Word.Svc;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Views.Word.WordEditV2.WordPropPage;

/// 單行屬性編輯頁 ViewModel。自行注入後端服務，原子落庫。
public partial class VmWordPropEdit: ViewModelBase, IMk<VmWordPropEdit>{
	protected VmWordPropEdit(){}
	public static VmWordPropEdit Mk(){return new VmWordPropEdit();}

	ISvcWordV2? SvcWordV2;
	IFrontendUserCtxMgr? UserCtxMgr;

	public partial VmWordPropEdit(
		ISvcWordV2? SvcWordV2,
		IFrontendUserCtxMgr? UserCtxMgr
	);

	/// 刪除成功後觸發，供宿主同步列表狀態。
	public event Action<VmWordPropRow>? Deleted;
	/// 保存成功後觸發。
	public event Action<VmWordPropRow>? Saved;

	/// 正在編輯的屬性行。
	public VmWordPropRow Row{
		get;
		set{SetProperty(ref field, value);}
	} = VmWordPropRow.NewRow();

	/// 直接調後端刪除。
	public partial Task<bool> Delete(CT Ct);
	/// 直接調後端保存（Add→OrdAddWordProp / Modified→OrdUpdWordProp）。
	public partial Task<bool> Save(CT Ct);
	/// 由 View 層直接調用的後端刪除，不包裝錯誤處理。
	public partial Task DelDirect();
	/// 由 View 層直接調用的後端保存，不包裝錯誤處理。
	public partial Task SaveDirect();
	/// View 層通知父級同步刪除後的列表狀態。
	public partial void OnDeletedByView();
}
