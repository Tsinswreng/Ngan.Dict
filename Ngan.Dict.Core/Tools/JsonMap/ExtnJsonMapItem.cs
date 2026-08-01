using Tsinswreng.CsTools;

namespace Ngaq.Core.Tools.JsonMap;
public static class ExtnJsonMapItem{
	extension(IUiJsonMapItem z){
		public bool TryGetNode(IUiJsonMap Root, out IJsonNode R){
			var pathObj = JsonNode.ResolvePath(z.PathStr);
			return Root.Raw.TryGetNodeByPath(pathObj, out R);
		}
		public bool TryGetValue(IUiJsonMap Root, out obj? R){
			R = default!;
			if(z.TryGetNode(Root, out var Node)){
				R = Node.ValueObj;
				return true;
			}
			return false;
		}
		public bool SetNode(IUiJsonMap Root, IJsonNode Node){
			var pathObj = JsonNode.ResolvePath(z.PathStr);
			return Root.Raw.SetNodeByPath(pathObj, Node);
		}
		public bool SetValue(IUiJsonMap Root, obj? Value){
			var pathObj = JsonNode.ResolvePath(z.PathStr);
			var Node = new JsonNode(Value);
			return Root.Raw.SetNodeByPath(pathObj, Node);
		}

	}
}
