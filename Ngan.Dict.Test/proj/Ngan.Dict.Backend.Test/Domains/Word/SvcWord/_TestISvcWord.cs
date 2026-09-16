using Ngan.Dict.Core.Shared.Word.Svc;
using Tsinswreng.CsTreeTest;

namespace Ngan.Dict.Backend.Test.Domains.Word;

public partial class TestISvcWord:ITester{
	readonly ISvcWord SvcWord = GetRSvc<ISvcWord>();
	
	public ITestNode RegisterTestsInto(ITestNode? Test){
		Test??=new TestNode();
		var register = Test.MkTestFnRegister(
			typeof(TestISvcWord), [typeof(ISvcWord)],[],nameof(TestISvcWord)
		);
		
		return Test;
	}
}
