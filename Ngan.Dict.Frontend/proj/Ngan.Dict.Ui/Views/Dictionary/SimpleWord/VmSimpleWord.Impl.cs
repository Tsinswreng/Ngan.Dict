namespace Ngan.Dict.Ui.Views.Dictionary.SimpleWord;

using System.Collections.ObjectModel;
using System.Text;
using Ngan.Dict.Core.Frontend.Clipboard;
using Ngan.Dict.Core.Frontend.User;
using Ngan.Dict.Core.Infra;
using Ngan.Dict.Core.Shared.Audio;
using Ngan.Dict.Core.Shared.Dictionary.Models;
using Ngan.Dict.Core.Shared.Dictionary.Svc;
using Ngan.Dict.Core.Shared.Word.Models.DictionaryApi;
using Ngan.Dict.Ui.Infra;

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
			,ISvcClipboard? SvcClipboard
		){
			this.SvcTts = SvcTts;
			this.AudioPlayer = AudioPlayer;
			this.SvcDictionary = SvcDictionary;
			this.FrontendUserCtxMgr = FrontendUserCtxMgr;
			this.SvcClipboard = SvcClipboard;
		}

	public partial nil FromRespLlmDict(IRespLlmDict Resp){
			// 最終結構化結果一旦落下，就不再接受晚到的流式片段覆蓋 UI 文本。
			IsFinalized = true;
			Head = Resp.Head;
			Pronunciations = Resp.Pronunciations.Select(p => new Pronunciation{
				TextType = p.TextType,
				Text = p.Text,
			}).ToList();

			// 翻譯模式：模型回包的 Kind 優先（可能是 Sentence/Paragraph），沒有就沿用請求端的判定。
			var RespKind = ParseKind(Resp.Kind);
			var IsRespTranslate = RespKind != EQueryKind.Word || !str.IsNullOrWhiteSpace(Resp.Translation);
			Kind = IsRespTranslate
				? (RespKind == EQueryKind.Word ? EQueryKind.Sentence : RespKind)
				: EQueryKind.Word;
			SourceText = str.IsNullOrWhiteSpace(Resp.SourceText) ? Head : Resp.SourceText;
			Translation = Resp.Translation;
			Segments = Resp.Segments;
			Notes = Resp.Notes;

			// 結構化義項供界面分塊排版；舊格式（只有 Descrs）已在後端補成 Senses，這裏再兜一次底。
			Senses = Resp.Senses.Count > 0
				? Resp.Senses
				: Resp.Descrs
					.Where(One => !str.IsNullOrWhiteSpace(One))
					.Select(One => new DictSense{ Meaning = One })
					.ToList();

			// 翻譯模式下 `Description` 就是譯文：複製按鈕沿用同一條路徑，不必另開一份。
			// 詞典模式則把結構化義項重新攤平成純文本，保持「複製出來就是可讀的一整段」。
			var ParsedDescription = IsRespTranslate
				? Resp.Translation
				: FormatSensesAsText(Senses);
			// 解析出的描述為空時，保留流式階段已展示的文本，避免界面在收尾時變空白。
			if(!string.IsNullOrWhiteSpace(ParsedDescription)){
				Description = ParsedDescription;
			}
			return NIL;
		}

	/// 把結構化義項攤平成純文本，供複製／保存使用。
	/// 格式沿用舊版的直覺：`【詞性】釋義`，例句縮排一行，譯文再縮排一行。
	public static partial str FormatSensesAsText(IList<DictSense> Senses){
		var Sb = new StringBuilder();
		foreach(var Sense in Senses){
			if(Sb.Length > 0){
				Sb.Append('\n');
			}
			if(!str.IsNullOrWhiteSpace(Sense.Pos)){
				Sb.Append('【').Append(Sense.Pos).Append('】');
			}
			Sb.Append(Sense.Meaning);
			foreach(var Example in Sense.Examples){
				Sb.Append('\n').Append("例：").Append(Example.Src);
				if(!str.IsNullOrWhiteSpace(Example.Dst)){
					Sb.Append('\n').Append("譯：").Append(Example.Dst);
				}
			}
		}
		return Sb.ToString();
	}

	/// 把模型回包的 `Kind` 字串轉成枚舉；認不出的一律當 `Word`，由譯文是否存在另行兜底。
	static EQueryKind ParseKind(str? Kind){
		if(str.IsNullOrWhiteSpace(Kind)){
			return EQueryKind.Word;
		}
		return Kind.Trim() switch{
			"Sentence" or "sentence" => EQueryKind.Sentence,
			"Paragraph" or "paragraph" => EQueryKind.Paragraph,
			_ => EQueryKind.Word,
		};
	}

	public partial nil StartStreaming(string QueryTerm){
			// 新一輪查詞開始時重新打開流式寫入窗口，並清掉上一輪的翻譯欄位，
			// 否則切到詞典模式時會殘留上一段的原文／譯文。
			IsFinalized = false;
			Head = QueryTerm;
			Description = "";
			SourceText = "";
			Translation = "";
			Segments = [];
			Notes = [];
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

	public async partial Task<nil> CopyDescription(CT Ct){
		Ct.ThrowIfCancellationRequested();
		// Description 是解析階段以換行拼接的全部 Descr；流式尚未產生內容時不覆蓋用戶剪貼板。
		if(str.IsNullOrWhiteSpace(Description) || SvcClipboard is null){
			return NIL;
		}
		await SvcClipboard.SetText(Description, Ct);
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
