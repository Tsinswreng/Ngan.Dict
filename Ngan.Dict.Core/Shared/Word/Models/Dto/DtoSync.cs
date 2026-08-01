namespace Ngaq.Core.Shared.Word.Models.Dto;

using Ngaq.Core.Shared.Base.Models;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Learn;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Tsinswreng.CsTools;

public class DtoSyncTwoWords{
	public PoWord? ChangedPoWord{get;set;}

	public Neo_Changed<PoWordProp> NeoOrChangedProps{get;set;} = new();
	public Neo_Changed<PoWordLearn> NeoOrChangedLearns{get;set;} = new();

}


public static class ExtnDtoSyncTwoWords{
	public static DtoSyncWords ToDtoSyncWords(
		this DtoSyncTwoWords z
		,IJnWord WordInDb
	){
		var R = new DtoSyncWords();

		R.UpdatedWords.Add(new DtoUdpWord{
			WordInDb = WordInDb,
			DtoSyncTwoWords = z
		});
		return R;
	}


	[Obsolete]
	public static DtoSyncWordsOld2 ToDtoSyncWordsOld(
		this DtoSyncTwoWords z
		,IJnWord WordInDb
	){
		var R = new DtoSyncWordsOld2();
		var NeoPart = new JnWord{Word=z.ChangedPoWord??WordInDb.Word};
		NeoPart.Props.AddRange(z.NeoOrChangedProps.NeoPart??[]);
		NeoPart.Learns.AddRange(z.NeoOrChangedLearns.NeoPart??[]);

		var ChangedPart = new JnWord{Word=z.ChangedPoWord??WordInDb.Word};
		ChangedPart.Props.AddRange(z.NeoOrChangedProps.ChangedPart??[]);
		ChangedPart.Learns.AddRange(z.NeoOrChangedLearns.ChangedPart??[]);
		var dtoUpd = new DtoUpdWordOld2(WordInDb, NeoPart, ChangedPart);
		R.UpdatedWords.Add(dtoUpd);
		return R;
	}
}
