using Ngan.Dict.Core.Shared.Dictionary.Models;
using Ngan.Dict.Core.Shared.Dictionary.Models.Po.NormLang;

namespace Ngan.Dict.Core.Shared.Dictionary.Svc;

[Doc(@$"文本轉語言服務")]
public interface ISvcTts{
	
	[Doc(@$"最好要帶緩存")]
	public Task<Audio.Audio> GetAudio(
		str Text, INormLang Lang
	);
}
