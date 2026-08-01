namespace Ngaq.Ui.Views.Dictionary.NormLangTagEdit;

using System.Collections.ObjectModel;
using Ngaq.Core.Shared.Dictionary.Models.Po.NormLang;
using Ngaq.Ui.Infra;
using Ngaq.Ui.Views.Dictionary.NormLangTag;
using Tsinswreng.CsCfg;
using Ctx = VmNormLangTagEdit;

/// 編輯詞典源語言快捷標籤的順序、自定義短文字與成員集合。
/// 編輯期間操作副本，只有保存成功後才通知詞典頁替換現有快捷欄。
public partial class VmNormLangTagEdit: ViewModelBase, IMk<Ctx>{
	/// 建立使用應用配置文件的編輯 ViewModel。
	protected partial VmNormLangTagEdit();
	/// 建立使用應用配置文件的編輯 ViewModel。
	public static partial Ctx Mk();
	/// 注入可替換的配置存取器，便於測試配置讀寫。
	public partial VmNormLangTagEdit(ICfgAccessor? Cfg);

	/// 本次編輯中的快捷標籤副本；集合順序即最終顯示順序。
	public ObservableCollection<VmNormLangTag> Tags{get;set;} = [];
	/// 保存成功後回傳已保存標籤的回調。
	Action<IList<VmNormLangTag>>? OnSaved;
	/// 本地配置存取器。
	ICfgAccessor Cfg = default!;

	/// 從詞典頁當前快捷標籤建立可獨立修改的編輯副本。
	public partial nil FromTags(IEnumerable<VmNormLangTag> Tags);
	/// 設置保存完成後更新詞典頁快捷欄的回調。
	public partial nil SetOnSaved(Action<IList<VmNormLangTag>> FnOnSaved);
	/// 將選中的標準語言追加為快捷標籤；重複語言不再次加入。
	public partial nil Add(PoNormLang Po);
	/// 將指定標籤向前移動一位；已位於首項時不變。
	public partial nil MoveUp(VmNormLangTag Tag);
	/// 將指定標籤向後移動一位；已位於末項時不變。
	public partial nil MoveDown(VmNormLangTag Tag);
	/// 從編輯副本刪除指定標籤；允許刪至空集合。
	public partial nil Remove(VmNormLangTag Tag);
	/// 保存當前順序與短文字，成功後通知詞典頁更新快捷欄。
	public partial Task<nil> Save(CT Ct);
	/// 深拷貝標籤，避免編輯頁直接改動詞典頁仍在展示的實例。
	private static partial VmNormLangTag CloneTag(VmNormLangTag Tag);
	/// 判斷編輯集合中是否已存在同一 Type 與 Code 的語言。
	private partial bool Contains(PoNormLang Po);
}
