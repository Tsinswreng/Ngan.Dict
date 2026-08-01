namespace Ngaq.Ui.Components.KvMap.JsonMap;
using System.Collections.ObjectModel;
using Ngaq.Core.Tools.JsonMap;
using Ngaq.Ui.Infra;

using Ctx = VmUiJsonMap;
//using Bo = Ngaq.Core.Tools.JsonMap.UiJsonMap;
public partial class VmUiJsonMap: ViewModelBase, IMk<Ctx>{
	//蔿從構造函數依賴注入、故以靜態工廠代無參構造器
	protected VmUiJsonMap(){}
	public static Ctx Mk(){
		return new Ctx();
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmUiJsonMap(){
		#if DEBUG
		{
			var o = new Ctx();
			Samples.Add(o);
		}
		#endif
	}

	public IUiJsonMap? UiJsonMap{get;set;}

	public void FromBo(IUiJsonMap Bo){
		this.UiJsonMap = Bo;
		if(Bo.PathToUiMap is null){
			ItemVms = new();
			return;
		}
		ItemVms = new(Bo.PathToUiMap.Select((item)=>{
			var vm = VmJsonMapItem.Mk();
			item.Value.PathStr = item.Key;
			vm.FromBo(item.Value);
			return vm;
		}));
	}


	public ObservableCollection<VmJsonMapItem> ItemVms{
		get;
		set{SetProperty(ref field, value);}
	}=[];

	/// 調用UpdData纔實際寫入內ʹ值
	public void UpdData(){
		foreach(var vm in ItemVms){
			vm.UpdData();
		}
	}

}
