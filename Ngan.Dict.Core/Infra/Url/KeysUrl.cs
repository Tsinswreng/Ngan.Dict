namespace Ngaq.Core.Infra.Url;

using Ngaq.Core.Tools;
using static Ngaq.Core.Tools.Url;


[Doc(@$"http api url鍵定義。
約定
- `Open/`開頭的不用驗令牌;
- `Api/`開頭的要驗令牌;
")]
public partial class KeysUrl{
	public static Url OpenV1 = Mk(null, ["Open", "V1"]);
	public static Url ApiV1 = Mk(null, ["Api", "V1"]);
	public static Url ApiV2 = Mk(null, ["Api", "V2"]);

	//勿作深層內部類。䀬ʹ子ʹApi枚舉內部類只置于ConstUrlʹ直ʹ子層
	public partial class OpenUser{
		//public static Url _Root = "";
		public static Url _R = Mk(OpenV1, ["User"]);
		public static Url Login = Mk(_R, [nameof(Login)]);
		public static Url AddUser = Mk(_R, [nameof(AddUser)]);
		public static Url TokenRefresh = Mk(_R, ["Token", "Refresh"]);
	}

	public partial class ApiUser{
		public static Url _R = Mk(ApiV1, ["User"]);
		public static Url Logout = Mk(_R, [nameof(Logout)]);
	}


	public partial class Word{
		public static Url _R = Mk(ApiV1, [nameof(Word)]);
		public static Url Push = Mk(_R, [nameof(Push)]);
		public static Url Pull = Mk(_R, [nameof(Pull)]);
	}

	public partial class WordV2{
		public static Url _R = Mk(ApiV2, ["Word"]);
		public static Url Push = Mk(_R, [nameof(Push)]);
		public static Url Pull = Mk(_R, [nameof(Pull)]);
		public static Url BatSyncJnWordByBizId = Mk(_R, [nameof(BatSyncJnWordByBizId)]);
		public static Url BatAddJnWord = Mk(_R, [nameof(BatAddJnWord)]);
		public static Url GetAllWordsWithDel = Mk(_R, [nameof(GetAllWordsWithDel)]);
		public static Url BatUpdHeadLang = Mk(_R, [nameof(BatUpdHeadLang)]);
		public static Url SoftDelJnWordInId = Mk(_R, [nameof(SoftDelJnWordInId)]);
	}

}

