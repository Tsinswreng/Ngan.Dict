using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Shared.Word.Models.Po.Kv;
using Ngaq.Core.Shared.Word.Models.Po.Learn;

namespace Ngaq.Core.Shared.Word.Models.Dto;

/// 搜索命中類型。
/// 用于區分本次命中的是單詞聚合根、單詞屬性還是學習記錄。
public enum EWordSearchHitKind{
	/// 直接命中單詞本身。
	/// 常見于按詞頭前綴搜索或按單詞 Id 精確命中。
	Word = 0,

	/// 命中某條單詞屬性。
	WordProp = 1,

	/// 命中某條單詞學習記錄。
	WordLearn = 2,
}

/// 單詞搜索命中結果。
/// 無論命中的是單詞本身還是其資產，都會攜帶所屬 <see cref="JnWord"/>，
/// 以便調用方同時拿到聚合上下文與精確命中信息。
public class DtoWordSearchHit{
	/// 命中結果所屬的完整單詞聚合。
	public JnWord JnWord { get; set; } = new();

	/// 本次命中的類型。
	public EWordSearchHitKind HitKind { get; set; } = EWordSearchHitKind.Word;

	/// 當 <see cref="HitKind"/> 爲 <see cref="EWordSearchHitKind.WordProp"/> 時，
	/// 這裏保存精確命中的屬性；其他情況下應爲 null。
	public PoWordProp? WordProp { get; set; }

	/// 當 <see cref="HitKind"/> 爲 <see cref="EWordSearchHitKind.WordLearn"/> 時，
	/// 這裏保存精確命中的學習記錄；其他情況下應爲 null。
	public PoWordLearn? WordLearn { get; set; }
}
