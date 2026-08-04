using Ngan.Dict.Core.Shared.Word.Models.Learn_;
using Ngan.Dict.Core.Shared.Word.Models.Po.Learn;
using Ngan.Dict.Core.Shared.Word.Models.Po.Word;

namespace Ngan.Dict.Core.Shared.Word.Models.Dto;

public partial struct WordId_PoLearns{
	public IdWord WordId;
	public IEnumerable<PoWordLearn> PoLearns;
}

public partial struct WordId_LearnRecords{
	public IdWord WordId;
	public IEnumerable<ILearnRecord> LearnRecords;
}
