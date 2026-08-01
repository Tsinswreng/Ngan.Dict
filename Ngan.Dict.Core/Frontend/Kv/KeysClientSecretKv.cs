using Ngaq.Core.Tools;

namespace Ngaq.Core.Frontend.Kv;

using static Ngaq.Core.Tools.Url;

public partial class KeysClientSecretKv{
	///臨時存。後宜移至系統級Essential
	public static Url AesSecret = Mk(null, [nameof(AesSecret)]);
}
