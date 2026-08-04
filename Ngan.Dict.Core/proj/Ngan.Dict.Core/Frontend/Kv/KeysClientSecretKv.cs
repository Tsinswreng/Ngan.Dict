using Ngan.Dict.Core.Tools;

namespace Ngan.Dict.Core.Frontend.Kv;

using static Ngan.Dict.Core.Tools.Url;

public partial class KeysClientSecretKv{
	///臨時存。後宜移至系統級Essential
	public static Url AesSecret = Mk(null, [nameof(AesSecret)]);
}
