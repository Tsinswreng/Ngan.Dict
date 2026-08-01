namespace Ngan.Dict.Ui.Views.Word.WordManage.SearchWords;

using System.Collections.ObjectModel;
using Ngan.Dict.Core.Frontend.User;
using Ngan.Dict.Core.Shared.Word.Models;
using Ngan.Dict.Core.Shared.Word.Models.Dto;
using Ngan.Dict.Core.Shared.Word.Svc;
using Ngan.Dict.Ui.Components.PageBar;
using Ngan.Dict.Ui.Infra;
using Ngan.Dict.Ui.Views.Word.WordCard;
using Ctx = VmSearchWords;

/// 用戶詞庫搜索頁的狀態與分頁協調器。
public partial class VmSearchWords: ViewModelBase, IWordCardMenuAction{
	/// 提供分頁搜索的單詞服務；未注入時搜索安全地不執行。
	ISvcWordV2 SvcWordV2;
	/// 提供當前登入使用者的資料庫上下文。
	IFrontendUserCtxMgr IUserCtxMgr;
	/// 為詞卡右鍵選單提供發音業務能力。
	IWordCardPronounceBiz WordCardPronounceBiz;
	/// 分頁狀態與翻頁回調。
	public VmPageBar PageBar{get;set;} = null!;
	/// 使用者輸入的搜索關鍵字。
	public str Input{
		get;
		set{SetProperty(ref field, value);}
	} = "";
	/// 當前頁搜索命中；保留命中資產資訊。
	public IList<DtoWordSearchHit> GotWords{
		get;
		set{SetProperty(ref field, value);}
	} = [];
	/// 調試環境中的頁面樣例集合。
	public static ObservableCollection<Ctx> Samples = [];

	/// 建立未注入服務的 ViewModel，供靜態工廠與設計期樣例使用。
	protected partial VmSearchWords();
	/// 建立可獨立使用的搜索 ViewModel。
	public static partial Ctx Mk();
	/// 由 DI 注入搜索、使用者上下文及發音服務。
	public partial VmSearchWords(IFrontendUserCtxMgr? IUserCtxMgr, ISvcWordV2? SvcWordV2, IWordCardPronounceBiz? WordCardPronounceBiz);
	/// 初始化分頁大小與前後翻頁回調。
	partial void Init();
	/// 重置至首頁並執行搜索。
	public partial Task<nil> InitSearch(CT Ct);
	/// 依目前關鍵字與分頁狀態取得一頁搜索命中。
	private partial Task<nil> Search(CT Ct = default);
	/// 在尚有前頁時調整頁碼並重新搜索。
	private partial Task<nil> OnPrevPage(VmPageBar PageBar, CT Ct);
	/// 在尚有後頁時調整頁碼並重新搜索。
	private partial Task<nil> OnNextPage(VmPageBar PageBar, CT Ct);
	/// 卡片菜單朗讀：先以 UserLang 找 NormLang，再調用 TTS 播放。
	public partial Task<DtoWordCardPronounceResult> PronounceWord(IJnWord? JnWord, CT Ct);
}
