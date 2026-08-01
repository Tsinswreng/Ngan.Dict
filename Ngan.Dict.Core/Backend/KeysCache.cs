namespace Ngaq.Core.Backend;
using K = Ngaq.Core.Tools.RedisKey;
using static Ngaq.Core.Tools.RedisKey;
using Ngaq.Core.Shared.User.Models.Po.User;

[Doc("Keys for cache(redis)")]
public class KeysCache{
	public K _Refresh = Mk(null, ["Refresh"]);
	public class Refresh{
		// public K Key(IdUser UserId){

		// }
	}
}
