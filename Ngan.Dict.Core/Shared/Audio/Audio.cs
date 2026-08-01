namespace Ngaq.Core.Shared.Audio;
public class Audio{
	public Audio(Func<CT, Task<Stream>> Data, EAudioType Type){
		this.MkrData = Data;
		this.Type = Type;
	}
	public EAudioType Type{get;set;}
	
	/// 若其流潙 不可褈讀、則每次璫返新ʹ流物件
	public Func<CT, Task<Stream>> MkrData{get;set;}

}
