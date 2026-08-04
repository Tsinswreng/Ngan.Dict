using Ngan.Dict.Core.Shared.Word.Models;
using Tsinswreng.CsTools;

namespace Ngan.Dict.Core.Word.Models.Dto;

public struct DtoJnWordEtAsset{
	public JnWord JnWord{get;set;}
	public obj Asset{get;set;}//PoWordProp, PoWordLearn
}
