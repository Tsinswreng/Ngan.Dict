namespace Ngaq.Core.Shared.Word.Models.Learn_;

using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Tsinswreng.CsSql;

public partial interface I_IdWord:I_Id<IdWord>{

}

public partial interface I_Index{
	public u64? Index{get;set;}
}

public partial interface I_Weight{
	public f64? Weight{get;set;}
}

