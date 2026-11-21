namespace Ngan.Dict.Core.Shared.Dictionary.Models;

/// 查詢類型。
/// 它決定兩件事：後端挑哪套系統提示詞、前端結果區用哪種卡片呈現。
/// 判定只用輸入文本的形狀，不需要模型參與；模型回包裏的 `Kind` 可用來校正。
public enum EQueryKind{
	/// 單詞或短語：走詞典路徑（詞頭＋讀音＋釋義）。
	Word = 0,
	/// 整句：走翻譯路徑（原文＋譯文，可帶逐句對照）。
	Sentence = 1,
	/// 整段（含換行）：走翻譯路徑，按段落保留換行。
	Paragraph = 2,
}

/// 查詢類型的判定規則。
/// *為何放在共享層*：前端要用它選卡片、後端要用它選提示詞；
/// 兩邊各寫一份遲早會走鐘，故收在 Core。
public static class ToolQueryKind{
	/// 句末標點：出現任一個就當作「句」。
	/// 中英日混排都覆蓋，避免只認 ASCII 標點。
	static readonly char[] SentenceEnds = ['.', '!', '?', '。', '！', '？', '…', '；', ';'];

	/// 超過這個長度（字符數）就當作「段」，即使沒有換行。
	/// 中文一句很少超過 80 字；超過通常是貼了一段。
	public const i32 ParagraphLengthThreshold = 80;

	/// 依輸入文本判定查詢類型。
	/// 規則刻意保守：拿不準就回 `Word`（詞典路徑最不容易出錯），
	/// 真的判錯時用戶仍可看到模型回包的 `Kind` 與選錯的卡片，再調整輸入。
	/// <param name="Text">用戶輸入的原始文本。</param>
	/// <returns>判定出的查詢類型。</returns>
	public static EQueryKind Detect(str? Text){
		var T = (Text ?? "").Trim();
		if(T.Length == 0){
			return EQueryKind.Word;
		}
		// 有換行 → 一定是段落。
		if(T.Contains('\n') || T.Contains('\r')){
			return EQueryKind.Paragraph;
		}
		// 很長、又含句末標點 → 段落；很長但沒有標點 → 仍按句子處理（可能是無標點的口語）。
		if(T.Length > ParagraphLengthThreshold){
			return EQueryKind.Paragraph;
		}
		foreach(var Ch in T){
			if(Array.IndexOf(SentenceEnds, Ch) >= 0){
				return EQueryKind.Sentence;
			}
		}
		return EQueryKind.Word;
	}
}
