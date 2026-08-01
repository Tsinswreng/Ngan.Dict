// using Ngan.Dict.Core.Frontend.User;
// using Ngan.Dict.Core.Infra;
// using Ngan.Dict.Core.Shared.Word.Models;
// using Ngan.Dict.Core.Shared.Word.Models.Po.Word;
// using Ngan.Dict.Ui.Views.Word.WordEditV2;
// using Tsinswreng.CsSql;
// using Tsinswreng.CsTreeTest;

// namespace Ngan.Dict.Ui.Test.Views.Word.WordEditV2;

// /// <summary>
// /// `IViewWordEditV2` 的接口測試主裝配類。
// /// 專注於頁面契約：子頁暴露、保存/刪除按鈕行為與完成事件。
// /// </summary>
// public partial class TestIViewWordEditV2: ITester{
// 	readonly IViewWordEditV2 ViewWordEditV2;
// 	readonly IFrontendUserCtxMgr UserCtxMgr;
// 	readonly IRepo<PoWord, IdWord> RepoWord;

// 	public TestIViewWordEditV2(
// 		IViewWordEditV2 ViewWordEditV2
// 		,IFrontendUserCtxMgr UserCtxMgr
// 		,IRepo<PoWord, IdWord> RepoWord
// 	){
// 		this.ViewWordEditV2 = ViewWordEditV2;
// 		this.UserCtxMgr = UserCtxMgr;
// 		this.RepoWord = RepoWord;
// 	}

// 	public ITestNode RegisterTestsInto(ITestNode? Node){
// 		Node ??= new TestNode();
// 		Node.Ordered = true;
// 		Node.IsParallelRecursive = false;
// 		RegisterPoWordEdit(Node);
// 		RegisterClickSave(Node);
// 		RegisterClickDelete(Node);
// 		return Node;
// 	}

// 	protected async Task<TRtn> RunNoTxn<TRtn>(Func<IDbFnCtx, Task<TRtn>> Fn){
// 		IDbFnCtx Ctx = new DbFnCtx();
// 		try{
// 			return await Fn(Ctx);
// 		}
// 		finally{
// 			await Ctx.DisposeAsync();
// 		}
// 	}

// 	protected static async IAsyncEnumerable<T> AsyE<T>(params T[] Items){
// 		foreach(var item in Items){
// 			yield return item;
// 		}
// 	}
// }
