namespace Ngaq.Ui.Tools;

using Ngaq.Core.Infra;

/// 文本显示工具：集中放置各页面共用的字符串格式化逻辑。
public static class ToolText{
	/// 长文本略缩显示：保留头尾，中间用 `..` 代替，避免表格列被超长内容撑坏。
	/// <param name="Text">原始文本；会先做 `Trim`。</param>
	/// <param name="HeadLen">头部保留字符数。</param>
	/// <param name="TailLen">尾部保留字符数。</param>
	/// <param name="EmptyText">空文本时的回退显示。</param>
	/// <returns>适合在列表/卡片中展示的短文本。</returns>
	public static str FormatCompactText(str? Text, int HeadLen = 10, int TailLen = 6, str EmptyText = "-"){
		var raw = Text?.Trim() ?? "";
		if(str.IsNullOrWhiteSpace(raw)){
			return EmptyText;
		}
		var minLen = HeadLen + TailLen + 2;
		if(raw.Length <= minLen){
			return raw;
		}
		return $"{raw[..HeadLen]}..{raw[^TailLen..]}";
	}
}
