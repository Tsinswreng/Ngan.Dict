namespace Ngaq.Ui.Views.Word.WordManage.SearchWords.SearchedWordCard;

using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Dto;
using Ngaq.Core.Shared.Word.Models.Learn_;
using Ngaq.Ui.Views.Word.WordCard;
using Ctx = VmSearchedWordCard;

public partial class VmSearchedWordCard{
	public static partial JnWord GetJnWordFromHit(DtoWordSearchHit Hit){return Hit.JnWord;}
	public partial Ctx FromSearchHit(DtoWordSearchHit Hit){
		SearchHit = Hit;
		WordForLearn = new WordForLearn(GetJnWordFromHit(Hit));
		Init();
		return this;
	}
	protected override partial nil Init(){
		try{
			if(Bo is null){return NIL;}
			DelAt = Bo.DelAt; Head = Bo.Head; Lang = Bo.Lang; Index = Bo.Index; Weight = Bo.Weight;
			Learn_Records = Bo.Learn_Records; SavedLearnRecords = Bo.LearnRecords; LastLearnedTime = Bo.LastLearnedTime_();
			HitKindText = FmtHitKind(SearchHit?.HitKind); HitAssetIdText = FmtHitAssetId(SearchHit);
			if(Learn_Records.TryGetValue(ELearn.Add, out var AddRecords)){FontColor = AddCntToFontColor((u64)AddRecords.Count);}
		}catch(Exception E){HandleErr(E);}
		return NIL;
	}
	private static partial str FmtHitKind(EWordSearchHitKind? Kind){return Kind switch{EWordSearchHitKind.Word => "Word", EWordSearchHitKind.WordProp => "Prop", EWordSearchHitKind.WordLearn => "Learn", _ => "",};}
	private static partial str FmtHitAssetId(DtoWordSearchHit? Hit){
		if(Hit is null){return "";}
		if(Hit.HitKind == EWordSearchHitKind.WordProp){return Hit.WordProp?.Id.ToString() ?? "";}
		if(Hit.HitKind == EWordSearchHitKind.WordLearn){return Hit.WordLearn?.Id.ToString() ?? "";}
		return Hit.JnWord.Word.Id.ToString();
	}
}
