namespace Ngaq.Core.Infra.Errors;
public static class AppErrTags{
	/// 業務異常 如參數不合法
	public static str BizErr = nameof(BizErr);
	/// 系統異常 如 數據庫異常
	public static str SysErr = nameof(SysErr);
	/// 公開、可示予用戶
	public static str Public = nameof(Public);
	/// 叵示予用戶
	public static str Private = nameof(Private);

}

