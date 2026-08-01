namespace Ngaq.Ui.Views.Dictionary.SimpleWord;

using System.Collections.ObjectModel;
using Ngaq.Core.Frontend.User;
using Ngaq.Core.Infra;
using Ngaq.Core.Shared.Audio;
using Ngaq.Core.Shared.Dictionary.Models;
using Ngaq.Core.Shared.Dictionary.Svc;
using Ngaq.Core.Shared.Word.Models.DictionaryApi;
using Ngaq.Ui.Infra;

using Ctx = VmSimpleWord;

/// 此文件只保存 VmSimpleWord 的函數實現；公開聲明位於同名 .cs 文件。
public partial class VmSimpleWord{
	protected partial VmSimpleWord(){}

	public static partial Ctx Mk(){
			return new Ctx();
		}

	public partial VmSimpleWord(
			ISvcTts? SvcTts
			,IAudioPlayer? AudioPlayer
			,ISvcDictionary? SvcDictionary
			,IFrontendUserCtxMgr? FrontendUserCtxMgr
		){
			this.SvcTts = SvcTts;
			this.AudioPlayer = AudioPlayer;
			this.SvcDictionary = SvcDictionary;
			this.FrontendUserCtxMgr = FrontendUserCtxMgr;
		}

	public partial nil FromRespLlmDict(IRespLlmDict Resp){
			// 最終結構化結果一旦落下，就不再接受晚到的流式片段覆蓋 UI 文本。
			IsFinalized = true;
			Head = Resp.Head;
			Pronunciations = Resp.Pronunciations.Select(p => new Pronunciation{
				TextType = p.TextType,
				Text = p.Text,
			}).ToList();
			var ParsedDescription = string.Join("\n", Resp.Descrs);
			// 解析出的描述為空時，保留流式階段已展示的文本，避免界面在收尾時變空白。
			if(!string.IsNullOrWhiteSpace(ParsedDescription)){
				Description = ParsedDescription;
			}
			return NIL;
		}

	public partial nil StartStreaming(string QueryTerm){
			// 新一輪查詞開始時重新打開流式寫入窗口。
			IsFinalized = false;
			Head = QueryTerm;
			Description = "";
			return NIL;
		}

	public partial nil GotNewSeg(DtoOnNewSeg NewSeg){
			// UI 執行緒中的流式片段可能晚於最終結果到達；
			// 若本輪已完成最終解析，則忽略這些遲到片段，避免把尾巴再拼回去。
			if(IsFinalized){
				return NIL;
			}
			Description += NewSeg.NewSeg;
			return NIL;
		}

	public partial nil StopStreaming(){
			IsFinalized = true;
			return NIL;
		}

	public async partial Task<nil> PlayHead(CT Ct){
			if(str.IsNullOrWhiteSpace(Head)){
				LogWarn($"{nameof(PlayHead)} skipped: Head is empty.");
				return NIL;
			}
			if(AnyNull(SvcTts, AudioPlayer, SvcDictionary, FrontendUserCtxMgr)){
				return NIL;
			}
	
			try{
				str UsedLangCode = "en";
				// Android 對主線程網絡請求有限制；這裡把「取音頻 + 播放」整段放到後台執行。
				await Task.Run(async ()=>{
					// step 1: 取當前詞典源語言，作為 TTS 的語言參數。
					var SrcLangPo = await SvcDictionary.GetCurSrcNormLang(
						FrontendUserCtxMgr.GetDbUserCtx(),
						Ct
					);
					var Lang = new NormLang{
						Type = ELangIdentType.Bcp47,
						Code = "en",
					};
					if(SrcLangPo is not null && !str.IsNullOrWhiteSpace(SrcLangPo.Code)){
						Lang.Type = SrcLangPo.Type;
						Lang.Code = SrcLangPo.Code;
					}else{
						LogWarn($"{nameof(PlayHead)}: CurSrcNormLang is null, fallback to en.");
					}
					UsedLangCode = Lang.Code;
	
					// step 2: 把詞頭傳給 ISvcTts，獲取音頻數據。
					var Audio = await SvcTts.GetAudio(
						Head.Trim(),
						Lang
					);
	
					// step 3: 調用播放器播放（按返回音頻類型自動處理）。
					await AudioPlayer.Play(Audio, Ct);
				}, Ct);
				LogInfo($"{nameof(PlayHead)} success. Head={Head.Trim()}, Lang={UsedLangCode}");
			}catch(Exception Ex){
				LogError($"{nameof(PlayHead)} failed. Head={Head.Trim()}");
				HandleErr(Ex);
			}
	
			return NIL;
		}
}

