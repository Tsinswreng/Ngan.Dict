namespace Ngan.Dict.Core.Backend;
using K = Ngan.Dict.Core.Tools.RedisKey;
using static Ngan.Dict.Core.Tools.RedisKey;
using Ngan.Dict.Core.Shared.User.Models.Po.User;

[Doc("Keys for cache(redis)")]
public class KeysCache{
	public K _Refresh = Mk(null, ["Refresh"]);
	public class Refresh{
		// public K Key(IdUser UserId){

		// }
	}
}
