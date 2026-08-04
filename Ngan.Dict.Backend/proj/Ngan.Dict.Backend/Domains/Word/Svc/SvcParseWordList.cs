namespace Ngan.Dict.Backend.Domains.Word.Svc;

using System.Text;
using Ngan.Dict.Core.Shared.Word.Models;
using Ngan.Dict.Core.Infra.Errors;
using Ngan.Dict.Core.Service.Parser;
using Ngan.Dict.Core.Iter;
using Ngan.Dict.Core.Tools.Io;
using Ngan.Dict.Core.Shared.Word.Svc;

public partial class SvcParseWordList
	: ISvcParseWordList
{

	public async Task<IEnumerable<JnWord>> ParseWordsByIterEtEncoding(
		IIterable<u8> Iter
		,Encoding Encoding
		,CT Ct = default
	){
		//I_Answer<IEnumerable<Bo_Word>> ans = new Answer<IEnumerable<Bo_Word>>();
		var Parser = new WordListParser(Iter);
		Parser.Encoding = Encoding;
		var DateBlocks = Parser.Parse();
		var metadata = Parser.Status.Metadata;
		if (metadata == null) {
			throw new Exception("Metadata is null");//TODO i18n
		}
		var JnWords = ParseResultMapper.Inst.Map(metadata, DateBlocks);
		return JnWords;
	}


	public async Task<IEnumerable<JnWord>> ParseWordsFromFilePath(
		Path_Encode Path_Encode
		, CT ct = default
	) {
		IIterable<u8> ByteReader = new ByteReader(Path_Encode.Path);
		return await ParseWordsByIterEtEncoding(ByteReader, Path_Encode.Encoding);
	}

	public async Task<IEnumerable<JnWord>> ParseWordsFromText(
		string Text
		, CT ct = default
	) {
		var ByteReader = new StrByteReader(Text);
		return await ParseWordsByIterEtEncoding(ByteReader, Encoding.UTF8);
	}

}
