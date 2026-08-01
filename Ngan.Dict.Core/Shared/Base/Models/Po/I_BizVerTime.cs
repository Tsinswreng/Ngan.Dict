namespace Ngaq.Core.Shared.Base.Models.Po;

using Tsinswreng.CsTempus;

public partial interface I_BizTimeVer{

/// 業務版本時間戳 非樂觀鎖ʹ版本

	//[Impl(typeof(I_BizTimeVer))]
	public UnixMs BizTimeVer{get;set;}
}
