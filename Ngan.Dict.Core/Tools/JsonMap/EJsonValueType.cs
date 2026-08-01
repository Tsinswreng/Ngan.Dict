namespace Ngaq.Core.Tools.JsonMap;
[Doc(@$"

異ₐUI ʸʹ編輯器控件ˇ針對、需細分JsonValue之類型 如下。

複雜ₐ嵌套對象ˇ不支持。

需改複雜ₐ嵌套對象旹 宜新建{nameof(UiJsonMapItem)}ⁿ用{nameof(UiJsonMapItem.PathStr)}
精準定位至其簡單ₐ字段
")]
public enum EJsonValueType{
	Unknown,
	String,
	Number,
	Boolean,
	Array,
	Object,
	StringArray,
	NumberArray,
	StringStringMap,
	NumberNumberMap,
	StringNumberMap,
	StringBooleanMap,
	NumberStringMap,
}
