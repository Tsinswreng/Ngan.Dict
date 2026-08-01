namespace Ngaq.Ui.Views.Word.WordManage.SearchWords.SearchedWordCard;

using System.Collections.ObjectModel;
using Ngaq.Core.Shared.User.Models.Po;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Ui.Views.Word.WordCard;
using Ctx = VmSearchedWordCard;

/// 將搜索命中轉成可供詞卡顯示的 ViewModel，同時保留命中來源。
public partial class VmSearchedWordCard: VmBaseWordListCard{
	/// 調試環境中的卡片樣例集合。
	public new static ObservableCollection<Ctx> Samples = [];
	/// 原始搜索命中。
	public DtoWordSearchHit? SearchHit{get;set;}
	/// 命中資產類型的顯示文字。
	public str HitKindText{get;set;} = "";
	/// 命中資產識別碼的顯示文字。
	public str HitAssetIdText{get;set;} = "";
	/// 聚合根的軟刪時間，用於詞卡呈現刪除狀態。
	public IdDel? DelAt{get;set;}
	/// 從搜索命中取出所屬的完整單詞聚合。
	public static partial JnWord GetJnWordFromHit(DtoWordSearchHit Hit);
	/// 載入命中資料並更新所有詞卡展示屬性。
	public partial Ctx FromSearchHit(DtoWordSearchHit Hit);
	/// 將基類資料映射為搜索結果卡片的展示狀態。
	protected override partial nil Init();
	/// 將命中類型轉為搜索卡片使用的簡短文字。
	private static partial str FmtHitKind(EWordSearchHitKind? Kind);
	/// 依命中類型取得對應聚合根或資產的識別碼文字。
	private static partial str FmtHitAssetId(DtoWordSearchHit? Hit);
}
