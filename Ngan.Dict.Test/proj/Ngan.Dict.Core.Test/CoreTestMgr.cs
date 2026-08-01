using Ngan.Dict.Core.Test.Tools.Json.IDictJsonSerializer;
using Ngan.Dict.Core.Test.Tools.Json.AppJsonSerializerTests;
using Ngan.Dict.Core.Test.Sync.IPacker;
using Tsinswreng.CsTreeTest;
using Ngan.Dict.Core.Test.Lib.MsgFmt;
using Ngan.Dict.Core.Test.Lang.ExprRefl;

namespace Ngan.Dict.Core.Test;

public class CoreTestMgr:DiEtTestMgr{
	public static CoreTestMgr Inst = new();
	public override ITestNode RegisterTestsInto(ITestNode? Node){
		Node = this.TestNode;
		this.RegisterTester<TestAppJsonSerializer>();
		this.RegisterTester<TestIDictJsonSerializer>();
		this.RegisterTester<TestIPacker>();
		this.RegisterTester<TestMsgFmt>();
		this.RegisterTester<TestExprRefl>();
		return Node;
	}
}
