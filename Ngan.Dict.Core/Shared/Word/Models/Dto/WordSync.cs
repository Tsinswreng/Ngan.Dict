using Ngaq.Core.Shared.Sync;
using Ngaq.Core.Shared.Word.Models.Po.Word;

namespace Ngaq.Core.Shared.Word.Models.Dto;



[Doc(@$"一對{nameof(JnWord)}同步結果。
Remote合入Local。
#See[{nameof(AggDiffCaseForSync)}]
")]
public class DtoJnWordSyncResult:AggSyncResult<JnWord>{

}
