// using Ngan.Dict.Ui.Views.Word.Learn;
// using Ngan.Dict.Ui.Views.Word.WordCard;
// using Tsinswreng.CsTreeTest;

// namespace Ngan.Dict.Ui.Test.Views.Word.Learn;

// public partial class TestIViewLearnWord{
// 	/// <summary>
// 	/// 註冊 `WordListCards` 屬性接口測試。
// 	/// </summary>
// 	/// <param name="Node">當前測試節點。</param>
// 	public void RegisterWordListCards(ITestNode Node){
// 		var register = Node.MkTestFnRegister(
// 			typeof(TestIViewLearnWord)
// 			,[typeof(IViewLearnWord)]
// 			,[nameof(IViewLearnWord.WordListCards)]
// 			,nameof(TestIViewLearnWord)
// 		);
// 		var R = register.Register;

// 		R("WordListCards_Should_BeReadable", async(o)=>{
// 			_ = ViewLearnWord.WordListCards;
// 			Assert.IsTrue(true);
// 			return null;
// 		});
// 	}
// }
