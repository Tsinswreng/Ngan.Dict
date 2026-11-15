// 從 Tsinswreng.AvlnTools 原樣搬入（該庫尚未遷入 TswgLib）。
using System.Collections.Generic;

namespace Ngan.Dict.Ui.Infra.AvlnSugar;

public partial interface IDict_Key<K,V>
	where K : notnull
{
	public IDictionary<K, V> Dict { get; set; }
	public K Key { get; set; }
}


public  partial class Dict_Key<K,V>
	where K : notnull
{
	public Dict_Key(IDictionary<K, V> Dict, K Key){
		this.Dict = Dict;
		this.Key = Key;
	}
	public IDictionary<K, V> Dict { get; set; } = new Dictionary<K, V>();
	public K Key { get; set; }
}
