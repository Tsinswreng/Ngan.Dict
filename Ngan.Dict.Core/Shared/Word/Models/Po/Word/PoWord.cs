#define Impl
namespace Ngaq.Core.Shared.Word.Models.Po.Word;

using Ngaq.Core.Shared.Base.Models.Po;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Infra;
using Tsinswreng.CsSql;
using Tsinswreng.Srefl;
using Tsinswreng.CsTempus;

[Doc(@$"
同Owner下 ({nameof(Head)}, {nameof(Lang)}) 纔是一詞ʹ 理則ʸʹ 唯一標識、洏非Id
(如異ʹ節點蜮在同步前皆各新增一詞芝有同ʹ({nameof(Head)}, {nameof(Lang)})、則雖同ʹ詞、猶將被予異ʹId)


{nameof(PoWord.BizCreatedAt)}可能變早。
當從生詞表加詞時、生詞表中記錄的時間比數據庫中已有的同個詞的{nameof(PoWord.BizCreatedAt)}更早
再要以更早者爲準、用于算權重。
{nameof(PoWord.BizCreatedAt)}可能變早時 {nameof(BizUpdatedAt)}一定會變。
")]
public partial interface IPoWord
	:IPoBase
	,I_Id<IdWord>
	,IAggRoot
	,IHeadLangWord
	,IBizCreateUpdateTime
	,I_Owner
{
	/// 最早ʹ入庫ʹ時 作增量同步ʹʃ依
	/// 如某詞ʹCreatedAt早于LastUploadToRemoteTime
	/// 辨: CreatedAt: 詞ˋ始被錄于文本單詞表之時
	/// DbCreatedAt: 實體插入數據庫旹
	/// StoredAt: 詞ˋ始被錄入數據庫旹
	/// 備份同步旹、于同一詞、同步後本地與遠端之StoredAt當一致、DbCreatedAt可不一致
	public UnixMs StoredAt{get;set;}
}

public partial class PoWord
	:PoBaseBizTime
	,I_Id<IdWord>
	,IHeadLangWord
	,IPoWord
{
	public static PoWord Example{get;set;} = new PoWord();
/// 僅用于臨時標識、詞ʹidˋ恐變
/// 欲做持久ʹ標識、宜用(Owner, Head, Lang)
/// 備份同步㕥合併同ʹ詞旹、當按詞頭洏非id㕥判兩詞是否潙同一詞、緣縱潙同ʹ詞、本地ʹ庫ʸ與遠端ᐪʹid恐不一
	public IdWord Id {get;set;} = new IdWord(); //不顯式調用構造器則內ʹValue 得零

	public IdUser Owner{get;set;}

	#region IPoWord

	/// 詞形標識
	[Doc(@$"
	字段爲用戶自填、未必對應「語言」、實則爲詞庫隔離之標識
	今ʹ理則ˋ 同{nameof(Owner)} 同{nameof(Lang)}時 {nameof(Head)}必唯一
	")]
	public str Head{get;set;}
	#if Impl
		="";
	#endif


	public str Lang{get;set;}
	#if Impl
		="";
#endif

	#endregion IPoWord

	
	/// 用于詞庫同步旹 比對㕥篩改˪ʹ詞
	[Doc(@$"最早入庫之時間。
	辨: {nameof(BizCreatedAt)} 爲 最早創建時間
	(比如被記于文本詞表之時。先記在生詞表上再導入、則其{nameof(BizCreatedAt)}被視爲文本生詞表上記錄的時間。)
	目前只在 DaoWord.FnPageChangedWordIdsWithDelWordsAfterTime中被讀取、
	")]
	public UnixMs StoredAt{get;set;} = UnixMs.Now();

	public override string ToString() {
		var Mgr = (IPropAccessorReg)CoreDictMapper.Inst;
		if(!Mgr.Type_PropAccessor.TryGetValue(typeof(PoWord), out var Accessor)){
			throw new Exception($"No {nameof(IPropAccessor)} registered for type: {typeof(PoWord)}");
		}
		var Dict = new Dictionary<str, obj?>();
		foreach(var Key in Accessor.GetGetterNames(this)){
			if(!Accessor.TryGet(this, Key, out var V)){
				continue;
			}
			Dict[Key] = V;
		}
		return ExtnIDict.Print(Dict);
	}

	[Obsolete]
	public class N{
		[Obsolete]
		public str Id = nameof(PoWord.Id);
		[Obsolete]
		public str Head = nameof(PoWord.Head);
		[Obsolete]
		public str Lang = nameof(PoWord.Lang);
		[Obsolete]
		public str Owner = nameof(PoWord.Owner);
	}


}
