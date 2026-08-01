using Ngaq.Core.Shared.Base.Models.Po;

namespace Ngaq.Core.Shared.Word.Svc;

public partial interface I_ModelToNewestVer<TModel>
	where TModel: I_BizTimeVer
{
	public TModel OldVerDictToNewestVerModel(
		IDictionary<str, object?> OldVerDict
	);
}
