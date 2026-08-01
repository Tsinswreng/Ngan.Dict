using Ngan.Dict.Ui.Test.Views.Word.WordEditV2;
using Tsinswreng.CsTreeTest;

namespace Ngan.Dict.Ui.Test;

public class UiTestMgr : DiEtTestMgr {
	public static UiTestMgr Inst = new();
	public override ITestNode RegisterTestsInto(ITestNode? Test) {
		Test = this.TestNode;
		Test.Ordered = true;
		Test.IsParallelRecursive = false;
		this.RegisterTester<TestViewWordEditV2>();
		return Test;
	}
}
