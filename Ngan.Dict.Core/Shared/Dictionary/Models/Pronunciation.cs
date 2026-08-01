namespace Ngaq.Core.Shared.Dictionary.Models;

using Ngaq.Core.Shared.Audio;
using Ngaq.Core.Shared.Word.Models;
using Tsinswreng.CsErr;

[Doc(@$"""
#Example
""")]
public class TextedPronunciation{
	//如 Ipa, 假名 等
	[Doc(@$"#See({nameof(EPronunciationTextType)})")]
	public str TextType{get;set;} = "";
	public str Text{get;set;} = "";
}

public class Pronunciation:TextedPronunciation{
	public str Lang{get;set;} = "";
	public str? AudioUrl{get;set;}
	public Audio? Audio{get;set;}
	public class Sample{
		public static IList<Pronunciation> Samples = [];
		static Sample(){
	#if DEBUG
			{
				var o = new Pronunciation();
				Samples.Add(o);
				o.Text = "həˈləʊ";
				o.AudioUrl = "https://api.dictionaryapi.dev/media/pronunciations/en/hello-uk.mp3";
			}
	#endif
		}
	}
}

public static class ExtnPronunciation{
	extension(Pronunciation z){
		public async Task<IAnswer<nil>> AssignAudioFromUrl(CT Ct){
			var R = new Answer<nil>();
			if(str.IsNullOrEmpty(z.AudioUrl)){
				return R.AddErr("AudioUrl is null or empty.");
			}
			try{
				var OnlineAudio = new OnlineAudio();
				z.Audio = await OnlineAudio.Get(z.AudioUrl, Ct);
			}
			catch (System.Exception e){
				return R.AddErr(e);
			}
			return R.OkWith(NIL);
		}
	}
}
