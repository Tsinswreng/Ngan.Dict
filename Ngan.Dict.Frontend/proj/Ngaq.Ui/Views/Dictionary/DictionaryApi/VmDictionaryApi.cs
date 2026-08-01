namespace Ngaq.Ui.Views.Dictionary.DictionaryApi;
using System.Collections.ObjectModel;
using AvaloniaEdit.Utils;
using Ngaq.Core.Shared.Word.Models.DictionaryApi;
using Ngaq.Ui.Infra;

using Ctx = VmDictionaryApi;
public partial class VmDictionaryApi: ViewModelBase, IMk<Ctx>{
	//蔿從構造函數依賴注入、故以靜態工廠代無參構造器
	protected VmDictionaryApi(){}
	public static Ctx Mk(){
		return new Ctx();
	}

	public static ObservableCollection<Ctx> Samples = [];
	static VmDictionaryApi(){
		#if DEBUG
		var o = new Ctx{ Words = new List<DictionaryApiWord>{
			new(){
				word = "apple",
				phonetics = new List<Phonetic>{ new(){ text = "/ˈæpl/" } },
				meanings = new List<Meaning>{
					new(){
						partOfSpeech = "noun",
						definitions = new List<Definition>{
							new(){ definition = "A round fruit with red or green skin.", example = "She ate an apple." }
						}
					}
				}
			},
			new(){
				word = "run",
				phonetics = new List<Phonetic>{ new(){ text = "/rʌn/" } },
				meanings = new List<Meaning>{
					new(){
						partOfSpeech = "verb",
						definitions = new List<Definition>{
							new(){ definition = "To move swiftly on foot." }
						}
					}
				}
			}
		}};
		Samples.Add(o);
		#endif
	}

	public IList<DictionaryApiWord> Words{
		get;
		set{SetProperty(ref field, value);}
	} = [];



}
