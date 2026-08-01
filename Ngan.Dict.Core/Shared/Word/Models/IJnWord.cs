namespace Ngan.Dict.Core.Shared.Word.Models;

using Ngan.Dict.Core.Infra;
using Ngan.Dict.Core.Infra.Errors;
using Ngan.Dict.Core.Infra.IF;
using Ngan.Dict.Core.Shared.User.Models.Po.User;
using Ngan.Dict.Core.Shared.Word.Models.Po.Kv;
using Ngan.Dict.Core.Shared.Word.Models.Po.Learn;
using Ngan.Dict.Core.Shared.Word.Models.Po.Word;
using Ngan.Dict.Core.Word.Models;
using Tsinswreng.CsSql;
using Tsinswreng.CsTools;


/// 嚴格對應數據庫ʹ實體ʹ聚合
/// 適用于序列化等
/// 不含字段如
/// 	[Impl]
///	public IdWord Id{
///		get{return Word.Id;}
///		set{
///			Word.Id = value;
///			AssignId();
///		}
///	}
public interface IJnWord
	:IAppSerializable
	,IAgg
//	,I_ClassVersion //使接口叶靜態抽象則json序列化生成器報錯
{
	public static readonly Version Ver = new Version(1,0);
	public PoWord Word{get;set;}
	public IList<PoWordProp> Props{get;set;}
	public IList<PoWordLearn> Learns{get;set;}
}

