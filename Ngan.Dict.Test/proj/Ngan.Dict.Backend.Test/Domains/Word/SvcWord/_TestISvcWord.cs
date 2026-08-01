using Ngan.Dict.Core.Shared.Word.Svc;
using Tsinswreng.CsTreeTest;

namespace Ngan.Dict.Backend.Test.Domains.Word;

public partial class TestISvcWord:ITester{
	ISvcWord SvcWord;
	public TestISvcWord(
		ISvcWord SvcWord
	){
		this.SvcWord = SvcWord;
	}
	
	public ITestNode RegisterTestsInto(ITestNode? Test){
		Test??=new TestNode();
		var register = Test.MkTestFnRegister(
			typeof(TestISvcWord), [typeof(ISvcWord)],[],nameof(TestISvcWord)
		);
		
		return Test;
	}
}
