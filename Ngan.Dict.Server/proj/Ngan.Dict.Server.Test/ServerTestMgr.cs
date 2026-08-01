using Ngan.Dict.Core.Test;
using Ngan.Dict.Backend.Test;
using Ngan.Dict.Server.Test.Domains.User.Http;
using Ngan.Dict.Server.Test.Domains.User.Svc;
using Tsinswreng.CsTreeTest;

namespace Ngan.Dict.Server.Test;

public class ServerTestMgr: DiEtTestMgr{
	public static ServerTestMgr Inst = new();
	public override ITestNode RegisterTestsInto(ITestNode? test){
		test = this.TestNode;
		test.Ordered = true;
		test.IsParallelRecursive = false;
		this.RegisterSubMgr(CoreTestMgr.Inst);
		this.RegisterSubMgr(LocalTestMgr.Inst);
		this.RegisterSubMgr(ServerBizTestMgr.Inst);
		return test;
	}
}

public class ServerBizTestMgr: DiEtTestMgr{
	public static ServerBizTestMgr Inst = new();
	public override ITestNode RegisterTestsInto(ITestNode? test){
		test = this.TestNode;
		test.Ordered = true;
		test.IsParallelRecursive = false;
		this.RegisterTester<TestISvcUser>();
		this.RegisterTester<TestCtrlrUser>();
		return test;
	}
}
