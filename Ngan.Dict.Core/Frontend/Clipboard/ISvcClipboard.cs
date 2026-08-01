namespace Ngan.Dict.Core.Frontend.Clipboard;

public interface ISvcClipboard{
	[Doc(@$"
	#Rtn[null when clipboard content is not string]
	")]
	public Task<str?> GetText(CT Ct);
}
