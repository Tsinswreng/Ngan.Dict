namespace Ngaq.Ui.Views.Word.WordManage.StudyPlan.WeightArgPayloadJsonEdit;

using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ngaq.Ui.Infra;

using Ctx = VmWeightArgPayloadJsonEdit;

/// WeightArg Payload(JSON) 子頁 ViewModel。
/// 只編輯 payload 文本，提交給主編輯頁，不直接寫庫。
public class VmWeightArgPayloadJsonEdit: ViewModelBase, IMk<Ctx>{
	protected VmWeightArgPayloadJsonEdit(){}

	public static Ctx Mk(){
		return new Ctx();
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmWeightArgPayloadJsonEdit(){
		#if DEBUG
		{
			var o = new Ctx();
			Samples.Add(o);
		}
		#endif
	}

	Action<str>? OnApply{get;set;}

	public str LastError{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public bool HasError => !str.IsNullOrWhiteSpace(LastError);

	public str PayloadJson{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	/// 由主頁初始化 payload 並注入回寫回調。
	public nil Load(str? PayloadJson, Action<str>? OnApply){
		this.OnApply = OnApply;
		this.PayloadJson = FormatJson(PayloadJson ?? "");
		LastError = "";
		OnPropertyChanged(nameof(HasError));
		return NIL;
	}

	/// 檢查 JSON 並回寫主頁，不直接調後端接口。
	public nil ApplyAndBack(){
		if(!TryValidateJson(out var err)){
			LastError = err;
			OnPropertyChanged(nameof(HasError));
			ShowDialog(err);
			return NIL;
		}
		LastError = "";
		OnPropertyChanged(nameof(HasError));
		OnApply?.Invoke(PayloadJson);
		ViewNavi?.Back();
		return NIL;
	}

	bool TryValidateJson(out str Err){
		Err = "";
		if(str.IsNullOrWhiteSpace(PayloadJson)){
			return true;
		}
		try{
			_ = JsonNode.Parse(PayloadJson);
			return true;
		}catch(Exception e){
			Err = e.Message;
			return false;
		}
	}

	static str FormatJson(str UglyJson){
		if(str.IsNullOrWhiteSpace(UglyJson)){
			return "";
		}
		try{
			var node = JsonNode.Parse(UglyJson);
			return node?.ToJsonString(new JsonSerializerOptions{
				WriteIndented = true,
				Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
			}) ?? UglyJson;
		}catch{
			return UglyJson;
		}
	}
}
