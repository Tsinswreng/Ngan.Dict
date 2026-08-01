using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Po.Word;

namespace Ngaq.Core.Shared.Sync;

using E = EDiffByBizIdResultForSync;
[Doc(@$"用于兩持久對象同步。
按業務層唯一標識(BizId)比較、而不是按數據庫中生成的Id。
")]
public class EDiffByBizIdResultForSync{
	public static readonly E Unknown = new();
	[Doc(@$"兩個對象都沒有變化。不用做。")]
	public static readonly E NoChange = new();
	
	[Doc(@$"常見情況")]
	public static readonly E RemoteIsNewer = new();
	
	[Doc(@$"Remote更舊、不需要改動Local。
	==true時 忽略其他字段的值 因爲已經不需操作了
	")]
	public static readonly E RemoteIsOlder = new();
	
	[Doc(@$"Remote 在 接收合入的一方 沒有對應的Local。
	直接把Remote入庫即可。
	")]
	public static readonly E LocalNotExist = new();
	
	[Doc(@$"
	Remote和Local的Id不一致、
	可能是 本來 要合併的兩個節點 根本就不存在 Remote 和 Local、
	在合併前、他們各自在不同時刻添加了 相同BizId的實體。
	于是會各自形成兩個 Id不同的對象。
	此時 理論上Local和Remote不會有重合的資產。
	Local的資產需接收Remote的資產的合入(不會)。下次Remote再從Local合入其兩端資產即可同步。
	
	合併後實體的Id 以 Id更小者 爲準。
	")]
	
	public static readonly E IdNotEqual = new();
}
