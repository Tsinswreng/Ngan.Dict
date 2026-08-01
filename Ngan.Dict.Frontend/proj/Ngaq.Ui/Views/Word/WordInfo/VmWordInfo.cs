using System.Collections.ObjectModel;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Word.Models.Samples;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Infra.I18n;

namespace Ngaq.Ui.Views.Word.WordInfo;
using Ctx = VmWordInfo;
using K = KeysUiI18nCommon;
public partial class VmWordInfo
	:ViewModelBase, IMk<Ctx>
{
	public static Ctx Mk(){
		return new Ctx();
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmWordInfo(){
		#if DEBUG
		{
			var o = new Ctx();
			Samples.Add(o);
			o.FromIWordForLearn(
				new WordForLearn(SampleWord.Inst.Samples[1])
			);
		}
		#endif
	}

	public Ctx FromIWordForLearn(IWordForLearn Word){
		WordForLearn = Word;
		Id = Word.Id.ToString();
		Head = Word.Head;
		Lang = Word.Lang;
		var DescriptionProps = new List<PoWordProp>();
		var DescriptionTexts = new List<str>();
		var SideProps = new List<PoWordProp>();
		foreach(var (StrKey, Props) in Word.StrKey_Props){
			foreach(var Prop in Props){
				var ClonedProp = (PoWordProp)((PoWordProp)Prop).ShallowCloneSelf();
				if(StrKey == KeysProp.Inst.description){
					DescriptionProps.Add(ClonedProp);
					DescriptionTexts.Add(ClonedProp.VStr ?? "");
					continue;
				}
				SideProps.Add(ClonedProp);
			}
		}
		DescriptionWordProps = DescriptionProps;
		Descrs = DescriptionTexts;
		SideWordProps = SideProps;
		return this;
	}

	public nil SetPromptBeforeStart(){
		ClearWordFields();
		SetDescriptionPrompt(I18n[K.PressStartButtonToBeginLearning]);
		return NIL;
	}

	public nil SetPromptAfterStart(){
		ClearWordFields();
		SetDescriptionPrompt(I18n[K.WordLearningHelpText_]);
		return NIL;
	}

	/// 清空當前單詞展示狀態，避免提示文本與上一個詞的內容混在一起。
	public nil ClearWordFields(){
		Id = "";
		Head = "";
		Lang = "";
		DescriptionWordProps = [];
		Descrs = [];
		SideWordProps = [];
		return NIL;
	}

	/// 無當前單詞時，提示文本也走 description prop 形態，避免與正常展示分叉。
	protected nil SetDescriptionPrompt(str PromptText){
		Descrs = [PromptText];
		DescriptionWordProps = [
			new PoWordProp{
				KType = EKvType.Str,
				KStr = KeysProp.Inst.description,
				VType = EKvType.Str,
				VStr = PromptText,
			}
		];
		return NIL;
	}


	public IWordForLearn? WordForLearn{get;set;}

	//public JnWord? BoWord{get;set;}

	public str Id{
		get;
		set{SetProperty(ref field, value);}
	} = "";



	public str Head{
		get;
		set{SetProperty(ref field, value);}
	} = "";

	public str Lang{
		get;
		set{SetProperty(ref field, value);}
	} = "";


	public IList<str> Descrs{
		get;
		set{SetProperty(ref field, value);}
	} = [];


	/// description 在信息頁也保留逐條 prop，便於直接跳到既有 WordProp 編輯框。
	public IList<PoWordProp> DescriptionWordProps{
		get;
		set{SetProperty(ref field, value);}
	} = [];

	/// 側欄按單條 prop 展示，便於每項直接掛編輯入口。
	public IList<PoWordProp> SideWordProps{
		get;
		set{SetProperty(ref field, value);}
	} = [];



}
