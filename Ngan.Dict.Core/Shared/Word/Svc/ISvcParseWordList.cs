namespace Ngaq.Core.Shared.Word.Svc;

using System.Text;
using Ngaq.Core.Shared.Word.Models;
using Ngaq.Core.Model;
using Ngaq.Core.Iter;
using Ngaq.Core.Tools.Io;


/// 從 路經/字串/字節生產者 解析單詞表文本、得Bo_Word列表。
public partial interface ISvcParseWordList{

	// Task<I_Answer<IList<Bo_Word>>> ParseWordsByIterEtEncoding(
	// 	I_I
	// );

	Task<IEnumerable<JnWord>> ParseWordsFromFilePath(
		Path_Encode Path_Encode
		, CT ct = default
	);

	Task<IEnumerable<JnWord>> ParseWordsFromText(
		str Text
		, CT ct = default
	);

	Task<IEnumerable<JnWord>> ParseWordsByIterEtEncoding(
		IIterable<u8> Iter
		,Encoding Encoding
		, CT ct = default
	);

}
