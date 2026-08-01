using Avalonia.Media;
using Tsinswreng.CsCore;

namespace Ngaq.Ui.Views.Word.WordCard;

/// 單詞卡片頁面契約。
/// 面向用戶可觀察/可操作語義，不暴露具體控件類型。
public interface IViewWordListCard {
	/// 卡片序號文本。
	public str? IndexText { get; }

	/// 單詞語言文本。
	public str? LangText { get; }

	/// 單詞詞頭文本。
	public str? HeadText { get; }

	/// 學習歷史摘要文本。
	public str? LearnHistoryText { get; }

	/// 最近一次學習時間文本。
	public str? LastLearnedTimeText { get; }

	/// 權重文本。
	public str? WeightText { get; }

	[Doc(@$"詞頭文ʹ色
	添加次數	詞頭文色
	1	白
	2	綠
	3	藍
	4	黃
	5	紫
	>=6	紅
	")]
	public IBrush? HeadFontColor{get;}

	[Doc(@$"單詞卡最左豎邊框之色
	此輪中
	未學時潙透明
	記得時潙綠
	忘時潙紅
	")]
	public IBrush? LearnedColor{get;}

	[Doc(@$"點擊單詞卡片")]
	public Task<nil> Click(CT Ct);


}
