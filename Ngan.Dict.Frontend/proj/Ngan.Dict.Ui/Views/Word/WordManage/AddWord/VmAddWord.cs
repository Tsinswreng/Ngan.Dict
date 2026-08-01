namespace Ngan.Dict.Ui.Views.Word.WordManage.AddWord;

using System.Collections.ObjectModel;
using Ngan.Dict.Core.Frontend.User;
using Ngan.Dict.Core.Shared.Word.Svc;
using Ngan.Dict.Ui.Infra;
using Tsinswreng.CsI18n;
using Tsinswreng.CsTools;

using Ctx = VmAddWord;
using K = Ngan.Dict.Ui.Infra.I18n.KeysUiI18nCommon;

public partial class VmAddWord
	:ViewModelBase
{
	public VmAddWord(){

	}

	public VmAddWord(
		ISvcWord? SvcWord = null
		,IFrontendUserCtxMgr? UserCtxMgr = null
	){
		this.SvcWord = SvcWord;
		this.UserCtxMgr = UserCtxMgr;
	}

	ISvcWord? SvcWord{get;set;}
	IFrontendUserCtxMgr? UserCtxMgr{get;set;}

	public static ObservableCollection<Ctx> Samples = [];
	static VmAddWord(){
		#if DEBUG
		{
			var o = new Ctx();
			o.Text = "hello\\ndictionary\\nexample";
			Samples.Add(o);
		}
		#endif
	}

	public str Text{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public str ErrStr{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public async Task<nil> Confirm(CT Ct){
		if(AnyNull(SvcWord, UserCtxMgr)){
			return NIL;
		}
		if(str.IsNullOrWhiteSpace(Text)){
			ShowDialog(I18n[K.TextIsEmpty]);
			return NIL;
		}
		try{
			await SvcWord.AddWordsFromText(
				UserCtxMgr.GetUserCtx(),
				Text,
				Ct
			);
			ShowToast(I18n[K.Submitted]);
		}catch(Exception ex){
			ErrStr = ex.Message;
			HandleErr(ex);
		}
		return NIL;
	}
}
