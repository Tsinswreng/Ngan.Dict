namespace Ngaq.Core.Tools.JsonMap;
public class UiText{
	public UiText(){

	}
	public UiText(str RawText){
		Type = EUiTextType.RawText;
		Data = RawText;
	}

	public static UiText FromRawText(str RawText){
		var z = new UiText(){
			Data = RawText,
			Type = EUiTextType.RawText,
		};
		return z;
	}
	public EUiTextType Type{get;set;}
	public str? Data{get;set;}
}
