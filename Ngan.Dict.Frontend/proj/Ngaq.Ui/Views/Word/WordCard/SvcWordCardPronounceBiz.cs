namespace Ngaq.Ui.Views.Word.WordCard;

using System;
using Ngaq.Core.Infra;
using Ngaq.Core.Shared.Audio;
using Ngaq.Core.Shared.Dictionary.Svc;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Po.UserLang;
using Ngaq.Core.Shared.Word.Svc;
using Tsinswreng.CsTools;

/// 單詞卡片朗讀業務服務。
/// 流程：UserLang -> NormLang 映射校驗 -> TTS 取音頻 -> 音頻播放。
public class SvcWordCardPronounceBiz: IWordCardPronounceBiz{
	readonly ISvcUserLang? SvcUserLang;
	readonly ISvcTts? SvcTts;
	readonly IAudioPlayer? AudioPlayer;

	public SvcWordCardPronounceBiz(
		ISvcUserLang? SvcUserLang,
		ISvcTts? SvcTts,
		IAudioPlayer? AudioPlayer
	){
		this.SvcUserLang = SvcUserLang;
		this.SvcTts = SvcTts;
		this.AudioPlayer = AudioPlayer;
	}

	public async Task<DtoWordCardPronounceResult> PronounceWord(
		IDbUserCtx DbUserCtx,
		IJnWord? JnWord,
		CT Ct
	){
		if(JnWord is null){
			return new DtoWordCardPronounceResult{
				Status = EWordCardPronounceStatus.NoWordSelected,
			};
		}
		if(str.IsNullOrWhiteSpace(JnWord.Lang)){
			return new DtoWordCardPronounceResult{
				Status = EWordCardPronounceStatus.WordLangEmpty,
			};
		}
		if(AnyNull(SvcUserLang, SvcTts, AudioPlayer)){
			return new DtoWordCardPronounceResult{
				Status = EWordCardPronounceStatus.ServiceUnavailable,
			};
		}

		try{
			var WordLang = JnWord.Lang.Trim();
			PoUserLang? mappedUserLang = null;
			var found = SvcUserLang.OrdGetUserLang(
				DbUserCtx,
				ToolAsyE.ToAsyE([WordLang]),
				Ct
			);
			await foreach(var po in found){
				if(po is not null){
					mappedUserLang = po;
					break;
				}
			}

			if(mappedUserLang is null || str.IsNullOrWhiteSpace(mappedUserLang.RelLang)){
				return new DtoWordCardPronounceResult{
					Status = EWordCardPronounceStatus.UserLangNotMapped,
					WordLang = WordLang,
				};
			}

			var audio = await SvcTts.GetAudio(JnWord.Head, mappedUserLang.ToNormLang());
			await AudioPlayer.Play(audio, Ct);
			return new DtoWordCardPronounceResult{
				Status = EWordCardPronounceStatus.Played,
				WordLang = WordLang,
			};
		}catch(Exception ex){
			return new DtoWordCardPronounceResult{
				Status = EWordCardPronounceStatus.Failed,
				Error = ex,
			};
		}
	}
}
