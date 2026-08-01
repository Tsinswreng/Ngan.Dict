namespace Ngaq.Core.Shared.User.Models.Po.RefreshToken;

using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.User.Models.Bo.Device;
using Ngaq.Core.Shared.User.Models.Bo.Jwt;
using Ngaq.Core.Shared.User.Models.Po.Device;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Model.Sys.Po.RefreshToken;
using Ngaq.Core.Models.Po;
using System.Security.Cryptography;
using System.Text;
using Tsinswreng.CsTempus;

/// 刷新令牌
public class PoRefreshToken
	:PoBaseBizTime
	,AppI_Id<IdRefreshToken>
{


	public enum ETokenValueType{
		TokenSha256,
		Jti
	}

	public IdRefreshToken Id{get;set;} = new ();
	public IdUser UserId{get;set;}
	public ETokenValueType TokenValueType{get;set;}
	public u8[]? TokenValue{get;set;}

	/// 客戶端生成後持久化ᵈ存
	/// 如 同一用戶可能同時在 手機與電腦登入/登出

	public IdClient ClientId{get;set;}
	public EClientType ClientType{get;set;} = EClientType.Unknown;
	public UnixMs ExpireAt{get;set;}
	public UnixMs RevokeAt{get;set;}
	public str? RevokeReason{get;set;}
	public str? IpAddr{get;set;}
	public UnixMs LastUsedAt{get;set;}

	/// 保留備用。萬一以後要給第三方限制介面範圍，可直接沿用 OAuth2 scope 思維。

	public str? Scope{get;set;}
	public str? UserAgent{get;set;}
}

public static class ExtnPoRefreshToken{
	public static TSelf SetTokenValueSha256<TSelf>(
		this TSelf z
		,str TokenStr
	)where TSelf:PoRefreshToken {
		z.TokenValueType = PoRefreshToken.ETokenValueType.TokenSha256;
		z.TokenValue = SHA256.HashData(Encoding.UTF8.GetBytes(TokenStr));
		return z;
	}
}
