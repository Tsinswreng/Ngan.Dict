using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Learn;

namespace Ngaq.Core.Word.Models.Dto;

public partial struct DtoWordDiff{
	public IList<PoWordProp> PoWordProps{get;set;}
	public IList<PoWordLearn> PoWordLearns{get;set;}
}
