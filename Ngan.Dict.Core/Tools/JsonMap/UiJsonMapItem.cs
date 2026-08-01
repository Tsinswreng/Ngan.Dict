using Ngaq.Core.Tools.Proxy;
using Tsinswreng.CsTools;

namespace Ngaq.Core.Tools.JsonMap;
public class IUiJsonMapItem : IFnProxy{
	protected IUiJsonMapItem(){
		Init();
	}
	public static IUiJsonMapItem MkNoInit(){
		return new IUiJsonMapItem();
	}

	public Func<obj?> FnGet{get;set;} = null!;
	public Func<obj?, obj?> FnSet{get;set;} = null!;
	public IUiJsonMap Root{get;set;} = new();
	/// 緩存ᐪ
	public obj? ValueObj{get;set;} = null;
	public Type? Type{get;set;} = null;
	public IJsonNode? Node{get;set;} = null;
	/// 如foo.bar[0].baz
	public str PathStr{get;set;} = "";
	public IList<obj> ResolvedPath{get;set;} = new List<obj>();
	public EJsonValueType JsonValueType{get;set;} = EJsonValueType.Unknown;
	public EValueMapper ValueMapper{get;set;} = EValueMapper.None;
	public UiText? DisplayName{get;set;} = null;
	public UiText? Descr{get;set;} = null;
	public bool IsReadOnly{get;set;} = false;
	public bool IsVisible{get;set;} = true;


	public void Init(){
		var z = this;
		z.ResolvedPath = JsonNode.ResolvePath(PathStr);
		z.Node = _GetNode();
		z.ValueObj = _GetValue();
		z.FnGet = _GetValue;
		z.FnSet = _SetValue;
		z.Type = z.ValueObj?.GetType();
	}

	IJsonNode? _GetNode(){
		if(Root.Raw.TryGetNode(ResolvedPath, out var node)){
			return node;
		}
		return null;
	}

	obj? _GetValue(){
		return _GetNode()?.ValueObj;
	}


	nil _SetNode(IJsonNode node){
		Root.Raw.SetNodeByPath(ResolvedPath, node);
		return NIL;
	}

	nil _SetValue(obj? value){
		var node = new JsonNode(value);
		_SetNode(node);
		return NIL;
	}


}

public class UiJsonMapItem:IUiJsonMapItem{
	//public UiJsonMapItem():base(){}
	protected UiJsonMapItem(){}
	public UiJsonMapItem(
		IUiJsonMap Root
		,str PathStr
	){
		this.Root = Root;
		this.PathStr = PathStr;
		Init();
	}
}
