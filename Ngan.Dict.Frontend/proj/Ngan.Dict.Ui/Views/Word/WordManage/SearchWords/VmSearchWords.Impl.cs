namespace Ngan.Dict.Ui.Views.Word.WordManage.SearchWords;

using Avalonia.Threading;
using Ngan.Dict.Core.Frontend.User;
using Ngan.Dict.Core.Infra;
using Ngan.Dict.Core.Shared.Word.Models;
using Ngan.Dict.Core.Shared.Word.Models.Dto;
using Ngan.Dict.Core.Shared.Word.Svc;
using Ngan.Dict.Ui.Components.PageBar;
using Ngan.Dict.Ui.Infra;
using Ngan.Dict.Ui.Views.Word.WordCard;
using Tsinswreng.CsPage;
using Tsinswreng.CsTools;
using Ctx = VmSearchWords;

public partial class VmSearchWords{
	static VmSearchWords(){
		#if DEBUG
		Samples.Add(new Ctx());
		#endif
	}
	protected partial VmSearchWords(){
		Init();
	}
	public static partial Ctx Mk(){
		return new Ctx();
	}
	public partial VmSearchWords(IFrontendUserCtxMgr? IUserCtxMgr, ISvcWordV2? SvcWordV2, IWordCardPronounceBiz? WordCardPronounceBiz):this(){
		this.SvcWordV2 = SvcWordV2;
		this.IUserCtxMgr = IUserCtxMgr;
		this.WordCardPronounceBiz = WordCardPronounceBiz;
		base.Init();
	}
	partial void Init(){
		PageBar = VmPageBar.Mk();
		PageBar.PageSize = 10;
		PageBar.FnPrevPage = OnPrevPage;
		PageBar.FnNextPage = OnNextPage;
	}
	public async partial Task<nil> InitSearch(CT Ct){
		PageBar.PageNum = 1;
		return await Search(Ct);
	}
	private async partial Task<nil> Search(CT Ct){
		await Task.Run(async()=>{
			CheckInit();
			var PageQry = PageBar.ToPageQry();
			PageQry.WantTotCnt = true;
			var Req = new ReqSearchWord{RawStr = Input};
			var Result = await SvcWordV2.PageSearch(IUserCtxMgr.GetDbUserCtx(), PageQry, Req, Ct);
			Dispatcher.UIThread.Post(()=>{
				GotWords = Result.Data ?? [];
				PageBar.FromPageResultInfo(Result);
			});
		}, Ct);
		return NIL;
	}
	private async partial Task<nil> OnPrevPage(VmPageBar PageBar, CT Ct){
		if(PageBar.PageNum <= 1){PageBar.PageNum = 1; return NIL;}
		PageBar.PageNum--;
		return await Search(Ct);
	}
	private async partial Task<nil> OnNextPage(VmPageBar PageBar, CT Ct){
		if(PageBar.TotPageCnt is u64 TotalPage && TotalPage > 0 && PageBar.PageNum >= TotalPage){return NIL;}
		PageBar.PageNum++;
		return await Search(Ct);
	}
	public async partial Task<DtoWordCardPronounceResult> PronounceWord(IJnWord? JnWord, CT Ct){
		if(AnyNull(WordCardPronounceBiz, IUserCtxMgr)){
			return new DtoWordCardPronounceResult{Status = EWordCardPronounceStatus.ServiceUnavailable};
		}
		return await WordCardPronounceBiz.PronounceWord(IUserCtxMgr.GetDbUserCtx(), JnWord, Ct);
	}
}
