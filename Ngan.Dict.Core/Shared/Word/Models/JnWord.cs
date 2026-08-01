#define Impl
namespace Ngaq.Core.Shared.Word.Models;

using Ngaq.Core.Shared.User.Models.Po;
using Ngaq.Core.Shared.User.Models.Po.User;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Learn;
using Ngaq.Core.Shared.Word.Models.Po.Word;
using Ngaq.Core.Infra;
using Ngaq.Core.Infra.Errors;
using Ngaq.Core.Infra.IF;
using Ngaq.Core.Tools;
using Ngaq.Core.Tools.Algo;
using Ngaq.Core.Word.Models;
using Tsinswreng.CsTools;
using Ngaq.Core.Shared.Base.Models.Po;
using Tsinswreng.CsSql;

public partial class JnWord
	:IJnWord
	,IAgg
	//,IPoWord
	,IAppSerializable
	//,IBizCreateUpdateTime
	,I_ClassVer
{
	[Impl(typeof(IPoWord))]
	public object ShallowCloneSelf()
#if Impl
	{
		return MemberwiseClone();
	}
#endif
	[Impl]
	public static Version ClassVer{get;set;} = new Version(1,0);
	public JnWord(){}
	public JnWord(PoWord PoWord, IList<PoWordProp> Props, IList<PoWordLearn> Learns){
		Word = PoWord;
		this.Props = Props;
		this.Learns = Learns;
	}

	[Impl(typeof(IJnWord))]
	public PoWord Word{get;set;} = new PoWord();
	[Impl(typeof(IJnWord))]
	public IList<PoWordProp> Props{get;set;} = new List<PoWordProp>();
	[Impl(typeof(IJnWord))]
	public IList<PoWordLearn> Learns{get;set;} = new List<PoWordLearn>();

	public PoWordLearn AddLearn(PoWordLearn Learn){
		Learn.WordId = Word.Id.Value;
		Learns.Add(Learn);
		return Learn;
	}


	/**
	 * 以時間(修改時間優先)潙準 取差集
	 * w1有洏w2無 者
	 * @param w1 待加者
	 * @param w2 已有者
	 * @returns 未加過之prop
	 */
	//有蠹 2個Prop芝CreatedAtˋ同者 diff 一個Prop旹 diff不出 只適用于新增單詞
	[Obsolete]
	public static IList<PoWordProp> DiffPropsByTime(
		IList<PoWordProp> PropsToAdd
		,IList<PoWordProp> ExistingProps
	){
		var diff = Algo.DiffListIntoDict(
			PropsToAdd, ExistingProps
			, (e)=> e.BizUpdatedAt.IsNullOrDefault() ? e.BizCreatedAt: e.BizUpdatedAt
		);
		List<PoWordProp> ans = [];
		foreach(var (Time,Props) in diff){
			ans.AddRange(Props);
		}
		return ans;
	}

	/// //有蠹 2個Prop芝CreatedAtˋ同者 diff 一個Prop旹 diff不出 只適用于新增單詞
	[Obsolete]
	public static IList<PoWordLearn> DiffLearnsByTime(
		IList<PoWordLearn> LearnsOfNeo
		,IList<PoWordLearn> ExistingLearns
	){
		var diff = Algo.DiffListIntoDict(
			LearnsOfNeo, ExistingLearns
			,(e)=> e.Time()
		);
		return diff.SelectMany(Time_Learns=>Time_Learns.Value).ToList();
	}



}


