namespace Ngaq.Ui.Components.KvMap;
using System.Collections.ObjectModel;
using Ngaq.Core.Tools.JsonMap;
using Ngaq.Ui.Infra;
using Tsinswreng.CsCore;
using Tsinswreng.CsTools;
using Ctx = VmJsonMapItem;
public partial class VmJsonMapItem: ViewModelBase, IMk<Ctx>{
	//蔿從構造函數依賴注入、故以靜態工廠代無參構造器
	protected VmJsonMapItem(){}
	public static Ctx Mk(){
		return new Ctx();
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmJsonMapItem(){
		#if DEBUG
		{
			var o = new Ctx();
			Samples.Add(o);

		}
		#endif
	}

	public IUiJsonMapItem? UiMapItem{
		get;
		set{SetProperty(ref field, value);}
	}


	public Ctx FromBo(IUiJsonMapItem Cur){
		var z = this;
		z.UiMapItem = Cur;
		var Root = Cur.Root;

		if(EUiTextType.RawText.Eq(Cur.DisplayName?.Type)){
			z.DisplayName = Cur.DisplayName?.Data??"";
		}else{
			var key = Cur.DisplayName?.Data??"";
			if(Root.I18n?.TryGetNode(key, out var DisplayDict)==true){
				z.DisplayName = DisplayDict[nameof(UiMapItem.DisplayName)]?.ValueObj as str??"";
			}
		}

		if(EUiTextType.RawText.Eq(Cur.Descr?.Type)){
			z.Descr = Cur.Descr?.Data??"";
		}else{
			var key = Cur.Descr?.Data??"";
			if(Root.I18n?.TryGetNode(key, out var DisplayDict)==true){
				z.Descr = DisplayDict[nameof(UiMapItem.Descr)]?.ValueObj as str??"";
			}
		}


		return this;
	}

	obj? GetData(){
		if(AnyNull(UiMapItem)){
			return null;
		}
		return UiMapItem.FnGet();
	}

	obj? SetData(obj? Data){
		if(AnyNull(UiMapItem)){
			return false;
		}
		return UiMapItem.FnSet(Data);
	}

	public obj? Data{
		get{
			field = GetData();
			return field;
		}
		set{
			SetData(value);
			SetProperty(ref field, value);
		}
	}="";


	public void UpdData(){
		Data = RawInputToData();
	}

	public str DisplayName{
		get;
		set{SetProperty(ref field, value);}
	}="";

	public str Descr{
		get;
		set{SetProperty(ref field, value);}
	}="";

	public str RawInput{
		get{
			field = DataToRawInput();
			return field;
		}
		set{
			//RawInputToData(); 確認保存旹汶執行、勿頻轉
			SetProperty(ref field, value);
		}
	}="";

	public str DataToRawInput(){
		// if(EJsonValueType.String.Eq(UiMapItem?.Type)){
		// 	return Data+"";
		// }
		// if(EJsonValueType.Number.Eq(UiMapItem?.Type)){
		// 	return Data+"";
		// }
		// return
		return Data+"";
	}


	public obj? RawInputToData(){
		if(EJsonValueType.String.Eq(UiMapItem?.JsonValueType)){
			return RawInput;
		}
		if(EJsonValueType.Number.Eq(UiMapItem?.JsonValueType)){
			return Convert.ToDouble(RawInput);//TODO
		}
		return null;//TODO 宜抛异常
	}





}
