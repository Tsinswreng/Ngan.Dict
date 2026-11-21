using Ngan.Dict.Core.Shared.Dictionary.Models;
using Ngan.Dict.Core.Shared.Dictionary.Svc;
using Ngan.Dict.Core.Shared.User.UserCtx;
using Ngan.Dict.Core.Infra.Cfg;
using Ngan.Dict.Core.Tools.Json;
using Ngan.Dict.Core.Infra.Errors;
using Tsinswreng.CsCfg;
using Tsinswreng.CsErr;
using Tsinswreng.CsCore;
using Tsinswreng.CsTools;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Ngan.Dict.Core.Tools;
using Tsinswreng.CsJsonr;
using Microsoft.Extensions.Logging;
using Ngan.Dict.Core.Infra;
using Ngan.Dict.Core.Shared.Dictionary.Models.Po.NormLang;
using Ngan.Dict.Core.Shared.Kv;
using Ngan.Dict.Core.Shared.Kv.Models;
using Ngan.Dict.Core.Shared.Kv.Svc;
using Ngan.Dict.Core.Shared.Word.Models.Po.Kv;
using Ngan.Dict.Core.Shared.Word.Svc;
using Tsinswreng.CsSql;
using Tsinswreng.CsJson;



namespace Ngan.Dict.Backend.Domains.Dictionary.Svc;

#region DTOs

/// DTO for LLM API call parameters
public class DtoLlmCallParam{
	public string? ApiUrl{get;set;}
	public string? ApiKey{get;set;}
	public string? Model{get;set;}
	/// 額外透傳到請求體頂層的 provider-specific JSON 對象。
	public string? ExtraBodyJson{get;set;}

	/// 用戶提示詞

	public string? UserPrompt{get;set;}

	/// 本次查詢類型；決定用查詞還是翻譯的系統提示詞。
	public EQueryKind Kind{get;set;} = EQueryKind.Word;
}

/// DTO for LLM API response
public class DtoLlmApiResp{

	/// Raw JSON response text (for debugging)

	public string? RawResponse{get;set;}


	/// Extracted content from choices[0].message.content

	public string? Content{get;set;}
}

#endregion

public class SvcDictionary:ISvcDictionary{
	ICfgAccessor Cfg;
	IJsonSerializer JsonS;
	HttpClient HttpClient;
	ILogger Logger;
	ISvcKv SvcKv;
	ISvcNormLang SvcNormLang;
	IRepo<PoKv, IdKv> RepoKv;
	ISqlCmdMkr SqlCmdMkr;
	public SvcDictionary(
		ICfgAccessor Cfg
		,IJsonSerializer JsonS
		,ILogger Logger
		,ISvcKv SvcKv
		,ISvcNormLang SvcNormLang
		,IRepo<PoKv, IdKv> RepoKv
		,ISqlCmdMkr SqlCmdMkr
	){
		this.Cfg = Cfg;
		this.JsonS = JsonS;
		this.HttpClient = new HttpClient();
		this.Logger = Logger;
		this.SvcKv = SvcKv;
		this.SvcNormLang = SvcNormLang;
		this.RepoKv = RepoKv;
		this.SqlCmdMkr = SqlCmdMkr;
	}

	public async Task<PoNormLang?> GetCurSrcNormLang(IDbUserCtx Ctx, CT Ct){
		return await LoadOrInitCurLang(
			Ctx, KeysKv.Dictionary.CurSrcLang+"", "en", "English", Ct
		);
	}

	public async Task<PoNormLang?> SetCurSrcNormLang(
		IDbUserCtx Ctx, PoNormLang Po, CT Ct
	){
		var normalized = NormalizeCurLang(Ctx, Po);
		await SaveCurLang(Ctx, KeysKv.Dictionary.CurSrcLang+"", normalized, Ct);
		return normalized;
	}

	public async Task<PoNormLang?> GetCurTgtNormLang(IDbUserCtx Ctx, CT Ct){
		return await LoadOrInitCurLang(
			Ctx, KeysKv.Dictionary.CurTgtLang+"", "zh", "中文", Ct
		);
	}

	public async Task<PoNormLang?> SetCurTgtNormLang(
		IDbUserCtx Ctx, PoNormLang Po, CT Ct
	){
		var normalized = NormalizeCurLang(Ctx, Po);
		await SaveCurLang(Ctx, KeysKv.Dictionary.CurTgtLang+"", normalized, Ct);
		return normalized;
	}

	/// 讀取詞典系統提示語。
	/// 若配置中尚未設置，則把默認提示語寫回配置後返回，保證設置頁能看到可編輯的初始值。
	public async Task<str> GetLlmDictSysPromptOrDflt(IDbUserCtx Ctx, CT Ct){
		_ = Ctx;
		var Prompt = Cfg.Get(KeysClientCfg.LlmDictionary.Prompt);
		if(!str.IsNullOrWhiteSpace(Prompt)){
			return Prompt!;
		}
		var Dflt = DfltPrompt.Prompt;
		Cfg.SetNoSave(KeysClientCfg.LlmDictionary.Prompt, Dflt);
		await Cfg.Save(Ct);
		return Dflt;
	}

	/// 保存詞典系統提示語，並返回保存後的值。
	public async Task<str> SetLlmDictSysPrompt(IDbUserCtx Ctx, str Prompt, CT Ct){
		_ = Ctx;
		var Normalized = string.IsNullOrWhiteSpace(Prompt) ? DfltPrompt.Prompt : Prompt;
		Cfg.SetNoSave(KeysClientCfg.LlmDictionary.Prompt, Normalized);
		await Cfg.Save(Ct);
		return Normalized;
	}

	public Task<IList<NormLang>> GetRecentUsedNormLangs(IDbUserCtx Ctx, CT Ct){
		IList<NormLang> r = [];
		return Task.FromResult(r);
	}

	/*
大模型輸出格式為 jsonr（無轉義 JSON 超集）：提示詞要求裸輸出 JSON，多行釋義用 heredoc 文本塊。
解析走 ParseResponse：先剝掉模型偶爾仍加的 markdown 代碼塊外殼，再交給 jsonr 轉標準 JSONC。
*/
	[Impl]
	public async Task<IRespLlmDict> Lookup(IUserCtx User, IReqLlmDict Req, CT Ct){
		_ = User;
		var apiUrl = Cfg.Get(KeysClientCfg.LlmDictionary.ApiUrl);
		var apiKey = Cfg.Get(KeysClientCfg.LlmDictionary.ApiKey);
		var model = Cfg.Get(KeysClientCfg.LlmDictionary.Model);

		if(string.IsNullOrWhiteSpace(apiUrl) || string.IsNullOrWhiteSpace(apiKey)){
			throw KeysErr.Dictionary.LlmApiNotConfigured.ToErr();
		}

		var userPrompt = BuildUserPrompt(Req);

		// 查詢類型：調用方有填就用；沒填（老調用方）就按輸入文本兜底判定。
		// 它決定用哪套系統提示詞（查詞／翻譯），故必須在挑 prompt 之前定好。
		var Kind = Req.Query.Kind;
		if(Kind == EQueryKind.Word){
			Kind = ToolQueryKind.Detect(Req.Query.InputText);
		}

		// 统一走流式输出流程
		// 如果传参方不按流式输出传，就自己攒着等完了一起发结果回去
		ReqLlmDictEvt? internalEvt = null;
		IReqLlmDictEvt evtToUse;

		if(Req is IReqLlmDictEvt reqEvt){
			evtToUse = reqEvt;
		}else{
			// 创建内部事件对象来收集流式输出
			internalEvt = new ReqLlmDictEvt();
			evtToUse = internalEvt;
		}

		var dtoParam = new DtoLlmCallParam{
			ApiUrl = apiUrl,
			ApiKey = apiKey,
			Model = model,
			ExtraBodyJson = Cfg.Get(KeysClientCfg.LlmDictionary.ExtraBodyJson),
			UserPrompt = userPrompt,
			Kind = Kind,
		};

		var result = await CallLlmApiStream(evtToUse, dtoParam, Ct);
		return result;
	}

	/// 構造發給大模型的用戶提示詞。
	/// 此處只傳遞統一的來源文本與任務類型；查詞或翻譯的格式約束由 system prompt 負責。
	/// `<kind>` 讓模型知道要回哪種 schema，也讓它在 `Kind` 欄位上保持一致。
	private string BuildUserPrompt(IReqLlmDict Req){
		var R =
$"""
<dictionary-request>
  <kind>{Req.Query.Kind}</kind>
  <source-language>{FormatLang(Req.OptLang.SrcLang)}</source-language>
  <target-languages>{FormatLangList(Req.OptLang.TgtLangs)}</target-languages>
  <input-text>
{Req.Query.InputText}
  </input-text>
  <context-sentence>
{Req.Query.ContextSentence}
  </context-sentence>
  <preferences>{FormatPreferences(Req.Preferences)}</preferences>
</dictionary-request>
""";
		return R;
	}

	/// 直接解析 LLM 原始輸出文本，不觸發外部 API 調用。
	public IRespLlmDict ParseRawOutput(str RawOutput){
		var dtoResp = new DtoLlmApiResp{
			RawResponse = RawOutput,
			Content = RawOutput,
		};
		return ParseResponse(dtoResp);
	}

	private async Task<PoNormLang?> LoadOrInitCurLang(
		IDbUserCtx Ctx,
		str Key,
		str DfltCode,
		str DfltNativeName,
		CT Ct
	){
		Ctx.DbFnCtx ??= new DbFnCtx();
		var owner = Ctx.UserCtx.UserId;
		var kv = await SvcKv.OrdGetByOwnerEtKStr(
			Ctx.DbFnCtx,
			ToolAsyE.ToAsyE([(owner, Key)]),
			Ct
		).FirstOrDefaultAsync(Ct);

		var parsed = ParsePoNormLang(kv?.GetVStr());
		if(parsed is not null && !str.IsNullOrWhiteSpace(parsed.Code)){
			parsed.Owner = owner;
			return parsed;
		}

		var dflt = await GetOrMkBuiltinNormLang(Ctx, DfltCode, DfltNativeName, Ct);
		await SaveCurLang(Ctx, Key, dflt, Ct);
		return dflt;
	}

	private async Task<PoNormLang> GetOrMkBuiltinNormLang(
		IDbUserCtx Ctx,
		str Code,
		str DfltNativeName,
		CT Ct
	){
		var po = await SvcNormLang.OrdGetNormLangByTypeCode(
			Ctx,
			ToolAsyE.ToAsyE([(ELangIdentType.Bcp47, Code)]),
			Ct
		).FirstOrDefaultAsync(Ct);
		if(po is not null){
			po.Owner = Ctx.UserCtx.UserId;
			return po;
		}
		return new PoNormLang{
			Owner = Ctx.UserCtx.UserId,
			Type = ELangIdentType.Bcp47,
			Code = Code,
			NativeName = DfltNativeName,
		};
	}

	private async Task<nil> SaveCurLang(
		IDbUserCtx Ctx,
		str Key,
		PoNormLang Po,
		CT Ct
	){
		Ctx.DbFnCtx ??= new DbFnCtx();
		var owner = Ctx.UserCtx.UserId;
		var json = JsonS.Serialize(Po);

		return await SqlCmdMkr.EnsureTxn(Ctx.DbFnCtx, Ct, async DbCtx=>{
			var oldKv = await SvcKv.OrdGetByOwnerEtKStr(
				DbCtx,
				ToolAsyE.ToAsyE([(owner, Key)]),
				Ct
			).FirstOrDefaultAsync(Ct);
			if(oldKv is null){
				var kv = new PoKv{
					Owner = owner,
				}.SetStrStr(Key, json);
				await RepoKv.OrdAdd(DbCtx, ToolAsyE.ToAsyE([kv]), Ct);
				return NIL;
			}
			oldKv.Owner = owner;
			oldKv.SetStrStr(Key, json);
			await RepoKv.OrdUpd(DbCtx, ToolAsyE.ToAsyE([oldKv]), Ct);
			return NIL;
		});
	}

	private static PoNormLang NormalizeCurLang(IDbUserCtx Ctx, PoNormLang Po){
		return new PoNormLang{
			Id = Po.Id,
			Owner = Ctx.UserCtx.UserId,
			Type = Po.Type == ELangIdentType.Unknown ? ELangIdentType.Bcp47 : Po.Type,
			Code = (Po.Code ?? "").Trim(),
			NativeName = (Po.NativeName ?? "").Trim(),
		};
	}

	/// 轉換單個語言描述，盡量把 code / native name / english name 都寫進 prompt。
	static str FormatLang(INormLangDetail Lang){
		var Segs = new List<str>{
			$"code={ToPromptValue(Lang.Code)}",
			$"type={Lang.Type}",
		};
		if(!str.IsNullOrWhiteSpace(Lang.NativeName)){
			Segs.Add($"NativeName={ToPromptValue(Lang.NativeName)}");
		}
		if(!str.IsNullOrWhiteSpace(Lang.EnglishName)){
			Segs.Add($"EnglishName={ToPromptValue(Lang.EnglishName)}");
		}
		return string.Join(", ", Segs);
	}

	/// 轉換多個目標語言描述。
	static str FormatLangList(IEnumerable<INormLangDetail> Langs){
		var Items = Langs.Select(FormatLang).Where(x=>!str.IsNullOrWhiteSpace(x));
		var Joined = string.Join("; ", Items);
		return Joined == "" ? "(none)" : Joined;
	}

	/// 將偏好配置轉為可讀文字；空值時顯式標識未指定，避免模型自行腦補。
	static str FormatPreferences(Preferences? Preferences){
		if(Preferences is null){
			return "";
		}
		return string.Join(", ", [
			$"TryIncludeExamples={Preferences.TryIncludeExamples}",
			$"TryIncludeSynonyms={Preferences.TryIncludeSynonyms}",
			$"TryIncludeAntonyms={Preferences.TryIncludeAntonyms}",
			$"TryIncludeEtymology={Preferences.TryIncludeEtymology}",
			$"MaxExamples={Preferences.MaxExamples}",
		]);
	}

	/// 將 prompt 值規整成單行文本；空白值統一標記為 null。
	static str ToPromptValue(str? Value){
		if(str.IsNullOrWhiteSpace(Value)){
			return "null";
		}
		return Value.Trim().Replace("\r", " ").Replace("\n", " ");
	}

	private PoNormLang? ParsePoNormLang(str? Json){
		if(str.IsNullOrWhiteSpace(Json)){
			return null;
		}
		try{
			return JsonS.Deserialize<PoNormLang>(Json!);
		}catch{
			return null;
		}
	}

	/// 调用 LLM API 流式输出
	private async Task<IRespLlmDict> CallLlmApiStream(
		IReqLlmDictEvt evt,
		DtoLlmCallParam param,
		CT Ct
	){
		// 按查詢類型挑系統提示詞：詞典與翻譯的輸出約束不同，不能共用一份。
		// 配置留空時回落到各自的內置提示詞。
		var IsTranslate = param.Kind != EQueryKind.Word;
		var SysPrompt = Cfg.Get(
			IsTranslate
				? KeysClientCfg.LlmDictionary.PromptTranslate
				: KeysClientCfg.LlmDictionary.Prompt
		);
		if(string.IsNullOrWhiteSpace(SysPrompt)){
			SysPrompt = IsTranslate ? DfltPromptTranslate.Prompt : DfltPrompt.Prompt;
		}
		// 用具體 DTO 組 OpenAI-compatible 請求體：線上式不能出現 object／Dictionary<string,object?>，
		// 否則源生成 context 沒有它的元資料，AOT 與本機都會直接拋 NotSupportedException。
		var reqBody = new DtoLlmApiReq{
			Model = param.Model,
			Messages = [
				new DtoLlmMessage{ Role = "system", Content = SysPrompt },
				new DtoLlmMessage{ Role = "user", Content = param.UserPrompt },
			],
			Stream = true
		};
		// 先構造通用主體，再允許配置追加頂層字段，以便適配 DeepSeek 等 provider 的私有參數。
		MergeExtraBodyJson(reqBody, param.ExtraBodyJson);
		var json = AppJsonS.Inst.Serialize(reqBody, typeof(DtoLlmApiReq), AppJsonCtx.Default);
		var content = new StringContent(json, Encoding.UTF8, "application/json");

		var request = new HttpRequestMessage(HttpMethod.Post, param.ApiUrl);
		request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", param.ApiKey);
		request.Content = content;

		// 使用 ResponseHeadersRead 以支持流式读取
		using var response = await HttpClient
			.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, Ct)
			.ConfigureAwait(false);
		response.EnsureSuccessStatusCode();

		var fullContent = new StringBuilder();
		try{
			using var stream = await response.Content.ReadAsStreamAsync(Ct).ConfigureAwait(false);
			using var reader = new StreamReader(stream);

			// 逐行解析 SSE。
			// 這裡必須把取消令牌傳進 ReadLineAsync，否則模型已開始流式輸出後，
			// 若下一行尚未到達，前端二次點擊取消時會卡在等待下一行，表面上看起來像「取消無效」。
			while(await reader.ReadLineAsync(Ct).ConfigureAwait(false) is { } line){
				if(string.IsNullOrEmpty(line)) continue;
				if(!line.StartsWith("data: ")) continue;

				var payload = line["data: ".Length..];
				if(payload == "[DONE]"){
					// 流结束
					evt.OnDone?.Invoke(new DtoOnDone(), Ct);
					break;
				}

				try{
					using var doc = System.Text.Json.JsonDocument.Parse(payload);
					var root = doc.RootElement;

					if(root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0){
						var delta = choices[0].GetProperty("delta");
						if(delta.TryGetProperty("content", out var contentProp)){
							var seg = contentProp.GetString();
							if(!string.IsNullOrEmpty(seg)){
								fullContent.Append(seg);
								// 触发事件
								evt.OnNewSeg?.Invoke(new DtoOnNewSeg{ NewSeg = seg }, Ct);
							}
						}
					}
				}catch(System.Text.Json.JsonException){
					// 忽略解析错误，继续处理下一行
					Logger.LogWarning("Failed to parse SSE line: {Line}", line);
				}
			}

			// 構建最終響應：RawResponse / Content 都記錄完整拼接文本，便於錯誤排查。
			var merged = fullContent.ToString();
			return ParseRawOutput(merged);
		}catch(OperationCanceledException ex) when(Ct.IsCancellationRequested){
			Logger.LogInformation(
				ex,
				"LLM dictionary stream canceled by caller. Partial LLM response: {LlmResponse}",
				fullContent.ToString()
			);
			throw;
		}catch(Exception ex){
			Logger.LogError(
				ex,
				"LLM dictionary stream failed. Partial LLM response: {LlmResponse}",
				fullContent.ToString()
			);
			throw;
		}
	}

	/// 把配置中的頂層 JSON 對象 merge 到請求體。
	/// 當存在同名鍵時，採用配置值覆蓋默認值，方便顯式關閉 provider 默認行為。
	/// *為何不用 IJsonSerializer*：`Dictionary<string, object?>` 在源生成 context 裏沒有元資料，
	/// 而配置裏的值本來就是任意 JSON；用 `JsonDocument` 逐屬性 Clone 進去，不經反射、也不經源生成。
	private static void MergeExtraBodyJson(DtoLlmApiReq ReqBody, str? ExtraBodyJson){
		if(str.IsNullOrWhiteSpace(ExtraBodyJson)){
			return;
		}
		try{
			using var Doc = JsonDocument.Parse(ExtraBodyJson, new JsonDocumentOptions{
				CommentHandling = JsonCommentHandling.Skip,
				AllowTrailingCommas = true,
			});
			if(Doc.RootElement.ValueKind != JsonValueKind.Object){
				throw KeysErr.Common.ArgErr.ToErr()
					.AddDebugArgs(nameof(KeysClientCfg.LlmDictionary.ExtraBodyJson), ExtraBodyJson);
			}
			// 同名鍵由配置覆蓋默認值：字典賦值即覆蓋語義。
			ReqBody.Extra ??= [];
			foreach(var Prop in Doc.RootElement.EnumerateObject()){
				ReqBody.Extra[Prop.Name] = Prop.Value.Clone();
			}
		}catch(TypedErr){
			throw;
		}catch(Exception ex){
			throw KeysErr.Common.ArgErr.ToErr()
				.AddErr(ex)
				.AddDebugArgs(nameof(KeysClientCfg.LlmDictionary.ExtraBodyJson), ExtraBodyJson);
		}
	}

	/// 解析 LLM 響應文本為 RespLlmDict。
	/// 流程：剝掉可能的 markdown 外殼 → jsonr 轉標準 JSONC → 反序列化。
	/// *為何經 jsonr*：模型用 heredoc 文本塊寫多行釋義時不必轉義，
	/// jsonr 會把文本塊原樣還原成普通 JSON 字串，後續解析與提示詞寫法無關。
	private IRespLlmDict ParseResponse(DtoLlmApiResp dtoResp){
		var rawResponse = dtoResp.RawResponse;
		var content = dtoResp.Content;
		try{
			var JsonrText = StripMarkdownFence(content ?? "");
			var Jsonc = JsonrToJsonc(JsonrText);
			Logger.LogWarning(
				"Dictionary parse diag: {Stage}",
				DescribeTextTail("Jsonc", Jsonc)
			);
			var R = JsonS.Deserialize<RespLlmDict>(Jsonc);
			NormalizeSenses(R);
			Logger.LogWarning(
				"Dictionary parse diag: {Stage}",
				DescribeDescrsFromResp(R)
			);
			return R!;
		}catch(System.Exception ex){
			Err(ex);
		}
		return null!;
		
		void Err(Exception? ex){
			Logger.LogError(
				ex,
				"Failed to parse LLM response. Content: {Content}; Raw response: {RawResponse}",
				content,
				rawResponse
			);
			throw KeysErr.Dictionary.LlmResponseParseFailed.ToErr()
				.AddDebugArgs(ex, rawResponse, content);
		};
	}

	/// 把舊版純文本釋義補成結構化義項。
	/// *兼容目的*：用戶可能仍在使用自定義的舊提示詞（只給 `Descrs`），
	/// 或本地快取是舊格式；兩種情況都不該讓界面退化成「一整段文字」。
	/// 舊格式每條釋義內部用「【詞性】釋義」「Example: …」「Translation: …」分層，
	/// 這裏按這套約定做一次盡力而為的拆解。
	private static nil NormalizeSenses(RespLlmDict R){
		if(R is null){
			return NIL;
		}
		if(R.Senses.Count > 0){
			return NIL;
		}
		foreach(var Raw in R.Descrs){
			if(str.IsNullOrWhiteSpace(Raw)){
				continue;
			}
			foreach(var Block in SplitLegacyDescr(Raw)){
				R.Senses.Add(Block);
			}
		}
		return NIL;
	}

	/// 把一條舊格式釋義（可能含多個義項）拆成若干結構化義項。
	private static IList<DictSense> SplitLegacyDescr(str Raw){
		IList<DictSense> Ans = [];
		DictSense? Cur = null;
		SenseExample? CurExample = null;
		foreach(var LineRaw in Raw.Split('\n')){
			var Line = LineRaw.Trim();
			if(Line.Length == 0){
				continue;
			}
			// 「【名詞】釋義」：新的義項起頭。
			if(Line.StartsWith('【') && Line.Contains('】')){
				var End = Line.IndexOf('】');
				var Pos = Line[1..End].Trim();
				var Meaning = Line[(End+1)..].Trim();
				Cur = new DictSense{ Pos = Pos, Meaning = Meaning };
				Ans.Add(Cur);
				CurExample = null;
				continue;
			}
			// 「Example: …」開新例句。
			if(Line.StartsWith("Example:", StringComparison.OrdinalIgnoreCase)
				|| Line.StartsWith("例句:", StringComparison.OrdinalIgnoreCase)
				|| Line.StartsWith("例句：", StringComparison.OrdinalIgnoreCase)
			){
				var Src = Line[(Line.IndexOfAny([':', '：'])+1)..].Trim();
				CurExample = new SenseExample{ Src = Src };
				Cur ??= AddImplicitSense(Ans);
				Cur.Examples.Add(CurExample);
				continue;
			}
			// 「Translation: …」補到當前例句。
			if(Line.StartsWith("Translation:", StringComparison.OrdinalIgnoreCase)
				|| Line.StartsWith("翻譯:", StringComparison.OrdinalIgnoreCase)
				|| Line.StartsWith("译文:", StringComparison.OrdinalIgnoreCase)
				|| Line.StartsWith("譯文:", StringComparison.OrdinalIgnoreCase)
				|| Line.StartsWith("译文：", StringComparison.OrdinalIgnoreCase)
				|| Line.StartsWith("譯文：", StringComparison.OrdinalIgnoreCase)
			){
				var Dst = Line[(Line.IndexOfAny([':', '：'])+1)..].Trim();
				if(CurExample is not null){
					CurExample.Dst = Dst;
				}
				continue;
			}
			// 其餘行：併入當前義項的釋義；沒有義項就開一個無詞性的。
			Cur ??= AddImplicitSense(Ans);
			Cur.Meaning = str.IsNullOrWhiteSpace(Cur.Meaning) ? Line : Cur.Meaning + "\n" + Line;
		}
		return Ans;
	}

	/// 舊格式沒有詞性前綴時，補一個無詞性的義項。
	private static DictSense AddImplicitSense(IList<DictSense> Ans){
		var S = new DictSense{ Pos = "", Meaning = "" };
		Ans.Add(S);
		return S;
	}

	/// 用 jsonr 把模型輸出轉成標準 JSONC 文本。
	/// heredoc／raw string 文本塊會被轉義成普通 JSON 字串；註釋與其餘字符原樣保留。
	private static str JsonrToJsonc(str JsonrText){
		using var Reader = Jsonr.StrToUtf8Stream(JsonrText);
		using var Writer = new MemoryStream();
		Jsonr.Inst.ToJson(Reader, Writer);
		// Utf8StreamToStr 從流當前位置讀到流尾，故先回到開頭。
		Writer.Position = 0;
		return Jsonr.Utf8StreamToStr(Writer);
	}

	/// 剝掉模型習慣性加上的 markdown 代碼塊外殼（```json … ```）。
	/// 提示詞已要求裸輸出；剝殼只是容錯，對本來就裸的輸出無影響。
	private static str StripMarkdownFence(str Text){
		var Trimmed = Text.Trim();
		if(!Trimmed.StartsWith("```")){
			return Trimmed;
		}
		var FirstLineEnd = Trimmed.IndexOf('\n');
		if(FirstLineEnd < 0){
			return Trimmed;
		}
		Trimmed = Trimmed[(FirstLineEnd+1)..].TrimEnd();
		if(Trimmed.EndsWith("```")){
			Trimmed = Trimmed[..^3].TrimEnd();
		}
		return Trimmed;
	}

	/// 只記錄尾部信息，避免把完整大模型輸出刷進日誌。
	private static str DescribeTextTail(str Name, str? Text){
		var Safe = Text ?? "";
		var TailLen = Math.Min(120, Safe.Length);
		var Tail = Safe[^TailLen..]
			.Replace("\r", "\\r")
			.Replace("\n", "\\n");
		return $"{Name}.Length={Safe.Length}; {Name}.Tail={Tail}";
	}

	/// 記錄最終 DTO 階段的 Descrs 末項，用於確認是映射前還是映射後出問題。
	private static str DescribeDescrsFromResp(IRespLlmDict? Resp){
		if(Resp is null){
			return "RespDescrs.NullResp";
		}
		if(Resp.Descrs is null || Resp.Descrs.Count == 0){
			return "RespDescrs.Empty";
		}
		return DescribeTextTail("RespDescrs.Last", Resp.Descrs[^1]);
	}
}
