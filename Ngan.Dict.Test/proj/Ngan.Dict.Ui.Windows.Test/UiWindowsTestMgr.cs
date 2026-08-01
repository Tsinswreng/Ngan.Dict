using Ngan.Dict.Core.Test;
using Ngan.Dict.Backend.Test;
using Ngan.Dict.Ui.Test;
using Tsinswreng.CsTreeTest;

namespace Ngan.Dict.Ui.Windows.Test;

public class UiWindowsTestMgr:DiEtTestMgr{
	public static UiWindowsTestMgr Inst = new();

	public override ITestNode RegisterTestsInto(ITestNode? Test){
		Test = this.TestNode;
		this.RegisterSubMgr(LocalTestMgr.Inst);
		this.RegisterSubMgr(CoreTestMgr.Inst);
		this.RegisterSubMgr(UiTestMgr.Inst);
		return Test;
	}
}
