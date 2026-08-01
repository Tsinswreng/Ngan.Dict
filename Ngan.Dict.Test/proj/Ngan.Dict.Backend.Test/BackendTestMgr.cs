using Ngan.Dict.Backend.Test.Domains.Word;
using Ngan.Dict.Backend.Test.Domains.StudyPlan;
using Ngan.Dict.Backend.Test.Domains.Kv;
using Ngan.Dict.Backend.Test.CsSql.Repo;
using Ngan.Dict.Backend.Test.CsSql.TblSetter;
using Tsinswreng.CsTreeTest;

namespace Ngan.Dict.Backend.Test;


public class LocalTestMgr:DiEtTestMgr{
	public static LocalTestMgr Inst = new();
	public override ITestNode RegisterTestsInto(ITestNode? Test){
		Test = this.TestNode;
		Test.Ordered = true;
		Test.IsParallelRecursive = true;  // Recursively disable parallel execution for all db-accessing tests
		this.RegisterTester<TestISvcStudyPlan>();
		this.RegisterTester<TestISvcWord>();
		this.RegisterTester<TestISvcWordV2>();
		this.RegisterTester<TestISvcUserLang>();
		this.RegisterTester<TestISvcNormLang>();
		this.RegisterTester<TestISvcNormLangToUserLang>();
		this.RegisterTester<TestISvcKv>();
		this.RegisterTester<TestDaoWord>();
		this.RegisterTester<TestRepo>();
		this.RegisterTester<TestTblSetter>();
		return Test;
	}
}
