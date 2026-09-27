using Tsinswreng.CsTreeTest;
using Tsinswreng.CsCore;

namespace Ngan.Dict.Core.Test.DictSerializer;

public partial class TestIDictSerializer: ITester{
	
	[Doc(@$"
	#Params([src obj],[target type], [target obj])
	")]
	public Func<obj?, Type, obj?> Deserialize = null!;
	Func<obj?, Type, obj?> de=>Deserialize;
	public ITestNode RegisterTestsInto(ITestNode? Node){
		Node??=new TestNode();
		RegisterDeserialize(Node);

		return Node;
	}
}
