namespace Ngaq.Core.Shared.Audio;

public interface IAudioPlayer{
	public Task<IPlayState?> Play(Stream S, EAudioType Type, CT Ct);
}

public class FakeAudioPlayer : IAudioPlayer{
	public async Task<IPlayState?> Play(Stream S, EAudioType Type, CT Ct){
		return null;
	}
}
