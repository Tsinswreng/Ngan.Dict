using Ngan.Dict.Core.Shared.Word.Models.Po.Kv;
using Ngan.Dict.Core.Shared.Word.Models.Po.Learn;

namespace Ngan.Dict.Core.Word.Models.Dto;

public partial struct DtoWordDiff{
	public IList<PoWordProp> PoWordProps{get;set;}
	public IList<PoWordLearn> PoWordLearns{get;set;}
}
