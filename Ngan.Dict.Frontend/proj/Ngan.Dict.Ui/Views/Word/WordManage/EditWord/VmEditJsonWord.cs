namespace Ngan.Dict.Ui.Views.Word.WordManage.EditWord;

using System.Collections.ObjectModel;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ngan.Dict.Core.Frontend.User;
using Ngan.Dict.Core.Infra;
using Ngan.Dict.Core.Shared.User.UserCtx;
using Ngan.Dict.Core.Shared.Word.Models;
using Ngan.Dict.Core.Shared.Word.Models.Po.Kv;
using Ngan.Dict.Core.Shared.Word.Models.Po.Learn;
using Ngan.Dict.Core.Shared.Word.Models.Po.Word;
using Ngan.Dict.Core.Shared.Word.Svc;
using Ngan.Dict.Core.Tools;
using Ngan.Dict.Core.Tools.Json;
using Ngan.Dict.Ui.Infra;
using Tsinswreng.CsTools;
using JsonNode = System.Text.Json.Nodes.JsonNode;

using Ctx = VmEditJsonWord;

public partial class VmEditJsonWord: ViewModelBase, IMk<Ctx>{
	// 蔿從構造函數依賴注入，故以靜態工廠代無參構造器。
	protected VmEditJsonWord(){}
	public static Ctx Mk(){
		return new Ctx();
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmEditJsonWord(){
		#if DEBUG
		{
			var o = new Ctx();
			Samples.Add(o);
		}
		#endif
	}

	public VmEditJsonWord(
		ISvcWordV2? SvcWordV2
		,IJsonSerializer? JsonSerializer
		,IFrontendUserCtxMgr? UserCtxMgr
	){
		this.SvcWordV2 = SvcWordV2;
		this.JsonSerializer = JsonSerializer;
		this.UserCtxMgr = UserCtxMgr;
	}

	ISvcWordV2? SvcWordV2;
	IJsonSerializer? JsonSerializer;
	IFrontendUserCtxMgr? UserCtxMgr;
	CancellationTokenSource Cts = new();

	static str FormatJson(string uglyJson){
		JsonNode? node = JsonNode.Parse(uglyJson);
		string pretty = node!.ToJsonString(new JsonSerializerOptions {
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		});
		return pretty;
	}

	public nil FromJnWord(IJnWord JnWord){
		Src = JnWord.DeepClone().AsOrToJnWord();
		Bo = JnWord.DeepClone().AsOrToJnWord();
		IJnWord simple = Bo;
		Json = JsonSerializer?.Stringify(simple) ?? "";
		Json = FormatJson(Json);
		return NIL;
	}

	public IJnWord? Src{
		get;
		set{SetProperty(ref field, value);}
	}

	public IJnWord? Bo{
		get;
		set{SetProperty(ref field, value);}
	}

	protected str _Json = "";
	public str Json{
		get{return _Json;}
		set{SetProperty(ref _Json, value);}
	}

	public nil Deserialize(){
		if(JsonSerializer is null){
			return NIL;
		}
		try{
			Bo = JsonSerializer.Parse<JnWord>(Json);
		}catch(Exception e){
			Console.WriteLine(e);
		}
		return NIL;
	}

	public nil Delete(){
		if(SvcWordV2 is null || UserCtxMgr is null){
			return NIL;
		}
		Deserialize();
		if(Bo is null){
			return NIL;
		}
		var ct = Cts.Token;
		SvcWordV2.SoftDelJnWordInId(
			UserCtxMgr.GetUserCtx().ToDbUserCtx(),
			ToolAsyE.ToAsyE([Bo.Id]),
			ct
		).ContinueWith(t=>{
			HandleErr(t);
		});
		return NIL;
	}

	/// 注意: 改 Props 或 Learns 內容時，需刪原時間，否則 diff 不到。
	public nil Save(){
		if(SvcWordV2 is null || UserCtxMgr is null){
			return NIL;
		}
		var ct = Cts.Token;
		SaveByDetail(ct).ContinueWith(t=>{
			HandleErr(t);
		});
		return NIL;
	}

	async Task<nil> SaveByDetail(CT Ct){
		if(SvcWordV2 is null || UserCtxMgr is null){
			return NIL;
		}
		Deserialize();
		if(Bo is null){
			return NIL;
		}
		var draft = Bo.AsOrToJnWord();
		draft.EnsureForeignId();
		var dbCtx = UserCtxMgr.GetUserCtx().ToDbUserCtx();

		var oldId = draft.Word.Id;
		var finalId = await UpdRootAndGetFinalId(dbCtx, draft, Ct);
		var hasMovedToOtherWord = oldId != finalId;
		if(hasMovedToOtherWord){
			draft.SetIdEtEnsureFKey(finalId);
		}

		await SavePropsByDiff(dbCtx, Src?.AsOrToJnWord(), draft, hasMovedToOtherWord, Ct);
		await SaveLearnsByDiff(dbCtx, Src?.AsOrToJnWord(), draft, hasMovedToOtherWord, Ct);

		Src = draft.DeepClone().AsOrToJnWord();
		Bo = draft.DeepClone().AsOrToJnWord();
		return NIL;
	}

	async Task<IdWord> UpdRootAndGetFinalId(IDbUserCtx dbCtx, JnWord draft, CT Ct){
		if(SvcWordV2 is null){
			return default;
		}
		var respAsyE = await SvcWordV2.OrdUpdPoWord(dbCtx, ToolAsyE.ToAsyE([draft.Word]), Ct);
		var resp = await respAsyE.FirstOrDefaultAsync(Ct);
		if(resp is null){
			throw new InvalidOperationException("BatUpdPoWord returned empty response");
		}
		return resp.FinalId;
	}

	async Task<nil> SavePropsByDiff(
		IDbUserCtx dbCtx,
		IJnWord? srcWord,
		JnWord draftWord,
		bool hasMovedToOtherWord,
		CT Ct
	){
		if(SvcWordV2 is null){
			return NIL;
		}
		var addProps = draftWord.Props
			.Where(x=>x.Id.IsNullOrDefault())
			.Select(x=>{
				var neo = (PoWordProp)x.ShallowCloneSelf();
				neo.WordId = draftWord.Word.Id;
				return neo;
			})
		;
		await SvcWordV2.OrdAddWordProp(dbCtx, ToolAsyE.ToAsyE(addProps), Ct);

		var updProps = draftWord.Props
			.Where(x=>!x.Id.IsNullOrDefault())
			.Select(x=>{
				var upd = (PoWordProp)x.ShallowCloneSelf();
				upd.WordId = draftWord.Word.Id;
				return upd;
			})
		;
		await SvcWordV2.OrdUpdWordProp(dbCtx, ToolAsyE.ToAsyE(updProps), Ct);

		if(srcWord is null || hasMovedToOtherWord){
			return NIL;
		}
		var keepPropIds = draftWord.Props
			.Where(x=>!x.Id.IsNullOrDefault())
			.Select(x=>x.Id)
			.ToHashSet()
		;
		var delPropIds = srcWord.Props
			.Where(x=>!x.Id.IsNullOrDefault() && !keepPropIds.Contains(x.Id))
			.Select(x=>x.Id)
		;
		await SvcWordV2.DelWordPropInId(dbCtx, ToolAsyE.ToAsyE(delPropIds), Ct);
		return NIL;
	}

	async Task<nil> SaveLearnsByDiff(
		IDbUserCtx dbCtx,
		IJnWord? srcWord,
		JnWord draftWord,
		bool hasMovedToOtherWord,
		CT Ct
	){
		if(SvcWordV2 is null){
			return NIL;
		}
		var addLearns = draftWord.Learns
			.Where(x=>x.Id.IsNullOrDefault())
			.Select(x=>{
				var neo = (PoWordLearn)x.ShallowCloneSelf();
				neo.WordId = draftWord.Word.Id;
				return neo;
			})
		;
		await SvcWordV2.OrdAddWordLearn(dbCtx, ToolAsyE.ToAsyE(addLearns), Ct);

		var updLearns = draftWord.Learns
			.Where(x=>!x.Id.IsNullOrDefault())
			.Select(x=>{
				var upd = (PoWordLearn)x.ShallowCloneSelf();
				upd.WordId = draftWord.Word.Id;
				return upd;
			})
		;
		await SvcWordV2.OrdUpdWordLearn(dbCtx, ToolAsyE.ToAsyE(updLearns), Ct);

		if(srcWord is null || hasMovedToOtherWord){
			return NIL;
		}
		var keepLearnIds = draftWord.Learns
			.Where(x=>!x.Id.IsNullOrDefault())
			.Select(x=>x.Id)
			.ToHashSet()
		;
		var delLearnIds = srcWord.Learns
			.Where(x=>!x.Id.IsNullOrDefault() && !keepLearnIds.Contains(x.Id))
			.Select(x=>x.Id)
		;
		await SvcWordV2.DelWordLearnInId(dbCtx, ToolAsyE.ToAsyE(delLearnIds), Ct);
		return NIL;
	}
}



