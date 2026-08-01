namespace Ngaq.Ui.Views.Word.WordCard;

using System;

public enum EWordCardPronounceStatus{
	Played = 0,
	NoWordSelected = 1,
	WordLangEmpty = 2,
	ServiceUnavailable = 3,
	UserLangNotMapped = 4,
	Failed = 5,
}

/// 朗讀動作結果。由 VM 返回，供 View 層決定提示與導航。
public class DtoWordCardPronounceResult{
	public EWordCardPronounceStatus Status{get;set;} = EWordCardPronounceStatus.Failed;
	public str WordLang{get;set;} = "";
	public Exception? Error{get;set;} = null;
}
