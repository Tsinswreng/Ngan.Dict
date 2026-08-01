using Ngaq.Core.Shared.Dictionary.Models;

namespace Ngaq.Core.Shared.Word.Models.DictionaryApi;

public static class ExtnPhonetic{
	extension(Phonetic z){
		public async Task<Pronunciation> ToPronunciation(CT Ct){
			var R = new Pronunciation();
			R.Lang = "English";
			R.AudioUrl = z.audio;
			R.TextType = EPronunciationTextType.Ipa.ToString();
			R.Lang = "English";
			await R.AssignAudioFromUrl(Ct);
			return R;
		}
	}
}



