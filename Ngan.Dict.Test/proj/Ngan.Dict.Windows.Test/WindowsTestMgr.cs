using Ngan.Dict.Core.Test;
using Ngan.Dict.Backend.Test;
using Ngan.Dict.Windows.Test.Frontend.User.Svc;
using Tsinswreng.CsTreeTest;

namespace Ngan.Dict.Windows.Test;

public class WindowsTestMgr:DiEtTestMgr{
	public static WindowsTestMgr Inst = new();
	public override ITestNode RegisterTestsInto(ITestNode? Test){
		Test = this.TestNode;
		this.RegisterSubMgr(LocalTestMgr.Inst);
		this.RegisterSubMgr(CoreTestMgr.Inst);
		this.RegisterTester<TestSvcTokenStorage>();
		return Test;
	}

}
